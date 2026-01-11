public class PaymentSagaTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PaymentSagaTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ProcessAsync_Should_Fail_When_Daily_Limit_Exceeded()
    {
        // Arrange
        var client = _factory.CreateClient();
        var db = _factory.Services.GetRequiredService<PaymentDbContext>();
        var saga = _factory.Services.GetRequiredService<PaymentSaga>();

        var userId = Guid.NewGuid();

        // Добавляем транзакции, чтобы превысить лимит
        db.Transactions.AddRange(new[]
        {
            new Transaction { UserId = userId, Amount = 60000m, Status = PaymentStatusEnum.Completed },
            new Transaction { UserId = userId, Amount = 50000m, Status = PaymentStatusEnum.Completed }
        });
        await db.SaveChangesAsync();

        var newTransaction = new Transaction
        {
            UserId = userId,
            Amount = 10000m,
            Currency = "RUB",
            Status = PaymentStatusEnum.Pending
        };

        // Act
        var result = await saga.ProcessAsync(newTransaction);

        // Assert
        Assert.Equal(PaymentStatusEnum.Failed, result);
    }
}