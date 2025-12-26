public class PlayerActionNoneState : PlayerBaseState
{
    public PlayerActionNoneState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        // Когда мы ничего не делаем руками, ноги свободны
        player.lockMovement = false; 
    }
}