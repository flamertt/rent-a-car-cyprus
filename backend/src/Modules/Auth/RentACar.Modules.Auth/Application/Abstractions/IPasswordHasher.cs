namespace RentACar.Modules.Auth.Application.Abstractions;

internal interface IPasswordHasher
{
    string Hash(string password);

    /// <summary>
    /// Özet boşsa (kullanıcı yok) yine de bir özet doğrulanır ve false döner: yanıt süresi e-postanın
    /// kayıtlı olup olmadığını ele vermesin.
    /// </summary>
    bool Verify(string? passwordHash, string password, out bool needsRehash);
}
