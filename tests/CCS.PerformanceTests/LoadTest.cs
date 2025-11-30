using Arquitectura_CCS.Common.Enums;
using Arquitectura_CCS.Common.Models;
using Newtonsoft.Json.Serialization;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http.Json;

namespace CCS.PerformanceTests;

public class LoadTest : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly List<VehicleTelemetryRequest> _testData;

    public LoadTest()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5263"),
            Timeout = TimeSpan.FromSeconds(30)
        };

        //_testData = GenerateTestTelemetry(60000).ToList(); // 60,000 señales para 2 minutos
        _testData = GenerateTestTelemetry(10).ToList(); // 10 señales para 2 minutos
    }

    [Fact]
    public async Task Process_500_Signals_Per_Second_For_2_Minutes_Full_Load()
    {
        // Arrange - Requerimiento completo
        const int signalsPerSecond = 500;
        const int testDurationSeconds = 120; // 2 minutos
        const int totalSignals = signalsPerSecond * testDurationSeconds; // 60,000 señales

        var successfulRequests = 0;
        var failedRequests = 0;
        var totalProcessingTime = 0L;
        var semaphore = new SemaphoreSlim(100); // Alta concurrencia
        var testStartTime = DateTime.UtcNow;

        Console.WriteLine($"FULL LOAD TEST: {totalSignals} signals over {testDurationSeconds} seconds");
        Console.WriteLine($"Target: 500 signals/second");

        var testData = GenerateTestTelemetry(totalSignals).ToList();
        var tasks = testData.Select(async request =>
        {
            await semaphore.WaitAsync();
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var response = await _httpClient.PostAsJsonAsync("/api/telemetry/vehicle", request);
                stopwatch.Stop();

                if (response.IsSuccessStatusCode)
                {
                    Interlocked.Increment(ref successfulRequests);
                    Interlocked.Add(ref totalProcessingTime, stopwatch.ElapsedMilliseconds);

                    // Mostrar progreso cada 5,000 requests
                    if (successfulRequests % 5000 == 0)
                    {
                        var elapsed = DateTime.UtcNow - testStartTime;
                        var currentThroughput = successfulRequests / elapsed.TotalSeconds;
                        Console.WriteLine($"Progress: {successfulRequests}/{totalSignals} - Throughput: {currentThroughput:F2}/s");
                    }
                }
                else
                {
                    Interlocked.Increment(ref failedRequests);
                }
            }
            catch
            {
                Interlocked.Increment(ref failedRequests);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);

        var testEndTime = DateTime.UtcNow;
        var actualTestDuration = testEndTime - testStartTime;

        // Análisis de resultados
        var successRate = (double)successfulRequests / totalSignals * 100;
        var avgProcessingTime = successfulRequests > 0 ? totalProcessingTime / successfulRequests : 0;
        var throughput = successfulRequests / actualTestDuration.TotalSeconds;

        Console.WriteLine($"\n FULL LOAD TEST RESULTS:");
        Console.WriteLine($"   Total Signals: {totalSignals}");
        Console.WriteLine($"   Successful: {successfulRequests} ({successRate:F2}%)");
        Console.WriteLine($"   Failed: {failedRequests}");
        Console.WriteLine($"   Test Duration: {actualTestDuration.TotalSeconds:F2}s");
        Console.WriteLine($"   Avg Processing Time: {avgProcessingTime:F2}ms");
        Console.WriteLine($"   Actual Throughput: {throughput:F2} signals/sec");
        Console.WriteLine($"   Target Throughput: {signalsPerSecond} signals/sec");

        // Validación de requerimientos
        var requirementsMet = successRate >= 95.0 && throughput >= 450;

        if (requirementsMet)
        {
            Console.WriteLine($" ALL REQUIREMENTS MET!");
            Console.WriteLine($"   Success Rate: {successRate:F2}% >= 95%");
            Console.WriteLine($"   Throughput: {throughput:F2}/s >= 450/s");
        }
        else
        {
            Console.WriteLine($"SOME REQUIREMENTS NOT MET:");
            if (successRate < 95.0)
                Console.WriteLine($"Success Rate: {successRate:F2}% < 95%");
            else
                Console.WriteLine($"Success Rate: {successRate:F2}% >= 95%");

            if (throughput < 450)
                Console.WriteLine($"Throughput: {throughput:F2}/s < 450/s");
            else
                Console.WriteLine($"Throughput: {throughput:F2}/s >= 450/s");
        }

        Assert.True(successRate >= 95.0, $"Success rate {successRate:F2}% below requirement");
        Assert.True(throughput >= 450, $"Throughput {throughput:F2}/s below requirement");
    }

    [Fact]
    public async Task Process_500_Signals_Per_Second_For_2_Minutes()
    {
        // Arrange
        const int signalsPerSecond = 500;
        const int testDurationSeconds = 120; // 2 minutos
        const int totalSignals = signalsPerSecond * testDurationSeconds;

        var successfulRequests = 0;
        var failedRequests = 0;
        var totalProcessingTime = 0L;
        var semaphore = new SemaphoreSlim(50); // Limitar concurrencia

        // Act
        var testStartTime = DateTime.UtcNow;
        Console.WriteLine($"Starting load test: {totalSignals} signals over {testDurationSeconds} seconds");

        var tasks = _testData.Take(totalSignals).Select(async request =>
        {
            await semaphore.WaitAsync();
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var response = await _httpClient.PostAsJsonAsync("/api/telemetry/vehicle", request);
                stopwatch.Stop();

                if (response.IsSuccessStatusCode)
                {
                    Interlocked.Increment(ref successfulRequests);
                    Interlocked.Add(ref totalProcessingTime, stopwatch.ElapsedMilliseconds);
                }
                else
                {
                    Interlocked.Increment(ref failedRequests);
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Request failed: {response.StatusCode} - {errorContent}");

                    // Solo mostrar los primeros 10 errores para no saturar la consola
                    if (failedRequests <= 10)
                    {
                        Console.WriteLine($"   Request data: {System.Text.Json.JsonSerializer.Serialize(request)}");
                    }
                }
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref failedRequests);
                if (failedRequests <= 10)
                {
                    Console.WriteLine($"Request exception: {ex.Message}");
                }
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);

        var testEndTime = DateTime.UtcNow;
        var actualTestDuration = testEndTime - testStartTime;

        // Assert
        var successRate = (double)successfulRequests / totalSignals * 100;
        var avgProcessingTime = successfulRequests > 0 ? totalProcessingTime / successfulRequests : 0;
        var throughput = successfulRequests / actualTestDuration.TotalSeconds;

        Console.WriteLine($"LOAD TEST RESULTS:");
        Console.WriteLine($"   Total Signals Attempted: {totalSignals}");
        Console.WriteLine($"   Successful: {successfulRequests} ({successRate:F2}%)");
        Console.WriteLine($"   Failed: {failedRequests}");
        Console.WriteLine($"   Test Duration: {actualTestDuration.TotalSeconds:F2}s");
        Console.WriteLine($"   Avg Processing Time: {avgProcessingTime:F2}ms");
        Console.WriteLine($"   Throughput: {throughput:F2} signals/sec");
        Console.WriteLine($"   Target Throughput: {signalsPerSecond} signals/sec");

        // Requisitos del negocio
        Assert.True(successRate >= 95.0, $"Success rate {successRate:F2}% is below 95%");
        Assert.True(throughput >= 450, $"Throughput {throughput:F2} signals/sec is below 450");
        Assert.True(actualTestDuration.TotalSeconds <= testDurationSeconds * 1.2,
            $"Test took too long: {actualTestDuration.TotalSeconds:F2}s");
    }

    private IEnumerable<VehicleTelemetryRequest> GenerateTestTelemetry(int count)
    {
        var random = new Random();
        var vehicleIds = new[] { "TRUCK001", "TRUCK002", "CAR001", "CAR002", "MOTO001" };

        for (int i = 0; i < count; i++)
        {
            yield return new VehicleTelemetryRequest
            {
                VehicleId = vehicleIds[random.Next(vehicleIds.Length)],
                VehicleType = (VehicleType)random.Next(1, 4), // Conversión explícita
                Latitude = 4.6 + random.NextDouble() * 0.2,
                Longitude = -74.2 + random.NextDouble() * 0.2,
                Speed = random.Next(0, 120),
                Direction = random.Next(0, 360),
                IsMoving = random.NextDouble() > 0.3,
                EngineOn = true,
                FuelLevel = random.Next(10, 100),
                Timestamp = DateTime.UtcNow.AddSeconds(random.Next(-300, 300))
            };
        }
    }
    [Fact]
    public async Task Process_100_Signals_Per_Second_For_30_Seconds()
    {
        // Arrange - Carga más pequeña para diagnóstico
        const int signalsPerSecond = 100;
        const int testDurationSeconds = 30;
        const int totalSignals = signalsPerSecond * testDurationSeconds; // 3,000 señales

        var successfulRequests = 0;
        var failedRequests = 0;
        var totalProcessingTime = 0L;
        var semaphore = new SemaphoreSlim(20); // Menor concurrencia

        // Act
        var testStartTime = DateTime.UtcNow;
        Console.WriteLine($" Starting diagnostic load test: {totalSignals} signals over {testDurationSeconds} seconds");

        var testData = GenerateTestTelemetry(totalSignals).ToList();
        var tasks = testData.Select(async request =>
        {
            await semaphore.WaitAsync();
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var response = await _httpClient.PostAsJsonAsync("/api/telemetry/vehicle", request);
                stopwatch.Stop();

                if (response.IsSuccessStatusCode)
                {
                    Interlocked.Increment(ref successfulRequests);
                    Interlocked.Add(ref totalProcessingTime, stopwatch.ElapsedMilliseconds);
                }
                else
                {
                    Interlocked.Increment(ref failedRequests);
                    var errorContent = await response.Content.ReadAsStringAsync();

                    // Solo mostrar detalles de los primeros 5 errores
                    if (failedRequests <= 5)
                    {
                        Console.WriteLine($" Request failed: {response.StatusCode}");
                        Console.WriteLine($"   Error: {errorContent}");
                        Console.WriteLine($"   Vehicle: {request.VehicleId}, Type: {request.VehicleType}");
                    }
                }
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref failedRequests);
                if (failedRequests <= 5)
                {
                    Console.WriteLine($" Request exception: {ex.Message}");
                }
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);

        var testEndTime = DateTime.UtcNow;
        var actualTestDuration = testEndTime - testStartTime;

        // Assert
        var successRate = (double)successfulRequests / totalSignals * 100;
        var avgProcessingTime = successfulRequests > 0 ? totalProcessingTime / successfulRequests : 0;
        var throughput = successfulRequests / actualTestDuration.TotalSeconds;

        Console.WriteLine($"   DIAGNOSTIC LOAD TEST RESULTS:");
        Console.WriteLine($"   Total Signals: {totalSignals}");
        Console.WriteLine($"   Successful: {successfulRequests} ({successRate:F2}%)");
        Console.WriteLine($"   Failed: {failedRequests}");
        Console.WriteLine($"   Test Duration: {actualTestDuration.TotalSeconds:F2}s");
        Console.WriteLine($"   Avg Processing Time: {avgProcessingTime:F2}ms");
        Console.WriteLine($"   Throughput: {throughput:F2} signals/sec");

        // Objetivos más realistas para diagnóstico
        Assert.True(successRate > 50.0, $"Success rate {successRate:F2}% is too low for diagnostic");
        Console.WriteLine($"  Diagnostic test completed with {successRate:F2}% success rate");
    }

    [Fact]
    public async Task Emergency_Performance_Under_Load()
    {
        // Probar emergencias mientras hay carga normal
        var loadTask = Task.Run(async () =>
        {
            for (int i = 0; i < 100; i++)
            {
                var telemetry = GenerateTestTelemetry(10).First();
                await _httpClient.PostAsJsonAsync("/api/telemetry/vehicle", telemetry);
                await Task.Delay(100); // 10 señales/segundo de fondo
            }
        });

        // Medir tiempo de respuesta de emergencia bajo carga
        var emergencyRequest = new EmergencySignalRequest
        {
            VehicleId = "TRUCK001",
            EmergencyType = EmergencyType.PanicButton,
            Source = "panic_button",
            Latitude = 4.710989,
            Longitude = -74.072092,
            Description = "Performance test under load",
            AdditionalData = "{\"test\": \"under_load\"}"
        };

        var stopwatch = Stopwatch.StartNew();
        var response = await _httpClient.PostAsJsonAsync("/api/telemetry/emergency", emergencyRequest);
        stopwatch.Stop();

        await loadTask; // Esperar que termine la carga de fondo

        Assert.True(response.IsSuccessStatusCode);
        Assert.True(stopwatch.ElapsedMilliseconds < 2000,
            $"Emergency response under load took {stopwatch.ElapsedMilliseconds}ms");

        Console.WriteLine($"Emergency under load: {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task Process_100_Signals_Per_Second_For_30_Seconds_With_Error_Details()
    {
        const int totalSignals = 3000;
        var successfulRequests = 0;
        var failedRequests = 0;
        var totalProcessingTime = 0L;
        var semaphore = new SemaphoreSlim(30); // Reducir concurrencia
        var errorDetails = new ConcurrentDictionary<string, int>(); // Agrupar errores

        var testStartTime = DateTime.UtcNow;
        Console.WriteLine($"Starting detailed diagnostic test: {totalSignals} signals");

        var testData = GenerateTestTelemetry(totalSignals).ToList();
        var tasks = testData.Select(async request =>
        {
            await semaphore.WaitAsync();
            try
            {
                
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var response = await _httpClient.PostAsJsonAsync("/api/telemetry/vehicle", request);

                stopwatch.Stop();

                if (response.IsSuccessStatusCode)
                {
                    Interlocked.Increment(ref successfulRequests);
                    Interlocked.Add(ref totalProcessingTime, stopwatch.ElapsedMilliseconds);
                }
                else
                {
                    Interlocked.Increment(ref failedRequests);
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorKey = $"Status: {response.StatusCode}";

                    errorDetails.AddOrUpdate(errorKey, 1, (key, count) => count + 1);

                    // Capturar detalles del primer error de cada tipo
                    if (errorDetails[errorKey] <= 3)
                    {
                        Console.WriteLine($"{errorKey}: {errorContent}");
                    }
                }
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref failedRequests);
                var errorKey = $"Exception: {ex.GetType().Name}";

                errorDetails.AddOrUpdate(errorKey, 1, (key, count) => count + 1);

                if (errorDetails[errorKey] <= 3)
                {
                    Console.WriteLine($"{errorKey}: {ex.Message}");
                }
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);

        var testEndTime = DateTime.UtcNow;
        var actualTestDuration = testEndTime - testStartTime;

        // Reporte detallado
        var successRate = (double)successfulRequests / totalSignals * 100;
        var throughput = successfulRequests / actualTestDuration.TotalSeconds;

        Console.WriteLine($"   DETAILED RESULTS:");
        Console.WriteLine($"   Success Rate: {successRate:F2}% ({successfulRequests}/{totalSignals})");
        Console.WriteLine($"   Throughput: {throughput:F2} signals/sec");
        Console.WriteLine($"   Duration: {actualTestDuration.TotalSeconds:F2}s");

        Console.WriteLine($"  ERROR BREAKDOWN:");
        foreach (var error in errorDetails.OrderByDescending(e => e.Value))
        {
            var percentage = (double)error.Value / totalSignals * 100;
            Console.WriteLine($"   {error.Key}: {error.Value} times ({percentage:F2}%)");
        }

        // Análisis de bottlenecks
        Console.WriteLine($"\n BOTTLENECK ANALYSIS:");
        if (errorDetails.Any(e => e.Key.Contains("Timeout")))
        {
            Console.WriteLine("    Timeouts detected - Consider increasing HttpClient timeout");
        }
        if (errorDetails.Any(e => e.Key.Contains("500")))
        {
            Console.WriteLine("     Server errors (500) - Check application logs");
        }
        if (errorDetails.Any(e => e.Key.Contains("BadRequest")))
        {
            Console.WriteLine("     BadRequest errors - Validate request data");
        }
    }

    [Fact]
    public async Task Process_With_Optimized_Configuration()
    {
        // Usar HttpClient con configuración optimizada
        var handler = new SocketsHttpHandler()
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
            MaxConnectionsPerServer = 50
        };

        using var optimizedClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:5263"),
            Timeout = TimeSpan.FromSeconds(10)
        };

        const int totalSignals = 1000;
        var successful = 0;
        var semaphore = new SemaphoreSlim(20); // Concurrencia conservadora

        var tasks = GenerateTestTelemetry(totalSignals).Select(async request =>
        {
            await semaphore.WaitAsync();
            try
            {
                var response = await optimizedClient.PostAsJsonAsync("/api/telemetry/vehicle", request);
                if (response.IsSuccessStatusCode)
                    Interlocked.Increment(ref successful);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);

        Console.WriteLine($"✅ Optimized test: {successful}/{totalSignals} successful ({((double)successful / totalSignals) * 100:F2}%)");
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        _httpClient?.Dispose();
    }
}