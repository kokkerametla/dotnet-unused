export interface UnusedCodeReport {
    summary: {
        totalSymbolsAnalyzed: number;
        totalReferencesFound: number;
        unusedCount: number;
        durationSeconds: number;
    };
    unusedSymbols: UnusedSymbol[];
}

export interface UnusedSymbol {
    kind: 'Method' | 'Property' | 'Field';
    fullyQualifiedName: string;
    filePath: string;
    lineNumber: number;
}

export interface AnalysisOptions {
    scope: 'workspace' | 'file';
    excludePublic: boolean;
    targetPath: string;
}
