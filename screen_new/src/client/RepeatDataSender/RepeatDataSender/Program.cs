using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using RepeatDataSender.Application.Abstractions;
using RepeatDataSender.Infrastructure.Configuration;
using RepeatDataSender.Infrastructure.Http;
using RepeatDataSender.Infrastructure.Linux;
using RepeatDataSender.Presentation;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddSingleton<ISystemInfoCollector, LinuxSystemInfoCollector>();
builder.Services.AddHttpClient<IMetricSender, HttpMetricSender>();

builder.Services.Configure<AppOptions>(builder.Configuration.GetSection(AppOptions.SectionName));
builder.Services.AddHostedService<MetricPumpHostedService>();

var host = builder.Build();
await host.RunAsync();