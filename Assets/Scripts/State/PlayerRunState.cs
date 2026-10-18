using GGG.Tool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRunState :PlayerStateBase
{
    public PlayerRunState(PlayerMovementController movementController, PlayerComboController comboController) : base(movementController, comboController)
    {
    }

    public override void OnEnter()
    {
        movementController.EnterState(StateType.Run);

        movementController.animator.CrossFade("行走动画", 0.25f, 0, 0);
        movementController.animator.CrossFade("行走动画物理", 0.25f, 1, 0);

        
        
    }


    public override void OnExit()
    {
        movementController.ExitState(StateType.Run);
    }


    public override void OnUpdate()
    {

        if (PlayerInputManager.MainInstance._attack)
        {
            PlayerFSM.MainInstance.SetState(StateType.Attack);
            return;
        }
        if(!movementController.isGround && movementController.characterHeight>1f)
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
            if (movementController.isDash)
            {
                PlayerFSM.MainInstance.SetState(StateType.Dash);
                return;
            }
            if (!movementController.isDash)
            {
                if (movementController.CanJump() && PlayerInputManager.MainInstance._jump)
                {
                    PlayerFSM.MainInstance.SetState(StateType.RunJump);
                    return;
                }
            }
        }

        else
        {
            PlayerFSM.MainInstance.SetState(StateType.Idle);  
        }



    }


}
