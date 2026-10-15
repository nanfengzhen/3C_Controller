using BehaviorDesigner.Runtime.Tasks;
using GGG.Tool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIComboAction : Action
{
    EnemyCombatController enemyCombatControl;


    public override void OnAwake()
    {
        enemyCombatControl = GetComponent<EnemyCombatController>();


        
    }

    public override TaskStatus OnUpdate()
    {
        if (enemyCombatControl.getAttackCommand())
        {
            enemyCombatControl.AIBaseAttackInput();
            return TaskStatus.Running;
        }
        return TaskStatus.Success;
    }
}
