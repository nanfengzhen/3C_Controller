using BehaviorDesigner.Runtime.Tasks;
using GGG.Tool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZZZ.Movement;

public class AIJustMoveToward : Action
{
    private EnemyMovementController enemyMovementControl;



    public override void OnAwake()
    {
        enemyMovementControl = GetComponent<EnemyMovementController>();

    }

    public override TaskStatus OnUpdate()
    {
        if (DevelopmentToos.DistanceForTarget(EnemyManager.MainInstance.getPlayer(), enemyMovementControl.transform) > 3f)
        {
            enemyMovementControl.SetApplyMovement(true);
            enemyMovementControl.SetAnimationMovementValue(1, 0);
            return TaskStatus.Running;
        }
        return TaskStatus.Success;
    }
}
