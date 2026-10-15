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
        if (!movementController.isGround)
        {
            PlayerFSM.MainInstance.SetState(StateType.Landing);
        }
        if (movementController.animator.GetBool("HasInput"))
        {

            if (movementController.isWalk)
            {
                if (movementController.CanJump() && PlayerInputManager.MainInstance._jump)
                {
                    PlayerFSM.MainInstance.SetState(StateType.WalkJump);
                }
            }
            if (movementController.isDash)
            {
                PlayerFSM.MainInstance.SetState(StateType.Dash);
            }
            if (movementController.isSprint)
            {
                PlayerFSM.MainInstance.SetState(StateType.Sprint);
                if (movementController.CanJump() && PlayerInputManager.MainInstance._jump)
                {
                    PlayerFSM.MainInstance.SetState(StateType.SprintJump);
                }
            }
            if (movementController.animator.GetBool("IsRun"))
            {
                PlayerFSM.MainInstance.SetState(StateType.Run);
                if (movementController.CanJump() && PlayerInputManager.MainInstance._jump)
                {
                    PlayerFSM.MainInstance.SetState(StateType.RunJump);
                }
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