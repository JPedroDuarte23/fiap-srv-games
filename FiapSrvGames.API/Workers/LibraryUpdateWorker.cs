namespace FiapSrvGames.API.Workers;

using Amazon.SQS;
using Amazon.SQS.Model;
using FiapSrvGames.Application.Interfaces;
using FiapSrvGames.Application.DTOs;
using System.Text.Json;

public class LibraryUpdateWorker : BackgroundService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly IServiceProvider _serviceProvider; // Necessário para criar escopo
    private readonly ILogger<LibraryUpdateWorker> _logger;
    private readonly string _queueUrl;

    public LibraryUpdateWorker(IAmazonSQS sqsClient, IServiceProvider serviceProvider, ILogger<LibraryUpdateWorker> logger, IConfiguration configuration)
    {
        _sqsClient = sqsClient;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _queueUrl = configuration["AWS:SqsQueueUrl"] ?? throw new ArgumentNullException("Url da fila não configurada");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando Worker de Atualização de Biblioteca...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var request = new ReceiveMessageRequest
                {
                    QueueUrl = _queueUrl,
                    MaxNumberOfMessages = 10,
                    WaitTimeSeconds = 20
                };

                var response = await _sqsClient.ReceiveMessageAsync(request, stoppingToken);

                foreach (var message in response.Messages)
                {
                    await ProcessMessageAsync(message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem da fila SQS.");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task ProcessMessageAsync(Message message)
    {
        try
        {
            var snsEnvelope = JsonDocument.Parse(message.Body);
            var innerMessage = snsEnvelope.RootElement.GetProperty("Message").GetString();

            var checkoutData = JsonSerializer.Deserialize<CheckoutEventDto>(innerMessage);

            if (checkoutData != null)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var libraryService = scope.ServiceProvider.GetRequiredService<ILibraryService>();

                    await libraryService.AddToLibraryAsync(checkoutData.UserId, checkoutData.GameIds);
                }

                await _sqsClient.DeleteMessageAsync(_queueUrl, message.ReceiptHandle);
                _logger.LogInformation($"Jogos adicionados para usuário {checkoutData.UserId}");
            }
        }   
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao processar mensagem específica.");
        }
    }
}
