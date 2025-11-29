using Arquitectura_CCS.Common;
using Arquitectura_CCS.EmergencyService.Services;

var builder = Host.CreateApplicationBuilder(args);

// Configurar servicios comunes
builder.Services.AddCommonServices(builder.Configuration);

// Registrar el consumidor de emergencias
builder.Services.AddHostedService<EmergencyConsumerService>();

var host = builder.Build();
host.Run();