using Arquitectura_CCS.Common.Models;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Arquitectura_CCS.RulesEngine.Rules;

public abstract class BaseRule : IRule
{
    public abstract string RuleId { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }
    public virtual int Priority => 1;

    public abstract Task<bool> EvaluateAsync(VehicleTelemetry telemetry);
    public abstract Task ExecuteActionsAsync(VehicleTelemetry telemetry, IServiceProvider serviceProvider);

    protected virtual async Task PublishToKafkaAsync(string topic, object message, IServiceProvider serviceProvider)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<BaseRule>>();

            // Obtener la configuración de Kafka
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var bootstrapServers = configuration["Kafka:BootstrapServers"];

            if (string.IsNullOrEmpty(bootstrapServers))
            {
                logger.LogWarning("Kafka BootstrapServers not configured");
                return;
            }

            var config = new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                Acks = Acks.All,
                MessageSendMaxRetries = 3
            };

            using var producer = new ProducerBuilder<Null, string>(config).Build();

            var messageJson = System.Text.Json.JsonSerializer.Serialize(message);

            var result = await producer.ProduceAsync(topic, new Message<Null, string>
            {
                Value = messageJson
            });

            logger.LogInformation("Alert published to {Topic} for rule {RuleName}", topic, Name);
        }
        catch (ProduceException<Null, string> ex)
        {
            using var scope = serviceProvider.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<BaseRule>>();
            logger.LogError(ex, "Kafka error publishing to {Topic}: {Error}", topic, ex.Error.Reason);
        }
        catch (Exception ex)
        {
            using var scope = serviceProvider.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<BaseRule>>();
            logger.LogError(ex, "Error publishing alert to Kafka topic: {Topic}", topic);
        }
    }
}