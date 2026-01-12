namespace Payments.UnitTests;

public class PaymentSagaTests{
    private readonly Mock<PaymentDbContext> _mockDb;
    private readonly Mock<IPaymentEventProducer> _mockProducer;
    private readonly PaymentSaga _saga;

    public PaymentSagaTests()
    {
        _mockDb = new Mock<PaymentDbContext>();
        _mockProducer = new Mock<IPaymentEventProducer>();
        _saga = new PaymentSaga(_mockDb.Object, _mockProducer.Object, null!);
    }

    [Fact]
    public async Task DailyLimitExceeded_ReturnsFailed()
    {
        // Arrange
        var request = new InitiatePaymentRequest("user1", 110000m, "RUB", "tok_visa");
        
        _mockDb.Setup(x => x.Transactions
            .Where(It.IsAny<Expression<Func<Transaction, bool>>>())
            .SumAsync(It.IsAny<Expression<Func<Transaction, decimal>>>())
        ).ReturnsAsync(95000m); // Уже потрачено

        // Act
        var result = await _saga.ProcessAsync(request);

        // Assert
        result.Should().Be(PaymentStatusEnum.Failed);
    }
}
