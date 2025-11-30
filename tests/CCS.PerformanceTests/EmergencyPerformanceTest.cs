using Arquitectura_CCS.Common.Enums;
using Arquitectura_CCS.Common.Models;
using Confluent.Kafka;
using System.Net.Http.Json;

namespace CCS.PerformanceTests;

public class EmergencyPerformanceTest : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly IProducer<Null, string> _kafkaProducer;

    public EmergencyPerformanceTest()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5263"),
            Timeout = TimeSpan.FromSeconds(30)
        };

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = "localhost:9092,localhost:9093,localhost:9094",
            Acks = Acks.All,
            MessageTimeoutMs = 5000
        };
        _kafkaProducer = new ProducerBuilder<Null, string>(producerConfig).Build();
    }

    [Fact]
    public async Task Emergency_Response_Under_2_Seconds_Single_Request()
    {
        // Arrange
        var emergencyRequest = new EmergencySignalRequest
        {
            VehicleId = "TRUCK001",
            EmergencyType = EmergencyType.PanicButton,
            Source = "panic_button",
            Latitude = 4.710989,
            Longitude = -74.072092,
            Description = "Performance test - panic button",
            AdditionalData = "{\"test\": \"performance\"}"
        };

        // Act & Measure
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var response = await _httpClient.PostAsJsonAsync("/api/telemetry/emergency", emergencyRequest);
        var content = await response.Content.ReadAsStringAsync();

        stopwatch.Stop();

        // Assert
        Assert.True(response.IsSuccessStatusCode, $"Request failed: {response.StatusCode} - {content}");
        Assert.True(stopwatch.ElapsedMilliseconds < 2000,
            $"Emergency response took {stopwatch.ElapsedMilliseconds}ms, expected <2000ms");

        Console.WriteLine($"Emergency processed in {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task Measure_Kafka_To_EmergencyService_Latency()
    {
        // Arrange
        var emergencyData = new
        {
            EmergencyId = Guid.NewGuid().ToString(),
            VehicleId = "TRUCK001",
            EmergencyType = 1,
            Source = "panic_button",
            Latitude = 4.710989,
            Longitude = -74.072092,
            Description = "Kafka latency test",
            CreatedAt = DateTime.UtcNow,
            Priority = "HIGH"
        };

        // Act & Measure
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var message = new Message<Null, string>
        {
            Value = System.Text.Json.JsonSerializer.Serialize(emergencyData)
        };

        var deliveryResult = await _kafkaProducer.ProduceAsync("emergency-topic", message);
        stopwatch.Stop();

        // Assert - Kafka production should be very fast
        Assert.True(stopwatch.ElapsedMilliseconds < 100,
            $"Kafka production took {stopwatch.ElapsedMilliseconds}ms");

        Console.WriteLine($"Kafka message produced in {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Message delivered to partition: {deliveryResult.Partition}, offset: {deliveryResult.Offset}");
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        _httpClient?.Dispose();
        _kafkaProducer?.Dispose();
    }
}