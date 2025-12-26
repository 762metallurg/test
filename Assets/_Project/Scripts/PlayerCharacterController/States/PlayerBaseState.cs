public abstract class PlayerBaseState
{
    protected PlayerController player;
    protected PlayerStateMachine stateMachine;

    public PlayerBaseState(PlayerController player, PlayerStateMachine stateMachine)
    {
        this.player = player;
        this.stateMachine = stateMachine;
    }

    public virtual void Enter() { }
    public virtual void LogicUpdate() { }
    public virtual void PhysicsUpdate() { }
    public virtual void Exit() { }
}