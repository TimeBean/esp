namespace RepeatDataSender.Domain.Metrics;

/// <summary>
/// Canonical placeholder names produced by the system-info collector.
/// Use them inside metric templates in <c>appsettings.json</c>, e.g.
/// <c>"{Load1}"</c>.
/// </summary>
public static class MetricPlaceholders
{
    public const string CpuBrand = "CpuBrand";
    public const string CpuVendor = "CpuVendor";
    public const string CpuLogical = "CpuLogical";
    public const string CpuPhysical = "CpuPhysical";
    public const string CpuFrequencyMaxMhz = "CpuFrequencyMaxMhz";
    public const string CpuUsage = "CpuUsage";
    public const string Load1 = "Load1";
    public const string Load5 = "Load5";
    public const string Load15 = "Load15";

    public const string MemoryTotalMb = "MemoryTotalMb";
    public const string MemoryAvailableMb = "MemoryAvailableMb";
    public const string MemoryUsedMb = "MemoryUsedMb";
    public const string MemoryUsedPercent = "MemoryUsedPercent";
    public const string SwapTotalMb = "SwapTotalMb";
    public const string SwapFreeMb = "SwapFreeMb";
    public const string SwapUsedMb = "SwapUsedMb";

    public const string MemoryAvailableGb = "MemoryAvailableGb";

    public const string DiskFreeGbPrefix = "FreeGb_";

    public const string RootTotalMb = "RootTotalMb";
    public const string RootUsedMb = "RootUsedMb";
    public const string RootAvailableMb = "RootAvailableMb";
    public const string RootUsedPercent = "RootUsedPercent";

    public const string NetInterface = "NetInterface";
    public const string NetRxBytes = "NetRxBytes";
    public const string NetTxBytes = "NetTxBytes";

    public const string Uptime = "Uptime";
    public const string UptimeHours = "UptimeHours";
    public const string Processes = "Processes";
    public const string Threads = "Threads";
    public const string RunningTasks = "RunningTasks";
    public const string BlockedTasks = "BlockedTasks";

    public const string OsName = "OsName";
    public const string Kernel = "Kernel";
    public const string BootTime = "BootTime";

    public const string BatteryPercent = "BatteryPercent";
    public const string BatteryStatus = "BatteryStatus";
    public const string BatteryVoltageV = "BatteryVoltageV";

    /// <summary>
    /// Placeholder for the free space of a configured disk, e.g. for
    /// <c>/mnt/nvme0n1p2</c> this returns <c>FreeGb_mnt_nvme0n1p2</c>.
    /// </summary>
    public static string DiskFreeGb(string path)
    {
        return DiskFreeGbPrefix + SanitizePath(path);
    }

    private static string SanitizePath(string path)
    {
        var trimmed = path.Trim('/');
        if (trimmed.Length == 0)
        {
            return "Root";
        }

        return string.Concat(trimmed.Select(c => char.IsLetterOrDigit(c) ? c : '_'));
    }
}