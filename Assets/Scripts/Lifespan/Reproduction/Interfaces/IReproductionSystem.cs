public interface IReproductionSystem : IAgentComponent, IReproductionCapable
{
    void Initialize(IAgent self, IMateFinder finder, IEnergyProvider energy);
}