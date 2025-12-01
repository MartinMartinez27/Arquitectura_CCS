using Arquitectura_CCS.Common;
using Arquitectura_CCS.Common.DTOs;
using Arquitectura_CCS.Common.Models;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;

namespace Arquitectura_CCS.EmergencyService.Services;

public class EmergencyConsumerService : BackgroundService
{
    private readonly ILogger<EmergencyConsumerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConsumer<Ignore, string> _consumer;

    public EmergencyConsumerService(
        ILogger<EmergencyConsumerService> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            GroupId = "emergency-group",
            AutoOffsetReset = AutoOffsetReset.Latest, // Solo mensajes nuevos
            EnableAutoCommit = false
        };

        _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe("emergency-topic");

        _logger.LogInformation("EmergencyConsumerService started. Listening for emergency messages...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _consumer.Consume(stoppingToken);

                var startTime = DateTime.UtcNow;
                _logger.LogWarning("EMERGENCY RECEIVED - Processing immediately");

                // Procesar la emergencia INMEDIATAMENTE
                await ProcessEmergencyMessageAsync(consumeResult.Message.Value);

                var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.LogWarning("EMERGENCY PROCESSED - Time: {ProcessingTime}ms", processingTime);

                _consumer.Commit(consumeResult);

                // Verificar que cumplimos con el requisito de <2 segundos
                if (processingTime > 2000)
                {
                    _logger.LogError("SLOW EMERGENCY RESPONSE: {ProcessingTime}ms", processingTime);
                }
                else
                {
                    _logger.LogInformation("EMERGENCY RESPONSE TIME OK: {ProcessingTime}ms", processingTime);
                }
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Error consuming emergency message from Kafka");
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("EmergencyConsumerService stopping...");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in EmergencyConsumerService");
            }
        }

        _consumer.Close();
    }

    private async Task ProcessEmergencyMessageAsync(string messageJson)
    {
        try
        {
            // Deserializar el mensaje de emergencia
            var emergencyData = System.Text.Json.JsonSerializer.Deserialize<EmergencyData>(messageJson);

            if (emergencyData == null)
            {
                _logger.LogWarning("Could not deserialize emergency message: {Message}", messageJson);
                return;
            }

            _logger.LogWarning(
                "PROCESSING EMERGENCY - ID: {EmergencyId}, Vehicle: {VehicleId}, Type: {EmergencyType}, Source: {Source}",
                emergencyData.EmergencyId, emergencyData.VehicleId, emergencyData.EmergencyType, emergencyData.Source
            );

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CCSDbContext>();

            // EJECUTAR ACCIONES DE EMERGENCIA INMEDIATAS

            // 1. Actualizar estado en base de datos
            var emergency = await dbContext.EmergencySignals.FindAsync(emergencyData.EmergencyId);
            if (emergency != null)
            {
                emergency.IsResolved = false; // Marcar como activa
                await dbContext.SaveChangesAsync();
            }

            // 2. Ejecutar acciones según el tipo de emergencia
            await ExecuteEmergencyActionsAsync(emergencyData);

            _logger.LogWarning(
                "EMERGENCY ACTIONS COMPLETED - ID: {EmergencyId}, Vehicle: {VehicleId}",
                emergencyData.EmergencyId, emergencyData.VehicleId
            );

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing emergency message: {Message}", messageJson);
        }
    }

    private async Task ExecuteEmergencyActionsAsync(EmergencyData emergency)
    {
        // Simular acciones de emergencia en tiempo real
        switch (emergency.EmergencyType)
        {
            case 1: // PanicButton
                _logger.LogWarning("PANIC BUTTON - Notifying authorities and owner");
                await SimulateAuthorityNotification(emergency);
                await SimulateOwnerNotification(emergency);
                break;

            case 2: // Mechanical
                _logger.LogWarning("MECHANICAL ISSUE - Dispatching assistance");
                await SimulateAssistanceDispatch(emergency);
                break;

            case 3: // Security
                _logger.LogWarning("SECURITY ISSUE - Alerting security services");
                await SimulateSecurityAlert(emergency);
                break;

            case 4: // Accident
                _logger.LogWarning("ACCIDENT - Dispatching emergency services");
                await SimulateEmergencyServicesDispatch(emergency);
                break;

            default:
                _logger.LogWarning("UNKNOWN EMERGENCY TYPE: {Type}", emergency.EmergencyType);
                break;
        }
    }

    // Métodos de simulación (en producción serían integraciones reales)
    private async Task SimulateAuthorityNotification(EmergencyData emergency)
    {
        await Task.Delay(100); // Simular llamada HTTP
        _logger.LogInformation("AUTHORITIES NOTIFIED - Vehicle: {VehicleId}, Location: {Lat}, {Lon}",
            emergency.VehicleId, emergency.Latitude, emergency.Longitude);
    }

    private async Task SimulateOwnerNotification(EmergencyData emergency)
    {
        await Task.Delay(50); // Simular envío de SMS/Email
        _logger.LogInformation("OWNER NOTIFIED - Vehicle: {VehicleId}, Emergency: {Type}",
            emergency.VehicleId, emergency.EmergencyType);
    }

    private async Task SimulateAssistanceDispatch(EmergencyData emergency)
    {
        await Task.Delay(200); // Simular dispatch
        _logger.LogInformation("ASSISTANCE DISPATCHED - Vehicle: {VehicleId}, Issue: {Description}",
            emergency.VehicleId, emergency.Description);
    }

    private async Task SimulateSecurityAlert(EmergencyData emergency)
    {
        await Task.Delay(150); // Simular alerta
        _logger.LogInformation("SECURITY ALERTED - Vehicle: {VehicleId}, Details: {Description}",
            emergency.VehicleId, emergency.Description);
    }

    private async Task SimulateEmergencyServicesDispatch(EmergencyData emergency)
    {
        await Task.Delay(300); // Simular dispatch de ambulancias/policía
        _logger.LogInformation("EMERGENCY SERVICES DISPATCHED - Vehicle: {VehicleId}, Accident reported",
            emergency.VehicleId);
    }

    public override void Dispose()
    {
        _consumer?.Dispose();
        base.Dispose();
    }
}