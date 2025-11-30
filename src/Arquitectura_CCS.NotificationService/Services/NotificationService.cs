using Arquitectura_CCS.Common;
using Arquitectura_CCS.Common.DTOs;
using Arquitectura_CCS.Common.Models;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;

namespace Arquitectura_CCS.NotificationService.Services;

public class NotificationConsumerService : BackgroundService
{
    private readonly ILogger<NotificationConsumerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConsumer<Ignore, string> _consumer;

    public NotificationConsumerService(
        ILogger<NotificationConsumerService> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            GroupId = "notification-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Suscribirse a múltiples topics
        _consumer.Subscribe(new List<string> { "emergency-topic", "alerts-topic" });

        _logger.LogInformation("NotificationService started. Listening for emergency and alert messages...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _consumer.Consume(stoppingToken);

                _logger.LogInformation("Received message from topic: {Topic}", consumeResult.Topic);

                // Procesar según el tipo de topic
                if (consumeResult.Topic == "emergency-topic")
                {
                    await ProcessEmergencyNotificationAsync(consumeResult.Message.Value);
                }
                else if (consumeResult.Topic == "alerts-topic")
                {
                    await ProcessAlertNotificationAsync(consumeResult.Message.Value);
                }

                _consumer.Commit(consumeResult);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Error consuming message from Kafka");
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("NotificationService stopping...");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in NotificationService");
            }
        }

        _consumer.Close();
    }

    private async Task ProcessEmergencyNotificationAsync(string messageJson)
    {
        try
        {
            var emergencyData = System.Text.Json.JsonSerializer.Deserialize<EmergencyData>(messageJson);

            if (emergencyData == null)
            {
                _logger.LogWarning("Could not deserialize emergency message");
                return;
            }

            _logger.LogWarning("SENDING EMERGENCY NOTIFICATIONS - Vehicle: {VehicleId}, Type: {EmergencyType}",
                emergencyData.VehicleId, emergencyData.EmergencyType);

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CCSDbContext>();

            // Obtener información del vehículo y propietario
            var vehicle = await dbContext.Vehicles.FindAsync(emergencyData.VehicleId);
            if (vehicle == null) return;

            // Enviar notificaciones según el tipo de emergencia
            await SendEmergencyNotificationsAsync(emergencyData, vehicle);

            // Guardar en base de datos
            var notification = new Notification
            {
                NotificationId = Guid.NewGuid().ToString(),
                VehicleId = emergencyData.VehicleId,
                RuleId = "EMERGENCY_AUTO",
                ActionId = "AUTO_NOTIFY",
                Type = "emergency",
                Recipient = "authorities,owner,rescue",
                Message = $"Emergency: {emergencyData.EmergencyType} - Vehicle: {vehicle.LicensePlate} - {emergencyData.Description}",
                IsSent = true,
                CreatedAt = DateTime.UtcNow,
                SentAt = DateTime.UtcNow
            };

            dbContext.Notifications.Add(notification);
            await dbContext.SaveChangesAsync();

            _logger.LogInformation("Emergency notifications sent for vehicle: {VehicleId}", emergencyData.VehicleId);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing emergency notification");
        }
    }

    private async Task ProcessAlertNotificationAsync(string messageJson)
    {
        try
        {
            // Procesar alertas normales (exceso de velocidad, detenciones, etc.)
            _logger.LogInformation("Processing alert notification: {Message}", messageJson);

            // Aquí procesarías alertas no críticas
            await SendAlertNotificationAsync("Alert received from system");

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing alert notification");
        }
    }

    private async Task SendEmergencyNotificationsAsync(EmergencyData emergency, Vehicle vehicle)
    {
        var tasks = new List<Task>();

        // Notificar a autoridades (simulado)
        tasks.Add(SendEmailNotificationAsync(
            to: "authorities@ccs.gov.co",
            subject: $"EMERGENCY - Vehicle {vehicle.LicensePlate}",
            body: BuildEmergencyEmailBody(emergency, vehicle)
        ));

        // Notificar al propietario (simulado)
        tasks.Add(SendSmsNotificationAsync(
            to: "+573001234567",
            message: $"EMERGENCY: Your vehicle {vehicle.LicensePlate} reported {GetEmergencyTypeName(emergency.EmergencyType)}. Location: {emergency.Latitude}, {emergency.Longitude}"
        ));

        // Notificar a servicios de rescate (simulado)
        if (emergency.EmergencyType == 4) // Accidente
        {
            tasks.Add(SendEmailNotificationAsync(
                to: "rescue@ccs.gov.co",
                subject: "ACCIDENT REPORTED - Immediate Response Required",
                body: BuildAccidentEmailBody(emergency, vehicle)
            ));
        }

        await Task.WhenAll(tasks);
    }

    private async Task SendEmailNotificationAsync(string to, string subject, string body)
    {
        try
        {
            // Simular envío de email
            _logger.LogInformation("[SIMULATED] Email sent to: {To}, Subject: {Subject}", to, subject);
            _logger.LogInformation("   Body: {Body}", body);

            await Task.Delay(100); // Simular envío

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to: {To}", to);
        }
    }

    private async Task SendSmsNotificationAsync(string to, string message)
    {
        try
        {
            // Simular envío de SMS
            _logger.LogInformation("[SIMULATED] SMS sent to: {To}, Message: {Message}", to, message);

            await Task.Delay(50); // Simular envío

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS to: {To}", to);
        }
    }

    private async Task SendAlertNotificationAsync(string message)
    {
        _logger.LogInformation("[SIMULATED] Alert notification: {Message}", message);
        await Task.CompletedTask;
    }

    private string BuildEmergencyEmailBody(EmergencyData emergency, Vehicle vehicle)
    {
        return $@"
<h2>EMERGENCY NOTIFICATION - CCS Vehicle Tracking</h2>
<p><strong>Vehicle:</strong> {vehicle.LicensePlate} ({vehicle.Brand} {vehicle.Model})</p>
<p><strong>Emergency Type:</strong> {GetEmergencyTypeName(emergency.EmergencyType)}</p>
<p><strong>Location:</strong> {emergency.Latitude}, {emergency.Longitude}</p>
<p><strong>Description:</strong> {emergency.Description}</p>
<p><strong>Time:</strong> {emergency.CreatedAt:yyyy-MM-dd HH:mm:ss UTC}</p>
<p><strong>Source:</strong> {emergency.Source}</p>
<br>
<p><em>This is an automated notification from CCS Vehicle Tracking System</em></p>
        ";
    }

    private string BuildAccidentEmailBody(EmergencyData emergency, Vehicle vehicle)
    {
        return $@"
<h2>ACCIDENT EMERGENCY - IMMEDIATE RESPONSE REQUIRED</h2>
<p><strong>Vehicle:</strong> {vehicle.LicensePlate} ({vehicle.Brand} {vehicle.Model})</p>
<p><strong>Emergency Type:</strong> ACCIDENT</p>
<p><strong>Exact Location:</strong> {emergency.Latitude}, {emergency.Longitude}</p>
<p><strong>Description:</strong> {emergency.Description}</p>
<p><strong>Time of Incident:</strong> {emergency.CreatedAt:yyyy-MM-dd HH:mm:ss UTC}</p>
<br>
<p style='color: red;'><strong>⚠️ IMMEDIATE MEDICAL/RESCUE RESPONSE REQUIRED</strong></p>
<br>
<p><em>Automated alert from CCS Emergency Response System</em></p>
        ";
    }

    private string GetEmergencyTypeName(int emergencyType)
    {
        return emergencyType switch
        {
            1 => "PANIC BUTTON",
            2 => "MECHANICAL ISSUE",
            3 => "SECURITY THREAT",
            4 => "ACCIDENT",
            _ => "UNKNOWN EMERGENCY"
        };
    }

    public override void Dispose()
    {
        _consumer?.Dispose();
        base.Dispose();
    }
}