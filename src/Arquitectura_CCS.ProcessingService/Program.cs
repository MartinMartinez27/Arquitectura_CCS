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
// DIAGNÓSTICO: Verificar servicios registrados
var serviceProvider = host.Services;
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

try
{
    var rulesEngine = serviceProvider.GetRequiredService<RulesEngine>();
    var rules = rulesEngine.GetActiveRules();
    logger.LogInformation("RulesEngine initialized with {Count} rules: {Rules}",
        rules.Count, string.Join(", ", rules.Select(r => r.Name)));
}
catch (Exception ex)
{
    logger.LogError(ex, "ERROR: RulesEngine not properly initialized");
}

host.Run();