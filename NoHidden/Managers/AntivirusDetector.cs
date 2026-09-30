using System.Management;

namespace NoHidden.Managers;

public static class AntivirusDetector
{
    public static AntivirusInfo? GetAntivirusInfo()
    {
        using var searcher = new ManagementObjectSearcher(
            @"root\SecurityCenter2",
            "SELECT * FROM AntivirusProduct");

        foreach (ManagementObject instance in searcher.Get())
        {
            return new AntivirusInfo
            {
                DisplayName = instance["displayName"]?.ToString(),
                ProductState = Convert.ToInt32(instance["productState"]),
                InstanceGuid = instance["instanceGuid"]?.ToString(),
                ProductExecutablePath =
                    instance["pathToSignedProductExe"]?.ToString(),
                ReportingExecutablePath =
                    instance["pathToSignedReportingExe"]?.ToString()
            };
        }

        return null;
    }
}

public sealed class AntivirusInfo
{
    public string? DisplayName { get; init; }

    public int ProductState { get; init; }

    public string? InstanceGuid { get; init; }

    public string? ProductExecutablePath { get; init; }

    public string? ReportingExecutablePath { get; init; }

    public string ProductStateHex => $"0x{ProductState:X6}";
}
