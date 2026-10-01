<p align="center">
  <img src="NoHidden/logo.png" width="96" alt="NoHidden Logo">
</p>

<h1 align="center">NoHidden 3</h1>

<p align="center">
  A lightweight Windows utility for recovering USB files hidden by malware and detecting common USB-malware patterns.
</p>

<p align="center">
  <a href="README.md">🇮🇷 راهنمای فارسی</a>
</p>

<p align="center">
  <a href="https://github.com/Mehrdad32/NoHidden/actions/workflows/build.yml">
    <img src="https://github.com/Mehrdad32/NoHidden/actions/workflows/build.yml/badge.svg" alt="Build">
  </a>
  <a href="https://github.com/Mehrdad32/NoHidden/releases/latest">
    <img src="https://img.shields.io/github/v/release/Mehrdad32/NoHidden?label=release" alt="Latest release">
  </a>
  <a href="LICENSE.txt">
    <img src="https://img.shields.io/badge/license-Apache%202.0-blue.svg" alt="Apache 2.0">
  </a>
</p>

---

## Download

NoHidden 3 is published as a **self-contained** Windows application. Users do not need to install .NET separately.

| Architecture | Recommended for | Download |
|---|---|---|
| Windows 64-bit (x64) | Almost all modern PCs | **[Download NoHidden 3 x64](https://github.com/Mehrdad32/NoHidden/releases/latest/download/NoHidden-v3.0.0-win-x64.exe)** |
| Windows 32-bit (x86) | Older 32-bit Windows installations | **[Download NoHidden 3 x86](https://github.com/Mehrdad32/NoHidden/releases/latest/download/NoHidden-v3.0.0-win-x86.exe)** |

All releases and SHA-256 checksums:

**[GitHub Releases](https://github.com/Mehrdad32/NoHidden/releases/latest)**

> If you are not sure which build to download, x64 is the correct choice for most current Windows PCs.

---

## What problem does NoHidden solve?

Some USB malware hides the user's real files and folders by applying **Hidden** and **System** attributes, then places deceptive shortcuts, scripts, or executables in their place. The drive may still contain the data while appearing empty or misleading in Explorer.

NoHidden is focused on that scenario:

- Finds user files and folders hidden with Hidden/System attributes.
- Restores their visibility without modifying their contents.
- Detects common suspicious USB-malware patterns.
- Can neutralize a suspicious file without deleting it.
- Can submit a selected file to Microsoft Defender for a custom scan.
- Can calculate SHA-256 locally and open the matching VirusTotal hash report.
- Does not blindly delete EXE/BAT/VBS or other files based on extension alone.

---

## Features in 3.0.0

### Hidden-file recovery

NoHidden detects user files and folders carrying:

- `Hidden`
- `System`
- or both attributes

These findings get a **Restore visibility** action. The operation removes only Hidden/System attributes and does not alter file contents.

### USB malware heuristics

The scanner checks for:

- `autorun.inf`
- `.bat`, `.cmd`, `.vbs`, `.vbe`, `.js`, `.jse`, `.wsf`, `.wsh`, `.ps1`, and `.hta`
- `.exe`, `.com`, `.scr`, and `.pif`
- double extensions such as `photo.jpg.exe`
- folder impersonation such as `Documents.exe` next to `Documents\`
- Windows `.lnk` shortcuts
- shortcuts launching CMD, PowerShell, WScript, CScript, MSHTA, or Rundll32
- Hidden/System executables and scripts

> A heuristic finding is not a definitive malware verdict. NoHidden shows the reason and provides safer review actions instead of automatically deleting files.

---

## Recommended workflow

1. Start NoHidden normally.
2. Connect the USB drive.
3. Select the drive in **USB scanner**.
4. Click **Scan USB**.
5. Review the findings.
6. Use **Restore visibility** for hidden user content.
7. For suspicious files, review **Defender scan** and/or **VirusTotal** first.
8. If the file remains suspicious and you do not want to delete it, use **Neutralize**.
9. Scan the USB again.

The initial scan is **non-destructive** and does not modify files.

---

## Finding levels

| Level | Meaning |
|---|---|
| Recovery | Hidden user content that can be made visible again |
| Low | Weak signal; review recommended |
| Suspicious | Location or behavior deserves attention |
| High risk | Strong pattern commonly associated with USB malware |
| Critical | Reserved for very high-risk findings |

---

## Buttons and actions

### Scan USB

Recursively scans the selected removable drive. The scan can be canceled without modifying files.

### Refresh

Refreshes the removable-drive list from Windows.

### Restore visibility

Shown only for recoverable hidden user content. Removes Hidden/System attributes.

NoHidden intentionally does not offer this action for executable files, scripts, shortcuts, or `autorun.inf`.

### Restore all hidden items

Restores all currently detected recoverable items. Suspicious payloads are excluded.

### Defender scan

Runs a Microsoft Defender custom scan for the selected file.

The action is launched with **automatic remediation disabled**, so NoHidden is asking Defender for a scan result rather than instructing it to delete the file.

Windows may show UAC depending on system configuration.

### VirusTotal

NoHidden does **not** upload the file.

It computes SHA-256 locally and opens the report page for that hash in VirusTotal.

If VirusTotal has never seen the hash, a report may not be available.

### Neutralize

Does not delete the suspicious file. It renames it like this:

```text
suspicious.exe
→
suspicious.exe.nohidden-disabled
```

This prevents normal execution while preserving the original bytes.

If you later determine that the file is safe, remove the `.nohidden-disabled` suffix manually.

### Protect Windows

Disables AutoRun and AutoPlay system-wide to reduce the risk of accidentally launching content from unknown removable drives.

This operation requires Administrator permission. NoHidden explains why elevation is needed before showing the normal Windows UAC prompt.

### Antivirus information button

Shows the security product reported by Windows Security Center and its diagnostic state code.

The raw state code is **not** a security score or health percentage.

---

## Privacy

NoHidden is designed to be **offline-first**.

- USB scanning is local.
- NoHidden never uploads files automatically.
- Defender scanning uses the Windows security engine installed on the PC.
- VirusTotal lookup computes SHA-256 locally and opens a hash-report URL.
- No custom analytics or telemetry is included in NoHidden.
- Online actions require an explicit user action.

---

## Known limitations in 3.0.0

- Release binaries are currently **not code-signed**, so Windows SmartScreen may warn about a newly downloaded build. Download only from the official GitHub Release and verify `SHA256SUMS.txt` when needed.
- Heuristic findings are not definitive malware verdicts.
- NoHidden intentionally does not auto-delete suspicious files.
- Neutralize has no in-app Undo in 3.0.0; trusted files can be restored manually by removing the `.nohidden-disabled` suffix.
- There is no in-app control in 3.0.0 to restore the previous AutoRun/AutoPlay policy after using Protect Windows; that action applies a system-wide policy.
- VirusTotal results are not displayed inline yet; NoHidden opens the hash report in the browser and does not upload the file.
- Defender Scan depends on the Microsoft Defender command-line scanner being available.
- Direct third-party antivirus CLI integration is not included yet.
- Some USB SSDs or devices reported by Windows as `Fixed` rather than `Removable` may not appear in the drive list.
- A dedicated ARM64 package is not published yet.
- Legitimate EXE/BAT/script files can exist on USB media; extension alone is never treated as proof of malware.
- If a USB drive is disconnected during scanning, NoHidden treats the scan as incomplete rather than reporting a clean result.

---

## Code signing policy

NoHidden is being prepared to use **Authenticode code signing** for official releases through the SignPath Foundation Open Source program.

- Only binaries built from this repository through the official release pipeline are eligible for signing.
- Signable builds must run on GitHub-hosted runners and have verifiable build origin.
- Signed files use `NoHidden` as the product name and their product version must match the GitHub release version.
- Older releases published before code signing is enabled, including `v3.0.0`, remain unsigned.
- A valid digital signature verifies publisher identity and file integrity after signing; it is not, by itself, a claim that software is malware-free.

Full policy and signature-verification instructions:

**[Code signing policy](CODE_SIGNING.md)**

---

## Languages

- Persian (فارسی)
- English

The selected UI language is saved in the user's local profile.

---

## Development

Stack:

- .NET 10
- WPF
- C#
- CommunityToolkit.Mvvm
- `NoHidden.Core` for UI-independent scanning/recovery logic
- GitHub Actions for build, tests, and release packaging

Build:

```powershell
dotnet restore
dotnet build -c Debug
```

Run:

```powershell
dotnet run --project .\NoHidden\NoHidden.csproj
```

Safe USB scanner fixture and manual test plan:

**[docs/TESTING.md](docs/TESTING.md)**

---

## Project history

NoHidden originally started as a small utility for recovering files hidden by USB malware. Version 3 is a modern rewrite using .NET 10 and WPF.

Original Persian article:

**[No Hidden — recover files hidden by viruses](https://mehrdad32.ir/669/no_hidden_software/)**

---

## License

Apache License 2.0 — see [LICENSE.txt](LICENSE.txt).

<p align="center">
  Built by <a href="https://github.com/Mehrdad32">Mehrdad32</a>
</p>
