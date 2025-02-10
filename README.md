# Monitoramento com Prometheus em Aplicações ASP.NET Core

Este documento descreve o processo de implementação de métricas com Prometheus em um projeto ASP.NET Core, incluindo contadores, gauges e histogramas. Além disso, documenta os passos para versionamento do código no GitHub.
**Referência:** Este projeto utiliza a biblioteca [prometheus-net](https://github.com/prometheus-net/prometheus-net) para expor métricas no formato Prometheus.

---

## 1️⃣ Configuração Inicial do Projeto

### 1.1 Criando um Novo Projeto ASP.NET Core

Navegue até a pasta onde deseja criar o projeto e execute o comando:

```bash
dotnet new web -o app
```
```bash
cd app
```
Isso criará um projeto básico do ASP.NET Core dentro da pasta app.

2️⃣ Adicionando Suporte ao Prometheus
2.1 Instalando o Pacote Prometheus-net
Para integrar o Prometheus ao projeto, instale o pacote prometheus-net com o comando:

```bash
dotnet add package prometheus-net.AspNetCore
```

Isso permitirá a coleta de métricas dentro da API.

2.2 Criando o Código Base
Edite o arquivo Program.cs para incluir a coleta de métricas. O código atualizado ficará assim:
```bash
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Middleware para expor métricas no endpoint /metrics
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapMetrics(); 
});

// Configuração do Swagger para documentação da API
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Rota principal
app.MapGet("/", () => "Hello World!");

// Inicializa o servidor na porta 3000
app.Run("http://localhost:3000");

```

Agora, sempre que a API for acessada, as métricas ficarão disponíveis em http://localhost:3000/metrics.

3️⃣ Criando Métricas Personalizadas
3.1 Contador de Requisições
Crie um contador para monitorar o número total de requisições à API:

```bash
var requestCounter = Metrics.CreateCounter("aula_request_total", "Contador de requests recebidos");

app.MapGet("/", () =>
{
    requestCounter.Inc(); // Incrementa o contador a cada requisição
    return "Hello World!";
});

```

Isso fará com que o Prometheus armazene quantas requisições foram feitas à API.

3.2 Adicionando Labels (Status Code)
Para diferenciar requisições bem-sucedidas e falhas, adicione um label ao contador:

```bash
var requestCounter = Metrics.CreateCounter("aula_request_total", "Contador de requests recebidos", new CounterConfiguration
{
    LabelNames = new[] { "status_code" }
});

app.MapGet("/", () =>
{
    requestCounter.Labels("200").Inc(); // Incrementa com status 200
    return "Hello World!";
});

```

Agora, a métrica exibirá o status code de cada requisição:


# HELP aula_request_total Contador de requests
# TYPE aula_request_total counter
aula_request_total{status_code="200"} 5



3.3 Criando um Gauge para Monitoramento de Bytes
Crie um Gauge para simular o monitoramento de free_bytes:

```bash
var bytesProcessed = Metrics.CreateGauge("free_bytes", "Quantidade de bytes livres no sistema");

app.MapGet("/memory", () =>
{
    var randomBytes = new Random().Next(1000, 5000);
    bytesProcessed.Set(randomBytes); // Atualiza o valor do gauge
    return $"Bytes livres: {randomBytes}";
});

```

A métrica free_bytes agora mostrará valores dinâmicos de bytes disponíveis.

3.4 Criando um Histograma para Tempo de Resposta
Crie um histograma para medir o tempo de resposta da API:
```bash
var responseTimeHistogram = Metrics.CreateHistogram("response_time_seconds", "Tempo de resposta da API em segundos",
    new HistogramConfiguration
    {
        Buckets = Histogram.LinearBuckets(start: 0.1, width: 0.1, count: 10) // Buckets de 100ms a 1s
    });

app.MapGet("/delay", async () =>
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    await Task.Delay(new Random().Next(100, 1000)); // Simula um delay aleatório
    stopwatch.Stop();
    
    responseTimeHistogram.Observe(stopwatch.Elapsed.TotalSeconds); // Registra o tempo de resposta
    return $"Tempo de resposta: {stopwatch.Elapsed.TotalSeconds:F2} segundos";
});

```


Agora, http://localhost:3000/metrics exibirá estatísticas do tempo de resposta.



3.5 Testando a API no Prometheus
Suba o servidor da API:
```bash
dotnet run

```

3.6 Acesse o endpoint de métricas:

[URL: http://localhost:3000/metrics](http://localhost:3000/metrics)
Suba o servidor da API:
```bash
dotnet run

```



3.7 Configurar o Prometheus para coletar dados:


No prometheus.yml, adicione:
```bash
global:
  scrape_interval: 30s

scrape_configs:
  - job_name: prometheus
    static_configs:
      - targets: ['localhost:3000']


```

Agora, o Prometheus começará a coletar os dados da API! 🚀





