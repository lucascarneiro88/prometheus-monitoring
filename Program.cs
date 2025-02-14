using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prometheus;
using System;
using System.Threading;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Contador de requisições com statusCode
var contadorRequisicoes = Metrics.CreateCounter("aula_requests_total", "Contador de requests", new CounterConfiguration
{
    LabelNames = new[] { "statusCode" }
});

// Gauge para usuários logados
var usuariosOnline = Metrics.CreateGauge("aula_usuarios_logados_total", "Número de usuários logados no momento");

// Histograma para tempo de resposta
var tempoDeResposta = Metrics.CreateHistogram("aula_request_duration_seconds", "Tempo de resposta da API");

bool zeraUsuariosLogados = false;
Random random = new Random();

// Função para simular distribuição estatística semelhante ao randn_bm do JS
static double RandnBm(double min, double max, double skew)
{
    Random random = new Random();
    double u = 0, v = 0;
    while (u == 0) u = random.NextDouble();
    while (v == 0) v = random.NextDouble();
    double num = Math.Sqrt(-2.0 * Math.Log(u)) * Math.Cos(2.0 * Math.PI * v);
    num = num / 10.0 + 0.5;
    if (num > 1 || num < 0) return RandnBm(min, max, skew);
    num = Math.Pow(num, skew);
    num *= max - min;
    num += min;
    return num;
}

// Atualiza métricas a cada 150ms
var timer = new Timer(_ =>
{
    var statusCode = (random.Next(0, 100) < 5) ? "500" : "200";
    contadorRequisicoes.Labels(statusCode).Inc();

    var usuariosLogados = (zeraUsuariosLogados) ? 0 : 500 + random.Next(0, 50);
    usuariosOnline.Set(usuariosLogados);

    var tempoObservado = RandnBm(0, 3, 4);
    tempoDeResposta.Observe(tempoObservado);
}, null, 0, 150);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🚀 REGISTRANDO ROTAS NO NÍVEL SUPERIOR
app.MapGet("/", () => "Hello World!");
app.MapGet("/zera-usuarios-logados", () =>
{
    zeraUsuariosLogados = true;
    return "OK";
});
app.MapGet("/retorna-usuarios-logados", () =>
{
    zeraUsuariosLogados = false;
    return "OK";
});

// Endpoint para métricas do Prometheus
app.UseMetricServer();

app.Run("http://localhost:3030");
