using Arquitectura_CCS.Common;
using Arquitectura_CCS.Common.Models;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;

namespace Arquitectura_CCS.IngestionService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelemetryController : ControllerBase
    {
        private readonly IProducer<Null, string> _kafkaProducer;
        private readonly ILogger<TelemetryController> _logger;
        private readonly CCSDbContext _dbContext;

        public TelemetryController(
            IProducer<Null, string> kafkaProducer,
            ILogger<TelemetryController> logger,
            CCSDbContext dbContext)
        {
            _kafkaProducer = kafkaProducer;
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpPost("vehicle")]
        public async Task<IActionResult> ReceiveVehicleTelemetry([FromBody] VehicleTelemetryRequest request)
        {
            try
            {
                // Validar que el vehículo existe
                var vehicle = await _dbContext.Vehicles.FindAsync(request.VehicleId);
                if (vehicle == null)
                {
                    return BadRequest($"Vehicle {request.VehicleId} not found");
                }

                // Crear VehicleTelemetry desde el request
                var telemetry = new VehicleTelemetry
                {
                    TelemetryId = Guid.NewGuid().ToString(),
                    VehicleId = request.VehicleId,
                    VehicleType = request.VehicleType,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    Speed = request.Speed,
                    Direction = request.Direction,
                    IsMoving = request.IsMoving,
                    EngineOn = request.EngineOn,
                    FuelLevel = request.FuelLevel,
                    CargoTemperature = request.CargoTemperature,
                    CargoStatus = request.CargoStatus,
                    IsPlannedStop = request.IsPlannedStop,
                    Timestamp = request.Timestamp,
                    Vehicle = vehicle
                };

                // Guardar en base de datos
                _dbContext.VehicleTelemetries.Add(telemetry);
                await _dbContext.SaveChangesAsync();

                // Publicar en Kafka - USAR DTO para evitar referencia circular
                var kafkaMessage = new
                {
                    telemetry.TelemetryId,
                    telemetry.VehicleId,
                    telemetry.VehicleType,
                    telemetry.Latitude,
                    telemetry.Longitude,
                    telemetry.Speed,
                    telemetry.Direction,
                    telemetry.IsMoving,
                    telemetry.EngineOn,
                    telemetry.FuelLevel,
                    telemetry.CargoTemperature,
                    telemetry.CargoStatus,
                    telemetry.IsPlannedStop,
                    telemetry.Timestamp
                };

                var message = new Message<Null, string>
                {
                    Value = System.Text.Json.JsonSerializer.Serialize(kafkaMessage)
                };

                await _kafkaProducer.ProduceAsync("telemetry-topic", message);

                _logger.LogInformation("Telemetry received for vehicle {VehicleId}", request.VehicleId);

                return Accepted(new
                {
                    Message = "Telemetry received",
                    TelemetryId = telemetry.TelemetryId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing telemetry for vehicle {VehicleId}", request.VehicleId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("emergency")]
        public async Task<IActionResult> ReceiveEmergencySignal([FromBody] EmergencySignalRequest request)
        {
            try
            {
                _logger.LogWarning("EMERGENCY signal received from vehicle {VehicleId}, type: {EmergencyType}",
                    request.VehicleId, request.EmergencyType);

                // Validar que el vehículo existe
                var vehicle = await _dbContext.Vehicles.FindAsync(request.VehicleId);
                if (vehicle == null)
                {
                    _logger.LogError("Emergency vehicle not found: {VehicleId}", request.VehicleId);
                    return BadRequest($"Vehicle {request.VehicleId} not found");
                }

                // Crear EmergencySignal desde el request
                var emergency = new EmergencySignal
                {
                    EmergencyId = Guid.NewGuid().ToString(),
                    VehicleId = request.VehicleId,
                    EmergencyType = request.EmergencyType,
                    Source = request.Source,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    Description = request.Description,
                    AdditionalData = request.AdditionalData,
                    IsResolved = false,
                    CreatedAt = DateTime.UtcNow,
                    Vehicle = vehicle
                };

                // Guardar en BD
                _dbContext.EmergencySignals.Add(emergency);
                await _dbContext.SaveChangesAsync();

                // Publicar en topic de emergencia - USAR DTO para evitar referencia circular
                var kafkaMessage = new
                {
                    emergency.EmergencyId,
                    emergency.VehicleId,
                    emergency.EmergencyType,
                    emergency.Source,
                    emergency.Latitude,
                    emergency.Longitude,
                    emergency.Description,
                    emergency.AdditionalData,
                    emergency.CreatedAt,
                    Priority = "HIGH"
                };

                var message = new Message<Null, string>
                {
                    Value = System.Text.Json.JsonSerializer.Serialize(kafkaMessage),
                    Headers = new Headers {
                new Header("priority", new byte[] { 1 }), // Alta prioridad
                new Header("emergency_type", System.Text.Encoding.UTF8.GetBytes(request.EmergencyType.ToString()))
            }
                };

                await _kafkaProducer.ProduceAsync("emergency-topic", message);

                _logger.LogWarning("Emergency processed successfully - ID: {EmergencyId}, Vehicle: {VehicleId}",
                    emergency.EmergencyId, emergency.VehicleId);

                return Accepted(new
                {
                    Message = "Emergency signal received and processed",
                    EmergencyId = emergency.EmergencyId,
                    Priority = "HIGH",
                    Timestamp = emergency.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing emergency signal for vehicle {VehicleId}", request.VehicleId);
                return StatusCode(500, new
                {
                    Error = "Internal server error",
                    Details = ex.Message
                });
            }
        }
        //[HttpPost("vehicle")]
        //public async Task<IActionResult> ReceiveVehicleTelemetry([FromBody] VehicleTelemetryRequest request)
        //{
        //    try
        //    {
        //        // Publicar directamente a Kafka
        //        var message = new Message<Null, string>
        //        {
        //            Value = System.Text.Json.JsonSerializer.Serialize(request)
        //        };

        //        await _kafkaProducer.ProduceAsync("telemetry-topic", message);

        //        _logger.LogInformation("Telemetry received for vehicle {VehicleId}", request.VehicleId);

        //        return Accepted(new
        //        {
        //            Message = "Telemetry queued",
        //            VehicleId = request.VehicleId
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error processing telemetry for vehicle {VehicleId}", request.VehicleId);
        //        return StatusCode(500, "Internal server error");
        //    }
        //}

        //[HttpPost("emergency")]
        //public async Task<IActionResult> ReceiveEmergencySignal([FromBody] EmergencySignalRequest request)
        //{
        //    try
        //    {
        //        var message = new Message<Null, string>
        //        {
        //            Value = System.Text.Json.JsonSerializer.Serialize(request),
        //            Headers = new Headers {
        //        new Header("priority", new byte[] { 1 }),
        //        new Header("emergency_type", System.Text.Encoding.UTF8.GetBytes(request.EmergencyType.ToString()))
        //    }
        //        };

        //        await _kafkaProducer.ProduceAsync("emergency-topic", message);

        //        _logger.LogWarning("Emergency queued for vehicle {VehicleId}, type {EmergencyType}",
        //            request.VehicleId, request.EmergencyType);

        //        return Accepted(new
        //        {
        //            Message = "Emergency queued",
        //            VehicleId = request.VehicleId,
        //            Priority = "HIGH"
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error processing emergency signal for vehicle {VehicleId}", request.VehicleId);
        //        return StatusCode(500, "Internal server error");
        //    }
        //}

    }
}