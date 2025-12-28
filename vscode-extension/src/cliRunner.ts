import * as vscode from 'vscode';
import * as child_process from 'child_process';
import * as fs from 'fs';
import * as path from 'path';
import * as os from 'os';
import { UnusedCodeReport, AnalysisOptions } from './types';

export class CliRunner {
    private outputChannel: vscode.OutputChannel;

    constructor(outputChannel: vscode.OutputChannel) {
        this.outputChannel = outputChannel;
    }

    async runAnalysis(options: AnalysisOptions): Promise<UnusedCodeReport | null> {
        const config = vscode.workspace.getConfiguration('dotnet-unused');
        const cliPath = config.get<string>('cliPath') || 'dotnet-unused';
        const excludePublic = config.get<boolean>('excludePublic', true);

        const tempFile = path.join(os.tmpdir(), `dotnet-unused-${Date.now()}.json`);

        const args = [
            options.targetPath,
            '--format', 'json',
            '--output', tempFile,
            '--exclude-public', excludePublic.toString()
        ];

        this.outputChannel.appendLine(`Running: ${cliPath} ${args.join(' ')}`);

        return new Promise((resolve, reject) => {
            const process = child_process.spawn(cliPath, args, {
                cwd: vscode.workspace.workspaceFolders?.[0]?.uri.fsPath,
                shell: true
            });

            let stdout = '';
            let stderr = '';

            process.stdout.on('data', (data) => {
                stdout += data.toString();
                this.outputChannel.append(data.toString());
            });

            process.stderr.on('data', (data) => {
                stderr += data.toString();
                this.outputChannel.append(data.toString());
            });

            process.on('close', (code) => {
                try {
                    if (code === 0) {
                        if (fs.existsSync(tempFile)) {
                            const content = fs.readFileSync(tempFile, 'utf-8');
                            const report: UnusedCodeReport = JSON.parse(content);
                            resolve(report);
                        } else {
                            this.outputChannel.appendLine('Error: Report file not found');
                            reject(new Error('Report file not found'));
                        }
                    } else {
                        this.outputChannel.appendLine(`Process exited with code ${code}`);
                        this.outputChannel.appendLine(`stderr: ${stderr}`);
                        reject(new Error(`Analysis failed with code ${code}`));
                    }
                } catch (error) {
                    this.outputChannel.appendLine(`Error parsing report: ${error}`);
                    reject(error);
                } finally {
                    // Always cleanup temp file
                    if (fs.existsSync(tempFile)) {
                        try {
                            fs.unlinkSync(tempFile);
                        } catch (cleanupError) {
                            this.outputChannel.appendLine(`Warning: Failed to cleanup temp file: ${cleanupError}`);
                        }
                    }
                }
            });

            process.on('error', (error) => {
                this.outputChannel.appendLine(`Failed to start process: ${error.message}`);
                // Cleanup temp file on process error
                if (fs.existsSync(tempFile)) {
                    try {
                        fs.unlinkSync(tempFile);
                    } catch (cleanupError) {
                        this.outputChannel.appendLine(`Warning: Failed to cleanup temp file: ${cleanupError}`);
                    }
                }
                reject(error);
            });
        });
    }

    async checkCliAvailable(): Promise<boolean> {
        const config = vscode.workspace.getConfiguration('dotnet-unused');
        const cliPath = config.get<string>('cliPath') || 'dotnet-unused';

        return new Promise((resolve) => {
            child_process.exec(`${cliPath} --help`, (error) => {
                resolve(!error);
            });
        });
    }
}
