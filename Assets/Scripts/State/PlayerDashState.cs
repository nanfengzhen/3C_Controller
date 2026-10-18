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
        movementController.EnterState(StateType.Dash);
        movementController.animator.Play("SprintBS",0);
        movementController.animator.Play("SprintBS",1);

    }

    public override void OnExit()
    {
        movementController.ExitState(StateType.Dash);
    }


    public override void OnUpdate()
    {
       
        if (!movementController.isGround && movementController.characterHeight >1f)
        {
            PlayerFSM.MainInstance.SetState(StateType.Landing);
            return;
        }
        else if (movementController.isGround)
        {
            if (PlayerInputManager.MainInstance._attack || PlayerInputManager.MainInstance.rightAttack)
            {
                PlayerFSM.MainInstance.SetState(StateType.Attack);
                return;
            }
            if (movementController.HasMoveInput)
            {

                if (movementController.isWalk)
                {
                    PlayerFSM.MainInstance.SetState(StateType.Walk);
                    return;
                }
                if (movementController.CanJump() && PlayerInputManager.MainInstance._jump)
                {
                    PlayerFSM.MainInstance.SetState(StateType.SprintJump);
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

                PlayerFSM.MainInstance.SetState(StateType.Run);
                return;

            }
            else
            {
                PlayerFSM.MainInstance.SetState(StateType.Idle);
            }
        }
        

       

    }

}
