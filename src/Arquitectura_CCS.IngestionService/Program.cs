using Arquitectura_CCS.Common;
using Confluent.Kafka;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configurar servicios comunes (DbContext, etc.)
builder.Services.AddCommonServices(builder.Configuration);

// Configurar Kafka Producer
builder.Services.AddSingleton<IProducer<Null, string>>(sp =>
{
    var config = new ProducerConfig
    {
        BootstrapServers = builder.Configuration["Kafka:BootstrapServers"],
        Acks = Acks.All,
        MessageSendMaxRetries = 3,
        BatchSize = 16384,
        LingerMs = 1,
        CompressionType = CompressionType.Snappy
    };

    return new ProducerBuilder<Null, string>(config).Build();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();