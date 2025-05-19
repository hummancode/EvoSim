using UnityEngine;

public class AgentAdapter : IAgent
{
    private AgentController controller;

    public AgentAdapter(AgentController controller)
    {
        this.controller = controller;
    }

    public GameObject GameObject => controller.gameObject;
    public Vector3 Position => controller.transform.position;

    public IReproductionCapable ReproductionSystem =>
        controller.GetComponent<ReproductionSystem>() as IReproductionCapable;

    public IEnergyProvider EnergySystem =>
        controller.GetComponent<EnergySystem>() as IEnergyProvider;
}