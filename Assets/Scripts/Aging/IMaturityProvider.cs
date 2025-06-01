/// <summary>
/// Interface for maturity checking - follows Interface Segregation
/// </summary>
public interface IMaturityProvider
{
    bool IsMature { get; }
    bool CanReproduce { get; }
}