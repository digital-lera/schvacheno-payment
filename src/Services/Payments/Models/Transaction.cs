namespace Payments.Models;
using Shared;

public class Transaction                    // PostgreSQL таблица
{
    public Guid Id { get; set; } = Guid.NewGuid();     // PK
    public Guid UserId { get; set; }                   // FK для лимитов
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "RUB";
    public PaymentStatusEnum Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CardLast4 { get; set; } = "";        // Последние 4 цифры карты (маскировка)
    public string EncryptedCardData { get; set; } = "";// Зашифрованные данные карты
}
