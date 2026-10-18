using GGG.Tool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class PlayerAttackState : PlayerStateBase
{
    public PlayerAttackState(PlayerMovementController movementController,PlayerComboController comboController) : base(movementController,comboController)
    {

    }
    public PlayerAttackState(PlayerComboController comboController) : base(comboController)
    {
    }

    public override void OnEnter()
    {
        movementController.EnterState(StateType.Attack);
        comboController.CharacterBaseAttackInput();

    }

    /// <summary>
    /// 需要清空在Attack的一些状态
    /// </summary>
    public override void OnExit()
    {
        movementController.ExitState(StateType.Attack);
    }



    public override void OnUpdate()
    {
        
        if (PlayerInputManager.MainInstance._attack || PlayerInputManager.MainInstance.rightAttack)
        {
            Debug.Log("进入攻击状态");

            comboController.CharacterBaseAttackInput();
            return;
            
        }

        if (movementController.animator.AnimationAtTag("Attack") || movementController.animator.IsInTransition(0))
        {
            return;
        }

        if (movementController.HasMoveInput)
        {
            if (movementController.isWalk)
            {
                if (movementController.CanJump() && PlayerInputManager.MainInstance._jump)
                {
                    PlayerFSM.MainInstance.SetState(StateType.WalkJump);
                }
                else
                {
                    PlayerFSM.MainInstance.SetState(StateType.Walk);
                }
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
        else if (!movementController.animator.AnimationAtTag("Attack"))
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

   





