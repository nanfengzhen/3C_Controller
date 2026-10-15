using GGG.Tool;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerDashState : PlayerStateBase
{
    public PlayerDashState(PlayerMovementController movementController, PlayerComboController comboController) : base(movementController, comboController)
    {
    }

    public override void OnEnter()
    {
        movementController.animator.Play("SprintBS",0);
        movementController.animator.Play("SprintBS",1);

    }

    public override void OnExit()
    {
    }


    public override void OnUpdate()
    {
       
        if (!movementController.isGround && movementController.characterHeight >1f)
        {
            PlayerFSM.MainInstance.SetState(StateType.Landing);
        }
        else if (movementController.isGround)
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
                }
                if (movementController.CanJump() && PlayerInputManager.MainInstance._jump)
                {
                    PlayerFSM.MainInstance.SetState(StateType.SprintJump);
                    movementController.ResetState();
                }
                if (movementController.isDash)
                {
                    PlayerFSM.MainInstance.SetState(StateType.Dash);
                }
                if (movementController.isSprint)
                {
                    PlayerFSM.MainInstance.SetState(StateType.Sprint);

                }

            }
            else
            {
                PlayerFSM.MainInstance.SetState(StateType.Idle);
            }
        }
        

       

    }

}
