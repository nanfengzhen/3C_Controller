using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSprintJumpState : PlayerStateBase
{
    public PlayerSprintJumpState(PlayerMovementController movementController, PlayerComboController comboController) : base(movementController, comboController)
    {
    }

    public override void OnEnter()
    {
        movementController.EnterState(StateType.SprintJump);
        float randomVal = Random.Range(0, 100);
        float feetVal = (randomVal < 50) ? -1f : 1f;
        movementController.animator.SetFloat("左右脚", feetVal);
        if (movementController.animator.GetFloat("左右脚") > 0.5f)
        {
            movementController.animator.Play("SprintJumpL");
        }
        else if (movementController.animator.GetFloat("左右脚") < -0.5f)
        {
            movementController.animator.Play("SprintJumpR");
        }

        movementController.animator.Play("冲刺跳跃");

    }

    public override void OnExit()
    {
        movementController.ExitState(StateType.SprintJump);
    }

    //public override void OnExit()
    //{
    //    throw new System.NotImplementedException();
    //}

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
            if (movementController.LoadingBufferTime() && movementController.isGround && movementController.isSprint)
            {
                PlayerFSM.MainInstance.SetState(StateType.Sprint);
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
