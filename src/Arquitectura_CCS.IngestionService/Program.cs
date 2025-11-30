using Arquitectura_CCS.Common;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Optimizar para alta carga
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
// Configurar HttpClientFactory para Kafka
builder.Services.AddHttpClient();

// Configurar DbContext con pooling
builder.Services.AddDbContextPool<CCSDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);
        });
});

// Configurar Kafka Producer como Singleton (reutilizar)
builder.Services.AddSingleton<IProducer<Null, string>>(sp =>
{
    var config = new ProducerConfig
    {
        BootstrapServers = builder.Configuration["Kafka:BootstrapServers"],
        Acks = Acks.All,
        MessageSendMaxRetries = 2,
        BatchSize = 16384,
        LingerMs = 5,
        CompressionType = CompressionType.Snappy,
        QueueBufferingMaxMessages = 100000
    };

    return new ProducerBuilder<Null, string>(config).Build();
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();