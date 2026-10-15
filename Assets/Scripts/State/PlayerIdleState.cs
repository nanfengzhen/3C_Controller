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
        movementController.animator.CrossFade("Idle", 0.13f,0,0);
    }

    public override void OnExit()
    {

    }


    public override void OnUpdate()
    {
        
        if (PlayerInputManager.MainInstance._attack || PlayerInputManager.MainInstance.rightAttack)
        {
            PlayerFSM.MainInstance.SetState(StateType.Attack);
        }
        if (!movementController.isGround && movementController.characterHeight > 2f)
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

            if (movementController.animator.GetBool("IsRun"))
            {
                PlayerFSM.MainInstance.SetState(StateType.Run);
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
