using Confluent.Kafka;
using Xunit;

namespace CCS.PerformanceTests;

public class RealTimeMonitor
{
    [Fact(Skip = "Manual test for monitoring")]
    public async Task Monitor_System_Performance_For_5_Minutes()
    {
        var kafkaConfig = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092,localhost:9093,localhost:9094",
            GroupId = "monitor-group-" + Guid.NewGuid().ToString(),
            AutoOffsetReset = AutoOffsetReset.Latest
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(kafkaConfig).Build();
        consumer.Subscribe("emergency-topic");

        Console.WriteLine("Starting real-time monitoring for 5 minutes...");

        var emergencyResponseTimes = new List<double>();
        var startTime = DateTime.UtcNow;

        try
        {
            while ((DateTime.UtcNow - startTime).TotalMinutes < 5)
            {
                try
                {
                    var consumeResult = consumer.Consume(TimeSpan.FromSeconds(1));
                    if (consumeResult?.Message != null)
                    {
                        var messageTime = DateTime.UtcNow;
                        var emergencyData = System.Text.Json.JsonSerializer.Deserialize<EmergencyData>(consumeResult.Message.Value);

                        if (emergencyData != null)
                        {
                            var processingTime = (messageTime - emergencyData.CreatedAt).TotalMilliseconds;
                            emergencyResponseTimes.Add(processingTime);

                            Console.WriteLine($"Emergency {emergencyData.EmergencyId} - Response: {processingTime:F2}ms");

                            // Alert if response time exceeds 2 seconds
                            if (processingTime > 2000)
                            {
                                Console.WriteLine($"ALERT: Slow emergency response: {processingTime:F2}ms");
                            }
                        }
                    }

                    // Print statistics every 30 seconds
                    if (emergencyResponseTimes.Count > 0 && (DateTime.UtcNow - startTime).Seconds % 30 == 0)
                    {
                        PrintStatistics(emergencyResponseTimes);
                    }
                }
                catch (ConsumeException ex)
                {
                    Console.WriteLine($"Monitoring error: {ex.Error.Reason}");
                }
            }
        }
        finally
        {
            PrintFinalStatistics(emergencyResponseTimes);
        }
    }

    private static void PrintStatistics(List<double> responseTimes)
    {
        if (responseTimes.Count == 0) return;

        var avg = responseTimes.Average();
        var max = responseTimes.Max();
        var min = responseTimes.Min();
        var p95 = responseTimes.OrderBy(x => x).Skip((int)(responseTimes.Count * 0.95)).First();

        Console.WriteLine($"STATS - Avg: {avg:F2}ms, Min: {min:F2}ms, Max: {max:F2}ms, P95: {p95:F2}ms");
        Console.WriteLine($"   Emergencies processed: {responseTimes.Count}");
        Console.WriteLine($"   Under 2s: {responseTimes.Count(t => t < 2000)}/{responseTimes.Count}");
    }

    private static void PrintFinalStatistics(List<double> responseTimes)
    {
        if (responseTimes.Count == 0)
        {
            Console.WriteLine("No emergencies processed during monitoring period");
            return;
        }

        var avg = responseTimes.Average();
        var max = responseTimes.Max();
        var min = responseTimes.Min();
        var p95 = responseTimes.OrderBy(x => x).Skip((int)(responseTimes.Count * 0.95)).First();
        var under2s = responseTimes.Count(t => t < 2000);
        var successRate = (double)under2s / responseTimes.Count * 100;

        Console.WriteLine("FINAL MONITORING RESULTS:");
        Console.WriteLine($"   Total Emergencies: {responseTimes.Count}");
        Console.WriteLine($"   Average Response: {avg:F2}ms");
        Console.WriteLine($"   Best Response: {min:F2}ms");
        Console.WriteLine($"   Worst Response: {max:F2}ms");
        Console.WriteLine($"   95th Percentile: {p95:F2}ms");
        Console.WriteLine($"   Success Rate (<2s): {successRate:F2}%");
        Console.WriteLine($"   Requirements Met: {successRate >= 99.0}");
    }
}

// DTO para el monitor
public class EmergencyData
{
    public string EmergencyId { get; set; } = string.Empty;
    public string VehicleId { get; set; } = string.Empty;
    public int EmergencyType { get; set; }
    public string Source { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? AdditionalData { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Priority { get; set; } = string.Empty;
}