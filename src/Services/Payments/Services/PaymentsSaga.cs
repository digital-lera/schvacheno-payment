using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Payments.Data;
using Payments.Models;
using Shared;

public class PaymentSaga
{
    private readonly PaymentDbContext _db;
    private readonly IPaymentEventProducer _producer;
    private readonly IDistributedCache _cache;

    public PaymentSaga(PaymentDbContext db, IPaymentEventProducer producer, IDistributedCache cache)
    {
        _db = db; _producer = producer; _cache = cache;
    }

    public async Task<PaymentStatusEnum> ProcessAsync(Transaction transaction)
    {
        // Шаг 1: Блокировка сессии (Redis)
        var lockKey = $"user:{transaction.UserId}:lock";
        var acquired = await _cache.GetStringAsync(lockKey) == null;
        
        if (!acquired)
            return PaymentStatusEnum.Failed; // Параллельный платеж

        await _cache.SetStringAsync(lockKey, "locked", new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });

        try
        {
            if (!await CheckDailyLimitAsync(transaction))
                return PaymentStatusEnum.Failed;

            transaction.Status = PaymentStatusEnum.Processing;
            _db.Add(transaction);
            await _db.SaveChangesAsync();


            await _producer.ProducePaymentRequestedAsync(transaction.Id, 
                new(transaction.UserId, transaction.Amount, transaction.Currency, "****"));

            return PaymentStatusEnum.Processing;
        }
        finally
        {
            await _cache.RemoveAsync(lockKey); 
        }
    }

    private async Task<bool> CheckDailyLimitAsync(Transaction tx)
    {
        var today = DateTime.UtcNow.Date;
        var limit = 100000m; // 100k RUB/day
        
        var todaySpent = await _db.Transactions
            .Where(t => t.UserId == tx.UserId && t.CreatedAt.Date == today)
            .SumAsync(t => t.Amount);

        return (todaySpent + tx.Amount) <= limit;
    }
}