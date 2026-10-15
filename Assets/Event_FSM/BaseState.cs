using UnityEngine;

public abstract class BaseState : MonoBehaviour
{

    public string content;

    public ExecuteType executeType;

    public BaseState parentState;

    public enum ExecuteType
    {
         None,
         Enter,
         Running,
         RunOver,
         Exit,
    }

    protected virtual void Awake()
    {
        executeType = ExecuteType.None;
    }

    protected virtual void OnEnter()
    {
        executeType = ExecuteType.Enter;
    }

    protected virtual void OnRunning()
    {
        executeType = ExecuteType.Running;
    }

    protected virtual void OnRunOver()
    {
        executeType = ExecuteType.RunOver;
    }

    protected virtual void OnExit()
    {
        executeType = ExecuteType.Exit;
    }


    
    public virtual void Execute()
    {
        OnEnter();
    }

    public virtual void Running()
    {
        OnRunning();
    }

    public virtual void RunOver()
    {
        OnRunOver();
    }
    public virtual void Exit()
    {
        OnExit();
    }

    protected virtual void OnAddState()
    {

    }
}