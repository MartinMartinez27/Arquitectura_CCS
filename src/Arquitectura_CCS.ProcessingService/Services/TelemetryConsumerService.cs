using Arquitectura_CCS.Common;
using Arquitectura_CCS.Common.DTOs;
using Confluent.Kafka;

namespace Arquitectura_CCS.ProcessingService.Services;

public class TelemetryConsumerService : BackgroundService
{
    private readonly ILogger<TelemetryConsumerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConsumer<Ignore, string> _consumer;

    public TelemetryConsumerService(
        ILogger<TelemetryConsumerService> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            GroupId = "processing-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe("telemetry-topic");

        _logger.LogInformation("TelemetryConsumerService started. Listening for telemetry messages...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _consumer.Consume(stoppingToken);

                _logger.LogInformation("Received telemetry message: {Value}", consumeResult.Message.Value);

                // Procesar el mensaje
                await ProcessTelemetryMessageAsync(consumeResult.Message.Value);

                _consumer.Commit(consumeResult);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Error consuming message from Kafka");
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("TelemetryConsumerService stopping...");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in TelemetryConsumerService");
            }
        }

        _consumer.Close();
    }

    private async Task ProcessTelemetryMessageAsync(string messageJson)
    {
        try
        {
            // Deserializar el mensaje
            var telemetryData = System.Text.Json.JsonSerializer.Deserialize<TelemetryData>(messageJson);

            if (telemetryData == null)
            {
                _logger.LogWarning("Could not deserialize telemetry message: {Message}", messageJson);
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CCSDbContext>();

            // Lógica de procesamiento
            _logger.LogInformation("Processed telemetry for vehicle {VehicleId} at {Timestamp}",
                telemetryData.VehicleId, telemetryData.Timestamp);

            // Detectar si el vehículo está detenido inesperadamente
            if (!telemetryData.IsMoving && telemetryData.EngineOn && !(telemetryData.IsPlannedStop ?? false))
            {
                _logger.LogWarning("Unplanned stop detected for vehicle {VehicleId}", telemetryData.VehicleId);
            }

            // Detectar exceso de velocidad (ejemplo: >80 km/h)
            if (telemetryData.Speed > 80)
            {
                _logger.LogWarning("Speed limit exceeded for vehicle {VehicleId}: {Speed} km/h",
                    telemetryData.VehicleId, telemetryData.Speed);
            }

            // Detectar temperatura de carga fuera de rango (para camiones)
            if (telemetryData.CargoTemperature.HasValue &&
                (telemetryData.CargoTemperature < 15 || telemetryData.CargoTemperature > 25))
            {
                _logger.LogWarning("Cargo temperature out of range for vehicle {VehicleId}: {Temperature}°C",
                    telemetryData.VehicleId, telemetryData.CargoTemperature);
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing telemetry message: {Message}", messageJson);
        }
    }

    public override void Dispose()
    {
        _consumer?.Dispose();
        base.Dispose();
    }
}