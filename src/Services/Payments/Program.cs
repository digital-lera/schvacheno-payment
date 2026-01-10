
using Microsoft.EntityFrameworkCore;
using Payments.Data;
using Shared;
using Payments.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddSingleton<IPaymentEventProducer, KafkaProducer>();  


var app = builder.Build();

app.MapGet("/", () => "This is Payments Service");


app.MapPost("/pay/initiate", async (InitiatePaymentRequest req, PaymentDbContext db) =>
{
    var transaction = new Transaction
    {
        UserId = req.UserId,
        Amount = req.Amount,
        Currency = req.Currency,
        CardLast4 = req.CardToken[^4..],  // Маскируем карту
        Status = PaymentStatusEnum.Initiated
    };
    
    db.Transactions.Add(transaction);     
    await db.SaveChangesAsync();        

    var producer = app.Services.GetRequiredService<IPaymentEventProducer>();
    await producer.ProducePaymentRequestedAsync(transaction.Id, req);

    return Results.Accepted("202"); // 202
});

app.MapGet("/pay/status/{id:guid}", async (Guid id, PaymentDbContext db) =>
    await db.Transactions
        .Where(t => t.Id == id)
        .Select(t => new { t.Id, t.Status, t.Amount }) 
        .FirstOrDefaultAsync() 
    is var status
        ? Results.Ok(status)                           // 200
        : Results.NotFound());                         // 404

app.Run();

