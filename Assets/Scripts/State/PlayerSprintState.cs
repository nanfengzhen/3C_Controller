using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSprintState : PlayerStateBase
{
    public PlayerSprintState(PlayerMovementController movementController, PlayerComboController comboController) : base(movementController, comboController)
    {
    }

    public override void OnEnter()
    {
        movementController.animator.CrossFade("行走动画", 0.25f, 0, 0);
        movementController.animator.CrossFade("行走动画物理", 0.25f, 1, 0);    //这个播放方式不能轮询触发，否则人物就会卡住不动
    }

    public override void OnUpdate()
    {
        if (PlayerInputManager.MainInstance._attack)
        {
            PlayerFSM.MainInstance.SetState(StateType.Attack);
        }
        if (!movementController.isGround && movementController.characterHeight >1f)
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
            if (movementController.isSprint)
            {
                if (movementController.CanJump() && PlayerInputManager.MainInstance._jump)
                {
                    PlayerFSM.MainInstance.SetState(StateType.SprintJump);
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


