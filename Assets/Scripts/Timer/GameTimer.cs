using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public class GameTimer
{
    public enum TimeState
    {
        NotWorking,
        Working,
        Done
    }

    private float workTime; //工作时间
    private Action action;  //工作内容 
    public TimeState timeState; //工作状态
    private bool isStopTimer;   //暂停计时



    //每获取一个计时器，都要重置
    public GameTimer()
    {
        ResetGameTimer();
    }


    //开始计时
    public void StartGameTimer(float time, Action task)
    {
        workTime = time;
        action = task;
        timeState = TimeState.Working;
        isStopTimer = false;
    }


    //计时
    public void UpdateGameTimer()
    {
        if (isStopTimer) return;
        workTime -= Time.deltaTime;    //计时器等待的时间
        if (workTime <= 0)
        {
            action?.Invoke();
            timeState = TimeState.Done;
            isStopTimer = true;
        }
    }
    public TimeState GetTimerState() => timeState;


    public void ResetGameTimer()
    {
        workTime = 0;
        timeState = TimeState.NotWorking;
        isStopTimer = true;
    }
}
