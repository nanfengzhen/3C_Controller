using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameTimer;

/// <summary>
/// timer的管理类，实际用来进行timer调度的中心，对timer进行start和update
/// </summary>
public class GameTimerManager : Singleton<GameTimerManager>
{
    [SerializeField] public int maxTimerAmounts;

    private Queue<GameTimer> notWorkingTimer = new Queue<GameTimer>();
    private List<GameTimer> WorkingTimer = new List<GameTimer>();

    private void Start()
    {
        InitGameTimerManager();
    }

    private void Update()
    {
        UpdateTimer();
    }


    private void InitGameTimerManager()
    {
        for (int i = 0; i < maxTimerAmounts; i++)
        {
            CreateTimer();
        }
    }

    private void CreateTimer()
    {
        var gameTimer = new GameTimer();
        notWorkingTimer.Enqueue(gameTimer);
    }


    //让外部获得一个计时器（主要是用来计时解决cd问题）
    public void TryGetOneTimer(float startTimer, Action action)
    {
        if (notWorkingTimer.Count == 0)
        {
            CreateTimer();
            var timer = notWorkingTimer.Dequeue();
            timer.StartGameTimer(startTimer, action);
            WorkingTimer.Add(timer);

        }
        else
        {
            var timer = notWorkingTimer.Dequeue();
            timer.StartGameTimer(startTimer, action);
            WorkingTimer.Add(timer);

        }
    }



    //计时
    private void UpdateTimer()
    {
        if (WorkingTimer.Count == 0) return;
        for (int i = 0; i < WorkingTimer.Count; i++)
        {
            if (WorkingTimer[i].timeState == TimeState.Working)
            {
                WorkingTimer[i].UpdateGameTimer();
            }
            //工作完的计时器就重置并且进入notworking队列
            else
            {
                WorkingTimer[i].ResetGameTimer();
                notWorkingTimer.Enqueue(WorkingTimer[i]);
                WorkingTimer.Remove(WorkingTimer[i]);

            }
        }
    }


}
