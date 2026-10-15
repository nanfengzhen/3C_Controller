using GGG.Tool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLandingState : PlayerStateBase
{
    public PlayerLandingState(PlayerMovementController movementController, PlayerComboController comboController) : base(movementController, comboController)
    {
    }

    public override void OnEnter()
    {
        movementController.animator.CrossFade("DropDown", 0.13f, 0, 0);
        movementController.animator.CrossFade("DropDown", 0.13f, 1, 0);
        if (movementController.characterHeight > 4f)
        {
            movementController.animator.SetInteger("Height", 8);
        }
        if (movementController.characterHeight < 4f)
        {
            movementController.animator.SetInteger("Height", 4);
        }
    }

    public override void OnExit()
    {

    }


    public override void OnUpdate()
    {
        if(movementController.isGround && !movementController.animator.GetBool("HasInput"))
        {
            Debug.Log("由Landing变成Idle");
            Debug.Log("当前玩家状态"+movementController.PlayerState);
            PlayerFSM.MainInstance.SetState(StateType.Idle);
        }
        //if (movementController.isGround && movementController.PlayerState == StateType.Sprint)
        //{
        //    Debug.Log("由Landing变成Run");
        //    PlayerFSM.MainInstance.SetState(StateType.Sprint);
        //}
        if (movementController.isGround && movementController.animator.AnimationAtTag("Move"))
        {
            Debug.Log("由Landing变成Run");
            PlayerFSM.MainInstance.SetState(StateType.Run);
        }


    }


}
