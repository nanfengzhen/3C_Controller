using GGG.Tool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRunJumpState :PlayerStateBase
{
    public PlayerRunJumpState(PlayerMovementController movementController, PlayerComboController comboController) : base(movementController, comboController)
    {
    }

    public override void OnEnter()
    {
        movementController.EnterState(StateType.RunJump);
        // 1. 【关键修改】在这里直接进行随机赋值，确保在播放前值已经改变
        // 或者你可以调用 Player.MainInstance.RandomizeFeet() 如果你把逻辑封装了
        float randomVal = UnityEngine.Random.Range(0, 100);
        float feetVal = (randomVal < 50) ? -1f : 1f;
        // 把值传给 Animator (这样参数就同步了)
        movementController.animator.SetFloat("左右脚", feetVal);
        if (movementController.animator.GetFloat("左右脚") > 0.5f)
        {
            movementController.animator.Play("RunJumpL");
        }
        else if(movementController.animator.GetFloat("左右脚") < -0.5f)
        {
            movementController.animator.Play("RunJumpR");
        }

        movementController.animator.Play("奔跑跳跃");

    }

    public override void OnExit()
    {
        movementController.ExitState(StateType.RunJump);
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
            return;
        }
        if (movementController.HasMoveInput)
        {

            if (movementController.isWalk)
            {
                PlayerFSM.MainInstance.SetState(StateType.Walk);
                return;
            }
            if (movementController.isDash)
            {
                PlayerFSM.MainInstance.SetState(StateType.Dash);
                return;
            }
            if (movementController.isSprint)
            {
                PlayerFSM.MainInstance.SetState(StateType.Sprint);
                return;
            }
            if (movementController.LoadingBufferTime() && movementController.isGround && !movementController.isWalk)
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
            else
            {
                PlayerFSM.MainInstance.SetState(StateType.Idle);
            }

        }
    }

}
