using Arquitectura_CCS.Common;
using Arquitectura_CCS.Common.DTOs;
using Arquitectura_CCS.Common.Models;
using Confluent.Kafka;
using RulesEngines = Arquitectura_CCS.RulesEngine.Engine.RulesEngine;

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
            _logger.LogInformation("RAW MESSAGE: {Message}", messageJson); // NUEVO

            var telemetryData = System.Text.Json.JsonSerializer.Deserialize<TelemetryData>(messageJson);

            if (telemetryData == null)
            {
                _logger.LogWarning("Could not deserialize telemetry message");
                return;
            }

            _logger.LogInformation("Processing telemetry for vehicle {VehicleId}", telemetryData.VehicleId); // NUEVO

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CCSDbContext>();
            var rulesEngine = scope.ServiceProvider.GetRequiredService<RulesEngines>();

            // Convertir a VehicleTelemetry para el RulesEngine
            var telemetry = new VehicleTelemetry
            {
                VehicleId = telemetryData.VehicleId,
                VehicleType = (Common.Enums.VehicleType)telemetryData.VehicleType,
                Latitude = telemetryData.Latitude,
                Longitude = telemetryData.Longitude,
                Speed = telemetryData.Speed,
                Direction = telemetryData.Direction,
                IsMoving = telemetryData.IsMoving,
                EngineOn = telemetryData.EngineOn,
                FuelLevel = telemetryData.FuelLevel,
                CargoTemperature = telemetryData.CargoTemperature,
                CargoStatus = telemetryData.CargoStatus,
                IsPlannedStop = telemetryData.IsPlannedStop,
                Timestamp = telemetryData.Timestamp
            };

            _logger.LogInformation("Calling RulesEngine for vehicle {VehicleId}", telemetryData.VehicleId); // NUEVO

            // Ejecutar el RulesEngine
            await rulesEngine.ProcessTelemetryAsync(telemetry);

            _logger.LogInformation("Completed processing for vehicle {VehicleId}", telemetryData.VehicleId);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing telemetry message");
        }
    }
    public override void Dispose()
    {
        _consumer?.Dispose();
        base.Dispose();
    }
}
