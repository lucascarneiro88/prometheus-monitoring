using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Prometheus;
using System;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Adiciona serviços necessários
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Criação de um contador de requisições com label 'statusCode'
var requestCounter = Metrics.CreateCounter("aula_request_total", "Contador de requests", new CounterConfiguration
{
    LabelNames = new[] { "statusCode" }
});

// Criação de um Gauge para monitorar o número de bytes processados
var BytesProcessed = Metrics.CreateGauge("aula_bytes_processed", "Bytes processados pelo servidor.");

// Criação de um Gauge para monitorar memória livre, com valor aleatório
var freeBytes = Metrics.CreateGauge("free_bytes", "Memória livre em bytes.");

// Criação de um Histograma para monitorar o tempo de resposta da API
var responseDurationHistogram = Metrics.CreateHistogram("aula_response_duration_seconds", "Histograma do tempo de resposta da API em segundos.", new HistogramConfiguration
{
    // Definindo buckets para medir o tempo de resposta
    Buckets = Histogram.LinearBuckets(start: 0.01, width: 0.1, count: 20) // Cada bucket de 0.1 segundos, começando em 0.01 segundos
});

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    // Endpoint para expor as métricas do Prometheus
    endpoints.MapMetrics();

    // Endpoint para incrementar o contador a cada requisição à raiz
    app.MapGet("/", () =>
    {
        // Inicia o cronômetro para medir o tempo de resposta
        var stopwatch = Stopwatch.StartNew();

        // Simulando um processamento e medindo o tempo de resposta
        Console.WriteLine("Requisição recebida! Incrementando o contador...");
        requestCounter.Labels("200").Inc(); // Incrementa o contador com o statusCode "200"

        // Processamento fictício
        var random = new Random();
        var bytesProcessedThisRequest = random.Next(500, 1000); // Exemplo de bytes processados entre 500 e 1000
        BytesProcessed.Inc(bytesProcessedThisRequest); // Incrementa o Gauge de bytes processados

        // Finaliza o cronômetro
        stopwatch.Stop();

        // Registra o tempo de resposta no histograma
        responseDurationHistogram.Observe(stopwatch.Elapsed.TotalSeconds);

        return "Hello World! Meus Amigos"; 
    });

    // Atualiza a métrica de memória livre a cada 5 segundos com um valor aleatório
    var randomMemory = new Random();
    var timer = new System.Threading.Timer(e =>
    {
        var randomFreeBytes = randomMemory.Next(0, 1000000000); // Valor aleatório até 1 bilhão de bytes
        freeBytes.Set(randomFreeBytes); // Atualiza a métrica de memória livre
        Console.WriteLine($"Memória livre (em bytes): {randomFreeBytes}");
    }, null, 0, 5000); // Executa a cada 5 segundos
});

// Servir arquivos estáticos (como favicon.ico)
app.UseStaticFiles();

// Configuração do Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Inicia o servidor na porta 3000
app.Run("http://localhost:3000");
