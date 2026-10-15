using BehaviorDesigner.Runtime.Tasks;
using GGG.Tool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICombatCondition : Conditional
{
    private EnemyCombatController enemyCombatControl;
    

    public override void OnAwake()
    {
        enemyCombatControl = GetComponent<EnemyCombatController>();
    }

    public override TaskStatus OnUpdate()
    {
        return enemyCombatControl.getAttackCommand() ? TaskStatus.Success : TaskStatus.Failure;
    }
}
