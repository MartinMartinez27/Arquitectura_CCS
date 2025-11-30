using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configurar servicios básicos
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configurar Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CCS Vehicle Tracking API Gateway",
        Version = "v1",
        Description = "Central API Gateway for CCS Vehicle Tracking System"
    });
});

// Configurar HttpClient CORREGIDO
builder.Services.AddHttpClient("IngestionService", client =>
{
    client.BaseAddress = new Uri("http://localhost:5263/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "CCS-Gateway/1.0");
});

var app = builder.Build();

// Configurar middleware
app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CCS API Gateway v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "CCS Vehicle Tracking API Documentation";
    });
}

// =============================================
// ENDPOINTS DEL GATEWAY
// =============================================

// Health check del Gateway
app.MapGet("/health", () =>
{
    return Results.Ok(new
    {
        status = "Healthy",
        timestamp = DateTime.UtcNow,
        service = "CCS API Gateway",
        version = "1.0.0"
    });
}).WithTags("Health");

// Redirección desde raíz
app.MapGet("/", () => Results.Redirect("/swagger"));

// =============================================
// PROXY AL INGESTION SERVICE - VERSIÓN CORREGIDA
// =============================================

// Enviar telemetría de vehículo - CORREGIDO
app.MapPost("/vehicles/telemetry", async (HttpRequest request, IHttpClientFactory httpClientFactory) =>
{
    try
    {
        // Leer el cuerpo de la request original
        using var reader = new StreamReader(request.Body);
        var requestBody = await reader.ReadToEndAsync();

        if (string.IsNullOrEmpty(requestBody))
        {
            return Results.BadRequest("Request body is empty");
        }

        var client = httpClientFactory.CreateClient("IngestionService");

        // Crear la request con el contenido correcto
        var content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/telemetry/vehicle", content);

        var responseContent = await response.Content.ReadAsStringAsync();
        return Results.Text(responseContent, "application/json", statusCode: (int)response.StatusCode);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Gateway error: {ex.Message}");
    }
}).WithTags("Vehicles");

// Enviar señal de emergencia - CORREGIDO
app.MapPost("/vehicles/emergency", async (HttpRequest request, IHttpClientFactory httpClientFactory) =>
{
    try
    {
        // Leer el cuerpo de la request original
        using var reader = new StreamReader(request.Body);
        var requestBody = await reader.ReadToEndAsync();

        if (string.IsNullOrEmpty(requestBody))
        {
            return Results.BadRequest("Request body is empty");
        }

        var client = httpClientFactory.CreateClient("IngestionService");

        // Crear la request con el contenido correcto
        var content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/telemetry/emergency", content);

        var responseContent = await response.Content.ReadAsStringAsync();
        return Results.Text(responseContent, "application/json", statusCode: (int)response.StatusCode);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Gateway error: {ex.Message}");
    }
}).WithTags("Vehicles");

// Obtener lista de emergencias - CORREGIDO (si existe el endpoint)
app.MapGet("/vehicles/emergencies", async (IHttpClientFactory httpClientFactory) =>
{
    try
    {
        var client = httpClientFactory.CreateClient("IngestionService");
        var response = await client.GetAsync("/api/telemetry/emergencies");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Results.NotFound("Endpoint /api/telemetry/emergencies not found in IngestionService");
        }

        var content = await response.Content.ReadAsStringAsync();
        return Results.Text(content, "application/json", statusCode: (int)response.StatusCode);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Gateway error: {ex.Message}");
    }
}).WithTags("Vehicles");

// Health check del IngestionService - CORREGIDO
app.MapGet("/health/ingestion", async (IHttpClientFactory httpClientFactory) =>
{
    try
    {
        var client = httpClientFactory.CreateClient("IngestionService");
        var response = await client.GetAsync("/api/telemetry/test");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Si no existe /test, probar con otro endpoint
            return Results.Ok(new
            {
                status = "IngestionService is running but /test endpoint not found",
                timestamp = DateTime.UtcNow
            });
        }

        var content = await response.Content.ReadAsStringAsync();
        return Results.Text(content, "application/json", statusCode: (int)response.StatusCode);
    }
    catch (Exception ex)
    {
        return Results.Problem($"IngestionService unavailable: {ex.Message}");
    }
}).WithTags("Health");

// Endpoint para verificar disponibilidad del IngestionService
app.MapGet("/debug/ingestion", async (IHttpClientFactory httpClientFactory) =>
{
    try
    {
        var client = httpClientFactory.CreateClient("IngestionService");
        var response = await client.GetAsync("/");

        return Results.Ok(new
        {
            ingestionServiceStatus = response.IsSuccessStatusCode ? "Reachable" : "Unreachable",
            statusCode = (int)response.StatusCode,
            timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Cannot connect to IngestionService: {ex.Message}");
    }
}).WithTags("Debug");

app.Run();