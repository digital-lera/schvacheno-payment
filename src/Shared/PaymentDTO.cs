namespace Shared; 

public record InitiatePaymentRequest(           // Входной JSON от клиента
    Guid UserId, decimal Amount, string Currency, string CardToken);

public record PaymentStatus(                    // Ответ клиенту
    Guid TransactionId, string Status, decimal Amount);

public enum PaymentStatusEnum  {           // БД enum для статусов
    Initiated, 
    Processing, 
    Completed, 
    Failed 
}
