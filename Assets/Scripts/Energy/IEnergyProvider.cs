public interface IEnergyProvider
{
    bool HasEnoughEnergyForMating { get; }
    bool IsHungry { get; }
    void ConsumeEnergy(float amount);
}
