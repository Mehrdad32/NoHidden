# NoHidden 3.0.0

NoHidden 3 is a modern rewrite of the original USB hidden-file recovery utility using .NET 10 and WPF.

## Highlights

- Recover user files and folders hidden with Hidden/System attributes
- Detect suspicious USB malware patterns
- Analyze autorun.inf
- Detect BAT/CMD/VBS/JS/PowerShell/HTA scripts
- Detect suspicious EXE/COM/SCR/PIF files
- Detect double extensions such as photo.jpg.exe
- Detect folder impersonation such as Documents.exe next to Documents\
- Inspect Windows shortcuts and suspicious command targets
- Scan selected files with Microsoft Defender
- Calculate SHA-256 locally and open VirusTotal hash reports without uploading the file
- Neutralize suspicious files by renaming them with .nohidden-disabled instead of deleting them
- System-wide AutoRun/AutoPlay protection with explicit Administrator elevation
- Persian and English UI
- Self-contained Windows x64 and x86 builds
- No separate .NET installation required

## Downloads

- **NoHidden-v3.0.0-win-x64.exe** — recommended for almost all current Windows PCs
- **NoHidden-v3.0.0-win-x86.exe** — for 32-bit Windows installations
- **SHA256SUMS.txt** — SHA-256 checksums for both executables

## Safety model

The initial USB scan is non-destructive. NoHidden does not blindly delete executable or script files. Recovery actions are separated from suspicious-file actions.

Microsoft Defender scans are invoked without automatic remediation, and VirusTotal lookup uses a locally calculated SHA-256 hash rather than uploading the file.

## Known limitations

- Binaries are not code-signed yet and may trigger Windows SmartScreen.
- Heuristic findings are not definitive malware verdicts.
- Neutralized files do not yet have an in-app Undo button.
- AutoRun/AutoPlay protection does not yet have an in-app rollback button.
- VirusTotal results open in the browser instead of being displayed inline.
- Direct integration with third-party antivirus CLI tools is not included.
- Devices reported by Windows as Fixed rather than Removable may not appear in the USB list.
- No dedicated ARM64 package is included in this release.

Full documentation is available in the repository README.
