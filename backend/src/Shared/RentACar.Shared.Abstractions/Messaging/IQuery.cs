namespace RentACar.Shared.Abstractions.Messaging;

/// <summary>Yalnız okuyan kullanım senaryosu (CQRS okuma tarafı): LINQ projeksiyonu, değişiklik izleme yok.</summary>
public interface IQuery<TResult>;
