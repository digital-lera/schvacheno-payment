using Microsoft.AspNetCore.DataProtection;
using System.Text;

public interface ICardEncryption
{
    string Encrypt(string cardToken);
    string Decrypt(string encryptedToken);
}

public class DataProtectionCardEncryption : ICardEncryption
{
    private readonly IDataProtector _protector;

    public DataProtectionCardEncryption(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("Schvacheno.v1.CardData");
    }

    public string Encrypt(string cardToken) => _protector.Protect(cardToken); 
    public string Decrypt(string encryptedToken) => _protector.Unprotect(encryptedToken);
}
