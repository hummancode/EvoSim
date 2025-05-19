


using UnityEngine;

public class InitiateMatingCommand : IReproductionCommand
{
    private readonly IAgent initiator;
    private readonly IAgent partner;
    private readonly MatingCoordinator coordinator;

    public InitiateMatingCommand(IAgent initiator, IAgent partner, MatingCoordinator coordinator)
    {
        this.initiator = initiator;
        this.partner = partner;
        this.coordinator = coordinator;
    }

    public void Execute()
    {
        if (coordinator != null)
        {
            Debug.Log("Executing InitiateMatingCommand");
            coordinator.RegisterMating(initiator, partner);
        }
        else
        {
            Debug.LogError("Cannot execute InitiateMatingCommand: coordinator is null");
        }
    }
}
