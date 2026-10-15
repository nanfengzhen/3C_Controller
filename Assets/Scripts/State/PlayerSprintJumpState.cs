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
        float randomVal = Random.Range(0, 100);
        float feetVal = (randomVal < 50) ? -1f : 1f;
        movementController.animator.SetFloat("×óÓÒ½Å", feetVal);
        if (movementController.animator.GetFloat("×óÓÒ½Å") > 0.5f)
        {
            movementController.animator.Play("SprintJumpL");
        }
        else if (movementController.animator.GetFloat("×óÓÒ½Å") < -0.5f)
        {
            movementController.animator.Play("SprintJumpR");
        }

        movementController.animator.Play("³å´ÌÌøÔ¾");

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
            if (movementController.LoadingBufferTime() && movementController.isGround && movementController.isSprint)
            {
                PlayerFSM.MainInstance.SetState(StateType.Sprint);
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
