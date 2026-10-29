using GGG.Tool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class PlayerAttackState : PlayerStateBase
{
    // 防止攻击进入时仍按住的方向输入立即打断攻击。
    private bool moveInputReleasedSinceAttack;
    public PlayerAttackState(PlayerMovementController movementController,PlayerComboController comboController) : base(movementController,comboController)
    {

    }
    public PlayerAttackState(PlayerComboController comboController) : base(comboController)
    {
    }

    public override void OnEnter()
    {
        movementController.EnterState(StateType.Attack);
        moveInputReleasedSinceAttack = !movementController.HasMoveInput;
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

        // 进入攻击时按住的方向键不算新的移动输入，必须先松开再重新按下。
        if (!movementController.HasMoveInput)
        {
            moveInputReleasedSinceAttack = true;
        }

        if (!movementController.isGround && movementController.characterHeight > 1f)
        {
            PlayerFSM.MainInstance.SetState(StateType.Landing);
            return;
        }

        bool walkPressed = PlayerInputManager.MainInstance._walk;
        bool sprintPressed = PlayerInputManager.MainInstance._sprint;
        bool newRunInput = movementController.HasMoveInput && moveInputReleasedSinceAttack;
        bool newMovementInput = walkPressed || sprintPressed || newRunInput;

        if (newMovementInput && movementController.HasMoveInput)
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
                if (movementController.CanJump() && PlayerInputManager.MainInstance._jump)
                {
                    PlayerFSM.MainInstance.SetState(StateType.SprintJump);
                }
                else
                {
                    PlayerFSM.MainInstance.SetState(StateType.Sprint);
                }
                return;
            }
            if (movementController.CanJump() && PlayerInputManager.MainInstance._jump)
            {
                PlayerFSM.MainInstance.SetState(StateType.RunJump);
                return;
            }
            PlayerFSM.MainInstance.SetState(StateType.Run);
            return;
        }

        if (movementController.CanJump() && PlayerInputManager.MainInstance._jump)
        {
            PlayerFSM.MainInstance.SetState(StateType.Jump);
            return;
        }

        if (movementController.animator.AnimationAtTag("Attack") || movementController.animator.IsInTransition(0))
        {
            return;
        }

        // 攻击动画结束但方向仍被按住时，先释放攻击动画参数，避免 IdleState 下一帧自动切回跑步。
        if (movementController.HasMoveInput)
        {
            movementController.ExitState(StateType.Attack);
            return;
        }

        PlayerFSM.MainInstance.SetState(StateType.Idle);
    }





}

   





