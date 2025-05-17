
using UnityEngine;


public class AgentContext
{
    public GameObject Agent { get; set; }
    public MovementSystem Movement { get; set; }
    public SensorSystem Sensor { get; set; }
    public EnergySystem Energy { get; set; }
    public ReproductionSystem Reproduction { get; set; }
}