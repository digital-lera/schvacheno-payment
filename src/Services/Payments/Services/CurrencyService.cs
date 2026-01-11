using Microsoft.Extensions.Caching.Distributed;

public class CurrencyService
{
    private readonly IDistributedCache _cache;
    private readonly HttpClient _http;

    public CurrencyService(IDistributedCache cache, HttpClient http)
    {
        _cache = cache;
        _http = http;
    }
    public async Task<decimal> GetRateAsync(string from, string to)
    {
        var key = $"rate:{from}:{to}";
        var cached = await _cache.GetStringAsync(key);
        
        if (cached != null) return decimal.Parse(cached);

        // CBRF API (или mock)
        var rate = 95.5m; // USD/RUB
        await _cache.SetStringAsync(key, rate.ToString(), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        });

        return rate;
    }
}