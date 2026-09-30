namespace NoHidden.Models;

public sealed record ScanProgress(
    int ScannedItems,
    string CurrentPath);
