using Confluent.Kafka;
using Shared;

public interface IPaymentEventProducer
{
    Task ProducePaymentRequestedAsync(Guid transactionId, InitiatePaymentRequest request);
}

public class KafkaProducer : IPaymentEventProducer
{
    private readonly IProducer<Null, string> _producer;

    public KafkaProducer(IConfiguration config)      
    {
        var configDict = new ProducerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"] ?? "localhost:9092"
        };
        _producer = new ProducerBuilder<Null, string>(configDict).Build();
    }

    public async Task ProducePaymentRequestedAsync(Guid transactionId, InitiatePaymentRequest request)
    {
        var json = $$"""{"transactionId":"{{transactionId}}","userId":"{{request.UserId}}","amount":{{request.Amount}}}""";
        await _producer.ProduceAsync("payments-requested", new Message<Null, string> { Value = json });
    }
}
