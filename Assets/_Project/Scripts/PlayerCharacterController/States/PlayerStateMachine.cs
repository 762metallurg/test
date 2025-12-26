public class PlayerStateMachine
{
    public PlayerBaseState CurrentState { get; private set; }

    public void Initialize(PlayerBaseState startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter();
    }

    public void ChangeState(PlayerBaseState newState)
    {
        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }
}