using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BaseTrigger : BaseState
{
    [Header("Trigger")]

    public List<BaseAction> actions;
    ExecuteState executeState;
    private Vector2 _move;

    private void SwitchStateName()
    {
        if (_move.y > 0)
        {
            executeState = ExecuteState.Move;
        }
        else
        {
            executeState = ExecuteState.Idle;
        }
    }


    public void OnMove(InputValue value)
    {
        _move = value.Get<Vector2>();
        Debug.Log(_move);


    }


    public bool DeleteEventOnDisEnable = false;

    public void GetActions()
    {
        actions = new List<BaseAction>(GetComponentsInChildren<BaseAction>());
    }
    protected override void Awake()
    {
        GetActions();
    }

    public override void Execute()
    {
        if (executeType == ExecuteType.Running)
            return;

        base.Execute();

        Running();
    }

    public override void Running()
    {
        base.Running();
        //执行第一个命令
        if (actions != null && actions.Count > 0)
        {
            actions[0].Execute();
        }
        else
            RunOver();

    }

    public override void RunOver()
    {
        base.RunOver();

        Exit();
    }

    public override void Exit()
    {
        base.Exit();
    }

    protected void OnDisable()
    {
        

        if (DeleteEventOnDisEnable)
            DeleteSaveTypeEvent();
    }


    public virtual void RegisterSaveTypeEvent()
    {
        //EventManager.StartListening("");
    }


    public virtual void DeleteSaveTypeEvent()
    {
        //EventManager.StopListening("");
    }
}