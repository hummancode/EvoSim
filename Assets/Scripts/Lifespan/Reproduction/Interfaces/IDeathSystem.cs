public interface IDeathSystem : IAgentComponent
{
    void Die(string cause);
    event System.Action<string> OnDeath;
}