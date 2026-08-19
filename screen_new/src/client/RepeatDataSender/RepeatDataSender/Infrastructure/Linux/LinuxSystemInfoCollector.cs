using LinuxDotNet.SystemInfo;
using RepeatDataSender.Application.Abstractions;
using RepeatDataSender.Domain.Metrics;

namespace RepeatDataSender.Infrastructure.Linux;

/// <summary>
/// Collects all relevant system metrics on Linux (via the
/// LinuxDotNet.SystemInfo package, which reads /proc and /sys).
/// </summary>
public sealed class LinuxSystemInfoCollector : ISystemInfoCollector
{
    private const int CpuUsageSampleDelayMilliseconds = 200;

    public Task<MetricSnapshot> CollectAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var values = new Dictionary<string, string>(StringComparer.Ordinal);

        var hardware = PlatformProvider.GetHardware();
        var kernel = PlatformProvider.GetKernel();
        var uptime = PlatformProvider.GetUptime();
        var load = PlatformProvider.GetLoadAverage();
        var memory = PlatformProvider.GetMemoryStat();
        var system = PlatformProvider.GetSystemStat();
        var fileSystem = PlatformProvider.GetFileSystemUsage("/");
        var network = PlatformProvider.GetNetworkStat();
        var processes = PlatformProvider.GetProcessSummary();
        var battery = PlatformProvider.GetBatteryDevice();

        // CPU
        values[MetricPlaceholders.CpuBrand] = hardware.CpuBrandString;
        values[MetricPlaceholders.CpuVendor] = hardware.CpuVendor;
        values[MetricPlaceholders.CpuLogical] = hardware.LogicalCpu.ToString();
        values[MetricPlaceholders.CpuPhysical] = hardware.PhysicalCpu.ToString();
        values[MetricPlaceholders.CpuFrequencyMaxMhz] =
            (hardware.CpuFrequencyMax / 1_000_000.0).ToString("0.##");
        values[MetricPlaceholders.CpuUsage] =
            MeasureCpuUsage().ToString("0.0");
        values[MetricPlaceholders.Load1] = load.Average1.ToString("0.00");
        values[MetricPlaceholders.Load5] = load.Average5.ToString("0.00");
        values[MetricPlaceholders.Load15] = load.Average15.ToString("0.00");

        // Memory
        values[MetricPlaceholders.MemoryTotalMb] = ToMegabytes(memory.MemoryTotal);
        values[MetricPlaceholders.MemoryAvailableMb] = ToMegabytes(memory.MemoryAvailable);
        values[MetricPlaceholders.MemoryUsedMb] = ToMegabytes(memory.MemoryTotal - memory.MemoryAvailable);
        values[MetricPlaceholders.MemoryUsedPercent] =
            Percent(memory.MemoryTotal, memory.MemoryTotal - memory.MemoryAvailable);
        values[MetricPlaceholders.SwapTotalMb] = ToMegabytes(memory.SwapTotal);
        values[MetricPlaceholders.SwapFreeMb] = ToMegabytes(memory.SwapFree);
        values[MetricPlaceholders.SwapUsedMb] = ToMegabytes(memory.SwapTotal - memory.SwapFree);

        // Root filesystem
        values[MetricPlaceholders.RootTotalMb] = ToMegabytes(fileSystem.TotalSize);
        values[MetricPlaceholders.RootUsedMb] = ToMegabytes(fileSystem.TotalSize - fileSystem.AvailableSize);
        values[MetricPlaceholders.RootAvailableMb] = ToMegabytes(fileSystem.AvailableSize);
        values[MetricPlaceholders.RootUsedPercent] =
            Percent(fileSystem.TotalSize, fileSystem.TotalSize - fileSystem.AvailableSize);

        // Network (first non-loopback interface)
        var iface = network.Interfaces.FirstOrDefault(i => !i.Interface.StartsWith("lo", StringComparison.Ordinal));
        values[MetricPlaceholders.NetInterface] = iface?.Interface ?? "-";
        values[MetricPlaceholders.NetRxBytes] = iface?.RxBytes.ToString() ?? "0";
        values[MetricPlaceholders.NetTxBytes] = iface?.TxBytes.ToString() ?? "0";

        // System
        values[MetricPlaceholders.Uptime] = FormatUptime(uptime.Elapsed);
        values[MetricPlaceholders.Processes] = processes.ProcessCount.ToString();
        values[MetricPlaceholders.Threads] = processes.ThreadCount.ToString();
        values[MetricPlaceholders.RunningTasks] = system.RunnableTasks.ToString();
        values[MetricPlaceholders.BlockedTasks] = system.BlockedTasks.ToString();

        // OS / kernel
        values[MetricPlaceholders.OsName] = kernel.OsPrettyName ?? kernel.OsName ?? kernel.OsId ?? "Linux";
        values[MetricPlaceholders.Kernel] = kernel.KernelVersion;
        values[MetricPlaceholders.BootTime] = kernel.BootTime != DateTimeOffset.MinValue
            ? kernel.BootTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
            : "-";

        // Battery
        if (battery.Supported)
        {
            values[MetricPlaceholders.BatteryPercent] = battery.Capacity.ToString();
            values[MetricPlaceholders.BatteryStatus] = battery.Status;
            values[MetricPlaceholders.BatteryVoltageV] = (battery.Voltage / 1_000_000.0).ToString("0.00");
        }

        return Task.FromResult(new MetricSnapshot(values));
    }

    private static double MeasureCpuUsage()
    {
        var first = PlatformProvider.GetSystemStat().CpuTotal;
        Thread.Sleep(CpuUsageSampleDelayMilliseconds);
        var second = PlatformProvider.GetSystemStat().CpuTotal;

        var total = second.User + second.Nice + second.System + second.Idle + second.IoWait +
                    second.Irq + second.SoftIrq + second.Steal + second.Guest + second.GuestNice
                    - (first.User + first.Nice + first.System + first.Idle + first.IoWait +
                       first.Irq + first.SoftIrq + first.Steal + first.Guest + first.GuestNice);

        if (total <= 0)
        {
            return 0;
        }

        var idle = second.Idle + second.IoWait - first.Idle - first.IoWait;
        return Math.Clamp((total - idle) * 100.0 / total, 0, 100);
    }

    private static string ToMegabytes(ulong bytes)
    {
        return (bytes / 1024.0 / 1024.0).ToString("0.##");
    }

    private static string Percent(ulong total, ulong part)
    {
        if (total == 0)
        {
            return "0";
        }

        return (part * 100.0 / total).ToString("0.0");
    }

    private static string FormatUptime(TimeSpan elapsed)
    {
        return $"{elapsed.Days}d {elapsed.Hours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}";
    }
}