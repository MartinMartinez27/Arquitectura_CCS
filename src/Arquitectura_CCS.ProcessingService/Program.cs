using Arquitectura_CCS.Common;
using Arquitectura_CCS.ProcessingService.Services;
using Arquitectura_CCS.RulesEngine.Engine;
using Arquitectura_CCS.RulesEngine.Rules.ConcreteRules;

var builder = Host.CreateApplicationBuilder(args);


builder.Services.AddCommonServices(builder.Configuration);

builder.Services.AddSingleton<RulesEngine>();
builder.Services.AddTransient<UnplannedStopRule>();
builder.Services.AddTransient<SpeedLimitRule>();
builder.Services.AddTransient<CargoTemperatureRule>();

// Registrar el consumidor de Kafka
builder.Services.AddHostedService<TelemetryConsumerService>();

var host = builder.Build();
host.Run();