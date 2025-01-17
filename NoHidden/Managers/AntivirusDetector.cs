using System.Management;

namespace NoHidden.Managers
{
    public class AntivirusDetector
    {
        public static AntivirusInfo? GetAntivirusInfo()
        {
            // Query WMI for installed antivirus products
            using var searcher = new ManagementObjectSearcher(@"root\SecurityCenter2", "SELECT * FROM AntivirusProduct");

            foreach (var instance in searcher.Get())
            {
                var displayName = instance["displayName"]?.ToString();
                var productState = Convert.ToInt32(instance["productState"]);

                return new AntivirusInfo
                {
                    DisplayName = displayName,
                    ProductState = productState
                };
            }

            return null;
        }
    }

    public class AntivirusInfo
    {
        public string? DisplayName { get; set; }
        public int ProductState { get; set; }
    }
}
