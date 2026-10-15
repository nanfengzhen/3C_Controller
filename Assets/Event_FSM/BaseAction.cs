using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BaseAction : BaseState
{
    [Header("Action")]

    public float delayTime;

    public float waitTime;

    public UnityEvent _unityEvent;

    protected override void Awake()
    {
        parentState = GetComponentInParent<BaseTrigger>();
    }

    public override void Execute()
    {
        base.Execute();

        if (delayTime > 0)
            Invoke(nameof(Running), delayTime);
        else
            Running();
    }

    public override void Running()
    {
        base.Running();
        RunningLogic();

    }

    //新Action只需重写RunningLogic()即可,并在结束时调用RunOver();
    protected virtual void RunningLogic()
    {
        _unityEvent?.Invoke();

        //RunOver();
    }

    public override void RunOver()
    {
        base.RunOver();

        if (waitTime > 0)
            Invoke(nameof(Exit), waitTime);
        else
            Exit();

    }

    public override void Exit()
    {
        //检查父节点Trigger，执行下一条命令或者结束Trigger
        if (parentState != null && parentState is BaseTrigger baseTrigger)
        {
            //顺序执行
            var index = baseTrigger.actions.IndexOf(this);
            index++;
            if (index >= baseTrigger.actions.Count)
            {
                baseTrigger.RunOver();
            }
            else
            {
                baseTrigger.actions[index].Execute();
            }
        }

        base.Exit();
    }

}