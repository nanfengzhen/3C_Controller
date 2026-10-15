using UnityEngine;
using UnityEngine.InputSystem;

public class DebugLogAction : BaseAction
{

    public string DebugContent = "DebugLogAction Execute";

    public bool Log = true;
    public bool LogWarning = false;
    public bool LogError = false;

    protected override void RunningLogic()
    {
        base.RunningLogic();
        if (Log)
            Debug.Log(DebugContent);
        if (LogWarning)
            Debug.LogWarning(DebugContent);
        if (LogError)
            Debug.LogError(DebugContent);

        RunOver();
    }

    private void Update()
    {
        //RunningLogic();
    }


}