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

        movementController.animator.CrossFade("行走动画", 0.25f, 0, 0);
        movementController.animator.CrossFade("行走动画物理", 0.25f, 1, 0);

        
        
    }


    public override void OnExit()
    {

    }


    public override void OnUpdate()
    {

        if (PlayerInputManager.MainInstance._attack)
        {
            PlayerFSM.MainInstance.SetState(StateType.Attack);
        }
        if(!movementController.isGround && movementController.characterHeight>1f)
        {
            PlayerFSM.MainInstance.SetState(StateType.Landing);
        }
        if (movementController.animator.GetBool("HasInput"))
        {

            if (movementController.isWalk)
            {
                PlayerFSM.MainInstance.SetState(StateType.Walk);
                if (movementController.CanJump() && PlayerInputManager.MainInstance._jump)
                {
                    PlayerFSM.MainInstance.SetState(StateType.WalkJump);
                }
            }
            if (movementController.isDash)
            {
                PlayerFSM.MainInstance.SetState(StateType.Dash);
            }
            if (movementController.animator.GetBool("IsRun") && !movementController.isDash)
            {
                if (movementController.CanJump() && PlayerInputManager.MainInstance._jump)
                {
                    PlayerFSM.MainInstance.SetState(StateType.RunJump);
                }
            }
        }

        else
        {
            PlayerFSM.MainInstance.SetState(StateType.Idle);  
        }



    }


}
