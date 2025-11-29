using Arquitectura_CCS.Common;
using Arquitectura_CCS.ProcessingService.Services;

var builder = Host.CreateApplicationBuilder(args);

// Configurar servicios comunes
builder.Services.AddCommonServices(builder.Configuration);

// Registrar el consumidor de Kafka
builder.Services.AddHostedService<TelemetryConsumerService>();

var host = builder.Build();
host.Run();