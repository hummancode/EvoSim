public interface IEnergySystem : IAgentComponent, IEnergyProvider
{
    float CurrentEnergy { get; }
    float MaxEnergy { get; }
    float EnergyPercent { get; }
    bool IsDead { get; }

    void AddEnergy(float amount);
    void ConsumeEnergy(float amount);
    void SetMaxEnergy(float value);
    void SetConsumptionRate(float value);

    event System.Action OnDeath;
}

