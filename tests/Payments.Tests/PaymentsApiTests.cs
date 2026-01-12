namespace Payments.Tests;

public class PaymentsApiTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly HttpClient _client;
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder().Build();
    private readonly RedisContainer _redis = new RedisBuilder().Build();

    public PaymentsApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Заменить connection strings на Testcontainers
                services.RemoveAll<Testcontainers>();
                services.AddDbContext<PaymentDbContext>(options =>
                    options.UseNpgsql(_postgres.GetConnectionString()));
            });
        }).CreateClient();
    }

    [Fact]
    public async Task PostInitiatePayment_ReturnsAccepted()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/pay/initiate", new
        {
            userId = "550e8400-e29b-41d4-a716-446655440000",
            amount = 100m,
            currency = "RUB",
            cardToken = "tok_visa"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task RateLimitExceeded_Returns429()
    {
        // Arrange: 11 запросов подряд
        var tasks = new List<Task>();
        for (int i = 0; i < 11; i++)
        {
            tasks.Add(_client.PostAsJsonAsync("/pay/initiate", new { /*...*/ }));
        }
        await Task.WhenAll(tasks);

        // Act: 12й запрос
        var response = await _client.PostAsJsonAsync("/pay/initiate", new { /*...*/ });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }
}