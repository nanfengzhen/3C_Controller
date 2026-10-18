using GGG.Tool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerStateBase
{
    public PlayerIdleState(PlayerMovementController movementController, PlayerComboController comboController) : base(movementController, comboController)
    {
    }

    public override void OnEnter()
    {
        movementController.EnterState(StateType.Idle);
        movementController.animator.CrossFade("Idle", 0.13f,0,0);
    }

    public override void OnExit()
    {
        movementController.ExitState(StateType.Idle);
    }


    public override void OnUpdate()
    {
        
        if (PlayerInputManager.MainInstance._attack || PlayerInputManager.MainInstance.rightAttack)
        {
            PlayerFSM.MainInstance.SetState(StateType.Attack);
            return;
        }
        if (!movementController.isGround && movementController.characterHeight > 2f)
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
                }
                else
                {
                    PlayerFSM.MainInstance.SetState(StateType.Walk);
                }
                return;
            }

            if (!movementController.isWalk)
            {
                PlayerFSM.MainInstance.SetState(StateType.Run);
                return;
            }
        }
        else
        {
            if (movementController.CanJump() && PlayerInputManager.MainInstance._jump)
            {
                PlayerFSM.MainInstance.SetState(StateType.Jump);
            }

        }



    }


}
