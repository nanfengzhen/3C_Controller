using GGG.Tool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerStateBase
{
    public PlayerJumpState(PlayerMovementController movementController, PlayerComboController comboController) : base(movementController, comboController)
    {
    }

    public override void OnEnter()
    {
        movementController.EnterState(StateType.Jump);
        float randomVal = Random.Range(0, 100);
        float feetVal = (randomVal < 50) ? -1f : 1f;
        // 把值传给 Animator (这样参数就同步了)
        movementController.animator.SetFloat("左右脚", feetVal);

        if (movementController.animator.GetFloat("左右脚") > 0.5f)
        {
            movementController.animator.Play("JumpL",0);
        }
        else if((movementController.animator.GetFloat("左右脚") < - 0.5f))
        {
            movementController.animator.Play("JumpR",0);
        }

        movementController.animator.Play("跳跃",1);
        //Debug.Log("进入跳跃状态");
    }

    public override void OnExit()
    {
        movementController.ExitState(StateType.Jump);
    }


    public override void OnUpdate()
    {

        //Debug.Log("进来的Jump");
        
        if (movementController.isGround)
        {
            if (PlayerInputManager.MainInstance._attack || PlayerInputManager.MainInstance.rightAttack)
            {
                PlayerFSM.MainInstance.SetState(StateType.Attack);
            }
            else if (movementController.HasMoveInput)
            {
                PlayerFSM.MainInstance.SetState(movementController.isWalk ? StateType.Walk : StateType.Run);
            }
            else
            {
                PlayerFSM.MainInstance.SetState(StateType.Idle);
            }
        }
        


    }
}


