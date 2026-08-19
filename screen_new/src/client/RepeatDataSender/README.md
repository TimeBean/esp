# RepeatDataSender

A .NET 10 console app that periodically collects Linux system metrics
(via the [`LinuxDotNet.SystemInfo`](https://www.nuget.org/packages/LinuxDotNet.SystemInfo)
NuGet package) and sends them to the ESP display (`screen_new`) over HTTP.

Every cycle it resolves the configured metric **variants** and posts each one
to the ESP `POST /data` endpoint, where it is rendered with
`write_service::print_one_metric`. The number of variants and the delays
between sends are configured in `appsettings.json`.

> Linux only — the collector reads `/proc` and `/sys`.

## How it works

```
+----------------+   collects    +------------------------+
|  Presentation   | -----------> | LinuxSystemInfoCollector|
| (background     |              | (LinuxDotNet.SystemInfo)|
|  hosted service)|              +------------------------+
+----------------+                       |
      |                                  v
      | SendMetricsCommand (MediatR)  MetricSnapshot (placeholders)
      v                                  |
+------------------+  resolves   +-------v--------+
| HttpMetricSender | <---------- |  MetricTemplate |
| POST /data (JSON)|             |  (appsettings)  |
+------------------+             +-----------------+
```

- **Domain** — `MetricTemplate` (value template + label + layout),
  `MetricLayout` (position/font for `print_one_metric`), `MetricSnapshot`
  (placeholder → value) and `MetricPlaceholders`.
- **Application** — MediatR `CollectSystemInfoQuery` and `SendMetricsCommand`;
  the command snapshots the system, resolves every variant and sends them one
  by one, pausing in between.
- **Infrastructure** — `LinuxSystemInfoCollector` (wraps the NuGet package),
  `HttpMetricSender` (POSTs JSON), `AppOptions` (config binding).
- **Presentation** — `MetricPumpHostedService`, the loop that fires the command
  every `IntervalMilliseconds`.

## Requirements

- Linux
- .NET SDK 10

```bash
dotnet run --project RepeatDataSender
```

## Configuration

`RepeatDataSender/appsettings.json`, section `RepeatData`:

| Key                            | Default                     | Meaning                                   |
|--------------------------------|-----------------------------|-------------------------------------------|
| `TargetUri`                    | `http://192.168.0.77/data`  | ESP endpoint to POST to.                  |
| `IntervalMilliseconds`         | `5000`                      | Delay between full cycles.                |
| `DelayBetweenMetricsMilliseconds` | `1000`                   | Delay between each variant within a cycle.|
| `Disks`                        | `["/", "/mnt/nvme0n1p2"]`   | Mount points whose free space is collected.|
| `Metrics`                      | 4 variants                  | The N metric variants to display.         |

Each `Metrics` entry:

```json
{
  "Value": "{UptimeHours}",
  "Label": "UPTIME",
  "ValueX": 120, "ValueY": 120,
  "MetricX": 120, "MetricY": 160,
  "ValueFontSize": 5, "MetricFontSize": 3
}
```

- `Value` / `Label` — text to draw; `{placeholder}` tokens are replaced with
  live values. `Label` maps to the `metric` field of the ESP JSON payload.
- `ValueX`/`ValueY` — position of the value; `MetricX`/`MetricY` — position of
  the label (240×240 screen).
- `ValueFontSize`/`MetricFontSize` — font sizes passed to `print_one_metric`.

CLI overrides are also supported, e.g.:

```bash
dotnet run -- --RepeatData:TargetUri http://10.0.0.5/data --RepeatData:IntervalMilliseconds 1000
```

## Placeholders

Collected every cycle from `LinuxDotNet.SystemInfo`:

| Group   | Placeholders |
|---------|--------------|
| CPU     | `{CpuBrand}` `{CpuVendor}` `{CpuLogical}` `{CpuPhysical}` `{CpuFrequencyMaxMhz}` `{CpuUsage}` |
| Load    | `{Load1}` `{Load5}` `{Load15}` |
| Memory  | `{MemoryTotalMb}` `{MemoryAvailableMb}` `{MemoryUsedMb}` `{MemoryUsedPercent}` `{MemoryAvailableGb}` `{SwapTotalMb}` `{SwapFreeMb}` `{SwapUsedMb}` |
| Disk    | `{FreeGb_<path>}` per entry in `Disks` — `/` → `{FreeGb_Root}`, `/mnt/nvme0n1p2` → `{FreeGb_mnt_nvme0n1p2}`; `{RootTotalMb}` `{RootUsedMb}` `{RootAvailableMb}` `{RootUsedPercent}` (root only) |
| Network | `{NetInterface}` `{NetRxBytes}` `{NetTxBytes}` (first non-loopback iface) |
| System  | `{Uptime}` `{UptimeHours}` `{Processes}` `{Threads}` `{RunningTasks}` `{BlockedTasks}` |
| OS      | `{OsName}` `{Kernel}` `{BootTime}` |
| Battery | `{BatteryPercent}` `{BatteryStatus}` `{BatteryVoltageV}` (only when present) |

`{UptimeHours}`, `{MemoryAvailableGb}` and `{FreeGb_<path>}` are rounded to
whole numbers (no decimal point).

Placeholders whose hardware is unavailable (e.g. no battery on a desktop) are
**not** collected; an unknown token stays visible in the sent text as-is, so a
typo in the config is easy to spot.