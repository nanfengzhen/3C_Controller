using UnityEngine;

public class PlayerLandingBufferState : PlayerStateBase
{
    public PlayerLandingBufferState(PlayerMovementController movementController, PlayerComboController comboController) : base(movementController, comboController) { }

    public override void OnEnter() => movementController.EnterState(StateType.LandingBuffer);
    public override void OnExit() => movementController.ExitState(StateType.LandingBuffer);

    public override void OnUpdate()
    {
        movementController.UpdateLandingBuffer(Time.deltaTime);
        if (!movementController.isLanded) return;
        if (PlayerInputManager.MainInstance._attack || PlayerInputManager.MainInstance.rightAttack) { PlayerFSM.MainInstance.SetState(StateType.Attack); return; }
        if (!movementController.HasMoveInput) { PlayerFSM.MainInstance.SetState(StateType.Idle); return; }
        if (movementController.isDash) PlayerFSM.MainInstance.SetState(StateType.Dash);
        else if (movementController.isSprint) PlayerFSM.MainInstance.SetState(StateType.Sprint);
        else if (movementController.isWalk) PlayerFSM.MainInstance.SetState(StateType.Walk);
        else PlayerFSM.MainInstance.SetState(StateType.Run);
    }
}
