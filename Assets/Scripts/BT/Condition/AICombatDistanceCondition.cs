using BehaviorDesigner.Runtime.Tasks;
using GGG.Tool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICombatDistanceCondition : Conditional
{
    private EnemyCombatController enemyCombatControl;
    [SerializeField] private float attackDistance;
    public override void OnAwake()
    {
        enemyCombatControl = GetComponent<EnemyCombatController>();
    }
    public override TaskStatus OnUpdate()
    {
        return DevelopmentToos.DistanceForTarget(EnemyManager.MainInstance.getPlayer(), enemyCombatControl.transform) < attackDistance ? TaskStatus.Success : TaskStatus.Failure;
    }


}
