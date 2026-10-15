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
        comboController.CharacterBaseAttackInput();

    }

    /// <summary>
    /// 需要清空在Attack的一些状态
    /// </summary>
    public override void OnExit()
    {

    }



    public override void OnUpdate()
    {
        
        if (PlayerInputManager.MainInstance._attack || PlayerInputManager.MainInstance.rightAttack)
        {
            Debug.Log("进入攻击状态");

            PlayerFSM.MainInstance.SetState(StateType.Attack);
            
        }

        if (movementController.animator.AnimationAtTag("Attack"))
        {
            movementController.PlayerState = StateType.Attack;
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
                PlayerFSM.MainInstance.SetState(StateType.Sprint);
            }
            PlayerFSM.MainInstance.SetState(StateType.Run);
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

   





