using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Payments.Data;
using Shared;
using Payments.Models;
using AspNetCoreRateLimit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
});

builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddInMemoryRateLimiting();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("./keys"))
    .SetApplicationName("Schvacheno");
builder.Services.AddSingleton<ICardEncryption, DataProtectionCardEncryption>();


builder.Services.AddHttpClient();
builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));


builder.Services.AddSingleton<IPaymentEventProducer, KafkaProducer>();  


builder.Services.AddScoped<PaymentSaga>();
builder.Services.AddScoped<CurrencyService>();


var app = builder.Build();

app.UseIpRateLimiting(); 

app.MapGet("/", () => "Schvacheno Payments Service v.1.0.0");
app.MapPost("/test-db", async (PaymentDbContext db) =>
    {
        // Тест INSERT
        var testTx = new Transaction 
        { 
            UserId = Guid.NewGuid(), 
            Amount = 999.99m,
            Currency = "RUB"
        };
        
        db.Transactions.Add(testTx);
        await db.SaveChangesAsync();
        
        // Тест SELECT
        var count = await db.Transactions.CountAsync();
        
        return Results.Ok(new 
        { 
            message = "✅ DB Works!",
            createdId = testTx.Id,
            totalTransactions = count
        });
    });

app.MapPost("/pay/initiate", async (InitiatePaymentRequest req, 
    PaymentDbContext db, 
    PaymentSaga saga,
    ICardEncryption encryption) =>
    {
        var encryptedCard = encryption.Encrypt(req.CardToken);
        
        var transaction = new Transaction
        {
            UserId = req.UserId,
            Amount = req.Amount,
            Currency = req.Currency,
            CardLast4 = req.CardToken[^4..],
            EncryptedCardData = encryptedCard
        };

        var result = await saga.ProcessAsync(transaction);
        
        if (result == PaymentStatusEnum.Failed)
            return Results.Problem("Daily limit exceeded or concurrent payment");

        return Results.Accepted($"/pay/status/{transaction.Id}", new { transactionId = transaction.Id });
    }).DisableAntiforgery(); 


app.MapGet("/pay/status/{id:guid}", async (Guid id, PaymentDbContext db) =>
    await db.Transactions
        .Where(t => t.Id == id)
        .Select(t => new { t.Id, t.Status, t.Amount }) 
        .FirstOrDefaultAsync() 
    is var status
        ? Results.Ok(status)                           // 200
        : Results.NotFound());                         // 404

app.Run();

