public interface IMovementSystem : IAgentComponent
{
    void SetMovementStrategy(IMovementStrategy strategy);
    void SetSpeed(float speed);
    void StartMoving();
    void StopMoving();
}