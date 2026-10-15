using GGG.Tool;
using UnityEngine;
using UnityEngine.Windows;

// 基类：处理所有状态都可能发生的通用转换
public abstract class PlayerStateBase
{
    protected PlayerMovementController movementController;
    protected PlayerComboController comboController;
    protected  PlayerStateBase(PlayerMovementController movementController,PlayerComboController comboController)
    {
        this.movementController = movementController;
        this.comboController = comboController;
    }

    protected PlayerStateBase(PlayerComboController comboController)
    {
        this.comboController = comboController;
    }

    public virtual void OnEnter()
    {
    }
    public virtual void OnExit()
    {

    }

    public virtual void OnUpdate()
    {
        //// 2. 攻击（几乎所有状态都能攻击）
        //if (PlayerInputManager.MainInstance._attack && Player.MainInstance.isGrounded)
        //{
        //    PlayerFSM.MainInstance.SetState(StateType.Attack);
        //}
        //CheckSpecialTransitions();
    }

    // 子类只需要实现自己特殊的逻辑
    public virtual void CheckSpecialTransitions()
    {

    }
}
