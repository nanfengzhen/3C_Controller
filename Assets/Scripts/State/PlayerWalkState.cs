using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalkState :PlayerStateBase
{
    public PlayerWalkState(PlayerMovementController movementController, PlayerComboController comboController) : base(movementController, comboController)
    {
    }

    public override void OnEnter()
    {
        movementController.EnterState(StateType.Walk);
        movementController.animator.CrossFade("行走动画", 0.25f, 0, 0);
        movementController.animator.CrossFade("行走动画物理", 0.25f, 1, 0);
    }

    public override void OnExit()
    {
        movementController.ExitState(StateType.Walk);
    }


    public override void OnUpdate()
    {
        if (PlayerInputManager.MainInstance._attack)
        {
            PlayerFSM.MainInstance.SetState(StateType.Attack);
            return;
        }
        if (!movementController.isGround)
        {
            PlayerFSM.MainInstance.SetState(StateType.Landing);
            return;
        }
        if (movementController.HasMoveInput)
        {

            if (movementController.isWalk)
            {
                if (movementController.CanJump() && PlayerInputManager.MainInstance._jump)
                {
                    PlayerFSM.MainInstance.SetState(StateType.WalkJump);
                    return;
                }
            }
            if (movementController.isDash)
            {
                PlayerFSM.MainInstance.SetState(StateType.Dash);
                return;
            }
            if (movementController.isSprint)
            {
                if (movementController.CanJump() && PlayerInputManager.MainInstance._jump)
                {
                    PlayerFSM.MainInstance.SetState(StateType.SprintJump);
                }
                else
                {
                    PlayerFSM.MainInstance.SetState(StateType.Sprint);
                }
                return;
            }
            if (!movementController.isWalk)
            {
                if (movementController.CanJump() && PlayerInputManager.MainInstance._jump)
                {
                    PlayerFSM.MainInstance.SetState(StateType.RunJump);
                }
                else
                {
                    PlayerFSM.MainInstance.SetState(StateType.Run);
                }
                return;
            }
        }
        else
        {
            if (movementController.CanJump() && PlayerInputManager.MainInstance._jump)
            {
                PlayerFSM.MainInstance.SetState(StateType.Jump);
            }
            else
            {
                PlayerFSM.MainInstance.SetState(StateType.Idle);
            }

        }
    }

}
