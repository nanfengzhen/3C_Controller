using BehaviorDesigner.Runtime.Tasks;
using GGG.Tool;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using ZZZ.Movement;

public class AIFreeMovementAction : Action
{
    //1.当ai不动的时候，播放idle
    //2.要么左右移动，要么前后移动
    //3.当玩家靠近的时候，ai要自动后退
    //4.当离玩家一定距离时，要让ai朝着玩家靠近

    private EnemyMovementController enemyMovementControl;
    private EnemyCombatController enemyCombatControl;
    private int currentIndex;
    private int lastIndex;
    private float actionTimer;


    public override void OnAwake()
    {
        base.OnAwake();
        enemyMovementControl = GetComponent<EnemyMovementController>();
        enemyCombatControl = GetComponent<EnemyCombatController>();
        lastIndex = currentIndex;
        actionTimer = 2f;
    }


    //返回成功
    public override TaskStatus OnUpdate()
    {
        //Debug.Log("进来了");
        if (!(enemyCombatControl.getAttackCommand()))   //没有攻击
        {
            if (distanceForTarget() > 8f)
            {
                enemyMovementControl.SetAnimationMovementValue(1, 0);
                //Debug.Log("距离8");
                
            }
            else if(distanceForTarget() >3f+0.1f && distanceForTarget() < 8f+0.1f)
            {
                
                UpdateFreeMoveTime();
                AIFreeAction();
                //Debug.Log("距离3.5");

            }
            else
            {
                enemyMovementControl.SetAnimationMovementValue(-1, -1);
                //Debug.Log("距离2");
            }
            return TaskStatus.Running;

        }
        else
        {
            //执行一些退出的逻辑
        }
        return TaskStatus.Success;
    }

    //1.当没有接收到攻击指令->判断距离->如果大于3->朝玩家行走->否则停下，一直在运行当中TaskStatus.Running
    //2.如果收到攻击指令->看看要不要执行一些退出的指令，然后退出TaskStatus.Success


    //切换ai动作
    private void AIFreeAction()
    {
        switch(currentIndex)
        {
            case 0:
                enemyMovementControl.SetAnimationMovementValue(0, 1);
                //Debug.Log("执行动作0");
                break;
            case 1:
                enemyMovementControl.SetAnimationMovementValue(0, -1);
                //Debug.Log("执行动作1");
                break;
        }
    }



    //更新计时器
    private void UpdateFreeMoveTime()
    {
        if (actionTimer>0f)
        {
            actionTimer -= Time.deltaTime;
        }
        else
        {
            //到点了，该改动作了
            ChangeAIAction();
        }

    }

    //更新动画索引值
    private void ChangeAIAction()
    {
        lastIndex = currentIndex;
        currentIndex = Random.Range(0, 2);
        actionTimer = 2f;
        if (currentIndex ==lastIndex)
        {
            currentIndex = Random.Range(0, 2);
        }

    }
    private float distanceForTarget() => DevelopmentToos.DistanceForTarget(EnemyManager.MainInstance.getPlayer(), transform);
}
