# NoHidden USB Scanner Test Plan

Use a spare USB flash drive. The fixture contains only harmless text content; filenames and attributes are chosen to exercise NoHidden detection rules.

## 1. Create the fixture

From the repository root:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\New-NoHiddenTestUsb.ps1 -Drive E:
```

Replace `E:` with the removable drive letter. The script refuses to run if Windows does not report the target as a removable drive, and it refuses to overwrite conflicting paths.

## 2. Scan with NoHidden

Expected findings:

| Fixture | Expected result |
| --- | --- |
| `NH-Test-HiddenFolder` | Recoverable Hidden/System item |
| `NH-Test-HiddenNote.txt` | Recoverable Hidden/System item |
| `NH-Test-Photo.jpg.exe` | High-risk double extension |
| `NH-Test-Documents.exe` next to `NH-Test-Documents\` | High-risk folder impersonation |
| `NH-Test-Review.bat` | Suspicious script |
| hidden `NH-Test-Payload.vbs` | High-risk hidden script; **must not** offer visibility recovery |
| `autorun.inf` | High-risk suspicious autorun |

Verify both English and Persian UI, Cancel Scan, individual visibility recovery, and Restore All Hidden Items.

## 3. Verify recovery

After restoring the two recoverable items, confirm they are visible in Explorer. NoHidden must not delete or alter the contents of those items.

## 4. Remove the fixture

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\Remove-NoHiddenTestUsb.ps1 -Drive E:
```

Cleanup requires the fixture marker and removes only the exact paths recorded by the fixture creator.

## Additional manual cases

- Disconnect the USB during a scan: NoHidden should report a failed/incomplete scan, never a clean result.
- Cancel a scan: no files should be changed.
- Change language after scan results are visible: all result labels and reasons should update.
- Scan a normal USB containing legitimate EXE/BAT files: findings should be review-only; NoHidden must not auto-delete them.
