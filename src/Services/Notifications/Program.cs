using Confluent.Kafka;

var builder = Host.CreateApplicationBuilder(args);  
builder.Services.AddLogging(x => x.AddConsole());

var host = builder.Build();

_ = Task.Run(() =>
{
    var consumerConfig = new ConsumerConfig
    {
        BootstrapServers = "kafka:29092",     
        GroupId = "notifications-group",      
        AutoOffsetReset = AutoOffsetReset.Earliest
    };

    using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
    consumer.Subscribe("payments-requested");
    
    var consumeResult = consumer.Consume(TimeSpan.FromSeconds(1));

    while (consumeResult != null)
    {
        var msg = consumeResult;
        consumeResult = consumer.Consume(TimeSpan.FromSeconds(1));

        Console.WriteLine($"PAYMENT EVENT: {msg.Message.Value}");
        consumer.Commit(msg);    
    }
});

await host.RunAsync();  
