# Code signing policy

NoHidden publishes Windows executables from the public source repository at:

https://github.com/Mehrdad32/NoHidden

## Scope

This policy applies to official NoHidden release binaries that are marked as digitally signed.

Older releases published before code signing was enabled remain unsigned. In particular, NoHidden v3.0.0 was released without an Authenticode signature.

## Signing provider

The project intends to use the SignPath Foundation Open Source Code Signing program for official release binaries.

When enabled, the certificate is provided under the SignPath Foundation open-source program. The project's private signing key is not stored in this repository or in GitHub Actions.

## Build origin

A binary is eligible for release signing only when all of the following are true:

- The source belongs to the `Mehrdad32/NoHidden` repository.
- The artifact is produced by the official GitHub Actions release workflow.
- All jobs leading to the signing request execute on GitHub-hosted runners.
- The build can be associated with a specific repository commit and release version.
- The normal build and project tests complete successfully before signing.
- The unsigned artifact is stored as a GitHub Actions artifact before it is submitted for signing.
- A release signing request is explicitly approved as required by the SignPath Foundation policy.

Locally built executables are not official signed releases.

## Signed file metadata

Signed NoHidden executable files must use metadata consistent with the release:

- **Product name:** NoHidden
- **Repository:** https://github.com/Mehrdad32/NoHidden
- **Product version:** must match the version of the corresponding release
- **Architectures:** x64 and/or x86, as identified by the release asset name

The signing configuration must reject artifacts that do not match the expected product metadata.

## Release integrity

For official releases, SHA-256 checksums are published alongside the executable files.

Code signing and checksums serve different purposes:

- Authenticode verifies the signing identity and detects changes made after signing.
- SHA-256 checksums let users compare downloaded bytes with the values published by the project.

A valid signature is not, by itself, a guarantee that a program is malware-free.

## Verify a release signature

### PowerShell

```powershell
Get-AuthenticodeSignature .\NoHidden-vX.Y.Z-win-x64.exe |
    Format-List Status, StatusMessage, SignerCertificate, TimeStamperCertificate
```

For a signed official release, `Status` should be `Valid`.

### SignTool

With the Windows SDK installed:

```powershell
signtool verify /pa /v .\NoHidden-vX.Y.Z-win-x64.exe
```

The command must complete successfully and the certificate chain must validate.

## Security model

NoHidden is a defensive utility. Its USB scan is non-destructive by default and identifies hidden user content and common suspicious USB-malware patterns. Potentially suspicious files are not blindly deleted.

System-changing operations are explicitly presented to the user before execution. Operations that require Administrator privileges use the standard Windows UAC elevation flow.

## Changes to this policy

Material changes to the build or signing process are made in the public repository and are reviewable through Git history.
