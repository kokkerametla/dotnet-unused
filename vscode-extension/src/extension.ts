import * as vscode from 'vscode';
import * as path from 'path';
import * as fs from 'fs';
import { CliRunner } from './cliRunner';
import { DiagnosticProvider } from './diagnosticProvider';
import { UnusedSymbolsProvider } from './unusedSymbolsProvider';
import { UnusedCodeReport } from './types';

let outputChannel: vscode.OutputChannel;
let cliRunner: CliRunner;
let diagnosticProvider: DiagnosticProvider;
let unusedSymbolsProvider: UnusedSymbolsProvider;

export function activate(context: vscode.ExtensionContext) {
    outputChannel = vscode.window.createOutputChannel('Dotnet Unused');
    cliRunner = new CliRunner(outputChannel);
    diagnosticProvider = new DiagnosticProvider();
    unusedSymbolsProvider = new UnusedSymbolsProvider();

    vscode.window.registerTreeDataProvider('dotnet-unused.unusedSymbols', unusedSymbolsProvider);

    checkCliInstallation();

    context.subscriptions.push(
        vscode.commands.registerCommand('dotnet-unused.analyzeWorkspace', analyzeWorkspace)
    );

    context.subscriptions.push(
        vscode.commands.registerCommand('dotnet-unused.analyzeCurrentFile', analyzeCurrentFile)
    );

    context.subscriptions.push(
        vscode.commands.registerCommand('dotnet-unused.clearDiagnostics', () => {
            diagnosticProvider.clear();
            unusedSymbolsProvider.refresh(null);
            vscode.window.showInformationMessage('Cleared unused code diagnostics');
        })
    );

    context.subscriptions.push(
        vscode.commands.registerCommand('dotnet-unused.exportReport', exportReport)
    );

    context.subscriptions.push(
        vscode.commands.registerCommand('dotnet-unused.fixUnusedUsings', fixUnusedUsings)
    );

    const config = vscode.workspace.getConfiguration('dotnet-unused');
    if (config.get<boolean>('autoRunOnSave')) {
        context.subscriptions.push(
            vscode.workspace.onDidSaveTextDocument((document) => {
                if (document.languageId === 'csharp') {
                    analyzeCurrentFile();
                }
            })
        );
    }

    context.subscriptions.push(diagnosticProvider);
    context.subscriptions.push(outputChannel);
}

async function checkCliInstallation() {
    const available = await cliRunner.checkCliAvailable();
    if (!available) {
        const action = await vscode.window.showWarningMessage(
            'dotnet-unused CLI tool not found. Please install it to use this extension.',
            'Install Now',
            'Instructions'
        );

        if (action === 'Install Now') {
            const terminal = vscode.window.createTerminal('Install dotnet-unused');
            terminal.sendText('dotnet tool install --global DotnetUnused');
            terminal.show();
        } else if (action === 'Instructions') {
            vscode.env.openExternal(vscode.Uri.parse('https://github.com/kokkerametla/dotnet-unused#installation'));
        }
    }
}

async function analyzeWorkspace() {
    const workspaceFolders = vscode.workspace.workspaceFolders;
    if (!workspaceFolders) {
        vscode.window.showErrorMessage('No workspace folder open');
        return;
    }

    const slnFiles = await vscode.workspace.findFiles('**/*.sln', '**/node_modules/**', 10);

    let targetPath: string;
    if (slnFiles.length === 0) {
        const csprojFiles = await vscode.workspace.findFiles('**/*.csproj', '**/node_modules/**', 10);
        if (csprojFiles.length === 0) {
            vscode.window.showErrorMessage('No .sln or .csproj files found in workspace');
            return;
        }

        if (csprojFiles.length === 1) {
            targetPath = csprojFiles[0].fsPath;
        } else {
            const selected = await vscode.window.showQuickPick(
                csprojFiles.map(f => ({ label: path.basename(f.fsPath), path: f.fsPath })),
                { placeHolder: 'Select a .csproj file to analyze' }
            );
            if (!selected) {
                return;
            }
            targetPath = selected.path;
        }
    } else {
        if (slnFiles.length === 1) {
            targetPath = slnFiles[0].fsPath;
        } else {
            const selected = await vscode.window.showQuickPick(
                slnFiles.map(f => ({ label: path.basename(f.fsPath), path: f.fsPath })),
                { placeHolder: 'Select a .sln file to analyze' }
            );
            if (!selected) {
                return;
            }
            targetPath = selected.path;
        }
    }

    await runAnalysis(targetPath, 'workspace');
}

async function analyzeCurrentFile() {
    const editor = vscode.window.activeTextEditor;
    if (!editor || editor.document.languageId !== 'csharp') {
        vscode.window.showErrorMessage('No active C# file');
        return;
    }

    const csprojPath = await findProjectFile(editor.document.uri.fsPath);
    if (!csprojPath) {
        vscode.window.showErrorMessage('Could not find .csproj file for current file');
        return;
    }

    await runAnalysis(csprojPath, 'file');
}

async function runAnalysis(targetPath: string, scope: 'workspace' | 'file') {
    const config = vscode.workspace.getConfiguration('dotnet-unused');
    const useTerminal = config.get<boolean>('useTerminal', true);

    if (useTerminal) {
        await runAnalysisInTerminal(targetPath, scope);
    } else {
        await runAnalysisInOutputWindow(targetPath, scope);
    }
}

async function runAnalysisInTerminal(targetPath: string, scope: 'workspace' | 'file') {
    const config = vscode.workspace.getConfiguration('dotnet-unused');
    const cliPath = config.get<string>('cliPath') || 'dotnet-unused';
    const excludePublic = config.get<boolean>('excludePublic', true);

    // Use array for arguments - ShellExecution handles escaping automatically
    // This safely handles paths with spaces, quotes, and special characters
    const args: string[] = [targetPath];
    if (excludePublic) {
        args.push('--exclude-public', 'true');
    } else {
        args.push('--exclude-public', 'false');
    }

    const execution = new vscode.ShellExecution(cliPath, args);
    const task = new vscode.Task(
        { type: 'shell', task: 'Dotnet Unused Analysis' },
        vscode.TaskScope.Workspace,
        'Dotnet Unused Analysis',
        'dotnet-unused',
        execution
    );

    await vscode.tasks.executeTask(task);
    vscode.window.showInformationMessage(`Running analysis in terminal...`);
}

async function runAnalysisInOutputWindow(targetPath: string, scope: 'workspace' | 'file') {
    outputChannel.show();
    outputChannel.appendLine(`\n=== Starting ${scope} analysis ===`);
    outputChannel.appendLine(`Target: ${targetPath}\n`);

    const config = vscode.workspace.getConfiguration('dotnet-unused');
    const excludePublic = config.get<boolean>('excludePublic', true);

    await vscode.window.withProgress(
        {
            location: vscode.ProgressLocation.Notification,
            title: `Analyzing ${scope === 'workspace' ? 'workspace' : 'current file'} for unused code...`,
            cancellable: false
        },
        async () => {
            try {
                const report = await cliRunner.runAnalysis({
                    scope,
                    excludePublic,
                    targetPath
                });

                if (report) {
                    diagnosticProvider.updateDiagnostics(report);
                    unusedSymbolsProvider.refresh(report);

                    const message = `Found ${report.unusedSymbols.length} unused symbol(s) in ${report.summary.durationSeconds.toFixed(2)}s`;
                    outputChannel.appendLine(`\n${message}`);
                    vscode.window.showInformationMessage(message);
                }
            } catch (error) {
                const errorMessage = error instanceof Error ? error.message : String(error);
                outputChannel.appendLine(`Error: ${errorMessage}`);
                vscode.window.showErrorMessage(`Analysis failed: ${errorMessage}`);
            }
        }
    );
}

async function findProjectFile(filePath: string): Promise<string | null> {
    let dir = path.dirname(filePath);
    const root = vscode.workspace.getWorkspaceFolder(vscode.Uri.file(filePath))?.uri.fsPath;

    while (dir && dir !== root) {
        const files = fs.readdirSync(dir);
        const csproj = files.find(f => f.endsWith('.csproj'));
        if (csproj) {
            return path.join(dir, csproj);
        }
        const parentDir = path.dirname(dir);
        if (parentDir === dir) {
            break;
        }
        dir = parentDir;
    }

    return null;
}

async function exportReport() {
    vscode.window.showInformationMessage('Export report functionality coming soon!');
}

async function fixUnusedUsings() {
    const workspaceFolders = vscode.workspace.workspaceFolders;
    if (!workspaceFolders) {
        vscode.window.showErrorMessage('No workspace folder open');
        return;
    }

    const slnFiles = await vscode.workspace.findFiles('**/*.sln', '**/node_modules/**', 10);

    let targetPath: string;
    if (slnFiles.length === 0) {
        const csprojFiles = await vscode.workspace.findFiles('**/*.csproj', '**/node_modules/**', 10);
        if (csprojFiles.length === 0) {
            vscode.window.showErrorMessage('No .sln or .csproj files found in workspace');
            return;
        }

        if (csprojFiles.length === 1) {
            targetPath = csprojFiles[0].fsPath;
        } else {
            const selected = await vscode.window.showQuickPick(
                csprojFiles.map(f => ({ label: path.basename(f.fsPath), path: f.fsPath })),
                { placeHolder: 'Select a .csproj file to fix' }
            );
            if (!selected) {
                return;
            }
            targetPath = selected.path;
        }
    } else {
        if (slnFiles.length === 1) {
            targetPath = slnFiles[0].fsPath;
        } else {
            const selected = await vscode.window.showQuickPick(
                slnFiles.map(f => ({ label: path.basename(f.fsPath), path: f.fsPath })),
                { placeHolder: 'Select a .sln file to fix' }
            );
            if (!selected) {
                return;
            }
            targetPath = selected.path;
        }
    }

    // Confirm before making changes
    const confirm = await vscode.window.showWarningMessage(
        'This will automatically remove unused using directives from your files. Continue?',
        { modal: true },
        'Yes',
        'No'
    );

    if (confirm !== 'Yes') {
        return;
    }

    const config = vscode.workspace.getConfiguration('dotnet-unused');
    const cliPath = config.get<string>('cliPath') || 'dotnet-unused';

    // Use array for arguments - ShellExecution handles escaping automatically
    const args: string[] = [targetPath, '--fix'];

    const execution = new vscode.ShellExecution(cliPath, args);
    const task = new vscode.Task(
        { type: 'shell', task: 'Fix Unused Usings' },
        vscode.TaskScope.Workspace,
        'Fix Unused Using Directives',
        'dotnet-unused',
        execution
    );

    await vscode.tasks.executeTask(task);
    vscode.window.showInformationMessage('Removing unused using directives...');
}

export function deactivate() {
    if (diagnosticProvider) {
        diagnosticProvider.dispose();
    }
}
