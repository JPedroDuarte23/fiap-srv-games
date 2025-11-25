namespace FiapSrvGames.API.Workers;

using Amazon.SQS;
using Amazon.SQS.Model;
using FiapSrvGames.Application.Interfaces;
using FiapSrvGames.Application.DTOs;
using System.Text.Json;

public class LibraryUpdateWorker : BackgroundService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly IServiceProvider _serviceProvider;
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
                    WaitTimeSeconds = 20,
                    VisibilityTimeout = 30
                };

                var response = await _sqsClient.ReceiveMessageAsync(request, stoppingToken);

                foreach (var message in response.Messages)
                {
                    await ProcessSingleMessageAsync(message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem da fila SQS.");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task ProcessSingleMessageAsync(Message message)
    {
        try
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var libraryService = scope.ServiceProvider.GetRequiredService<ILibraryService>();

                // 1. Parse da mensagem (Desembrulhando o JSON do SNS)
                var snsEnvelope = JsonDocument.Parse(message.Body);
                var innerMessageJson = snsEnvelope.RootElement.GetProperty("Message").GetString();

                var checkoutEvent = JsonSerializer.Deserialize<CheckoutEventDto>(innerMessageJson);

                if (checkoutEvent != null)
                {
                    // 2. Tenta executar a lógica de negócio (Mongo)
                    await libraryService.AddToLibraryAsync(checkoutEvent.UserId, checkoutEvent.GameIds);

                    // 3. SUCESSO: Só deletamos a mensagem SE a linha de cima funcionar
                    await _sqsClient.DeleteMessageAsync(_queueUrl, message.ReceiptHandle);

                    _logger.LogInformation("Fulfillment concluído para User {UserId}. Jogos adicionados.", checkoutEvent.UserId);
                }
            }
        }   
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao processar mensagem {MessageId}. Mensagem retornará para fila (Retry).", message.MessageId);
        }
    }
}
