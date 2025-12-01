using Arquitectura_CCS.Common;
using Arquitectura_CCS.NotificationService.Services;

var builder = Host.CreateApplicationBuilder(args);

// Configurar servicios comunes
builder.Services.AddCommonServices(builder.Configuration);

// Registrar el servicio de notificaciones
builder.Services.AddHostedService<NotificationConsumerService>();

var host = builder.Build();
host.Run();