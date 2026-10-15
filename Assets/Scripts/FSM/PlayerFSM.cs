using GGG.Tool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerFSM:Singleton<PlayerFSM> 
{

    public PlayerStateBase playerCurState;
    public Dictionary<StateType, PlayerStateBase> states;

    public PlayerFSM()
    {
        states = new Dictionary<StateType, PlayerStateBase>();
    }

    public void AddState(StateType stateType, PlayerStateBase state)
    {
        if (states.ContainsKey(stateType))
        {
            //Debug.Log("已有该状态");

        }
        else
        {
            states.Add(stateType, state);
        }
    }

    public void SetState(StateType nextStateType)
    {
        DevelopmentToos.WTF(playerCurState + "->" + nextStateType);
        playerCurState?.OnExit();
        playerCurState = states[nextStateType];
        playerCurState.OnEnter();
    }

    /// <summary>
    /// 负责各个状态中要更新的东西
    /// </summary>
    public void OnTink()
    {
        playerCurState?.OnUpdate();
    }
}
