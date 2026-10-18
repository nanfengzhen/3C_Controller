using UnityEngine;

public class PlayerLandingState : PlayerStateBase
{
    public PlayerLandingState(PlayerMovementController movementController, PlayerComboController comboController) : base(movementController, comboController) { }

    public override void OnEnter()
    {
        movementController.EnterState(StateType.Landing);
        movementController.animator.CrossFade("DropDown", 0.13f, 0, 0);
        movementController.animator.CrossFade("DropDown", 0.13f, 1, 0);
        movementController.animator.SetInteger("Height", movementController.characterHeight > 4f ? 8 : 4);
    }

    public override void OnExit() => movementController.ExitState(StateType.Landing);

    public override void OnUpdate()
    {
        if (movementController.isGround)
        {
            PlayerFSM.MainInstance.SetState(StateType.LandingBuffer);
        }
    }
}
