using GGG.Tool;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ZZZ.Movement;

public class EnemyManager : Singleton<EnemyManager>
{
    Transform main_player;
    [SerializeField] private List<GameObject> allEnemy = new List<GameObject>();
    [SerializeField] private List<GameObject> activeEnemy = new List<GameObject>();
    private WaitForSeconds waitTime;

    private bool closeAttackCommandStartCoroutine;





    protected override void Awake()
    {
        base.Awake();
        main_player = GameObject.FindWithTag("Player").transform;

        waitTime = new WaitForSeconds(3);

    }


    private void Start()
    {
        if (activeEnemy.Count > 0)
        {
            closeAttackCommandStartCoroutine = false;
        }
        InitAllEnemy();

        StartCoroutine(EnableEnemyAttackPlayer());
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }


    public Transform getPlayer() => main_player;


    public void AddEnemy(GameObject gameObject)
    {
        if (!allEnemy.Contains(gameObject))
        {
            allEnemy.Add(gameObject);
        }
    }


    //如果敌人死了，就把敌人清单里的人物给移除
    public void RemoveEnemy(GameObject enemy)
    {
        if (activeEnemy.Contains(enemy))
        {
            EnemyMovementController enemyMovementControl;
            if (enemy.TryGetComponent(out enemyMovementControl))
            {
                enemyMovementControl.EnableChracterControl(false);
                DevelopmentToos.WTF("移除成功");
            }
            activeEnemy.Remove(enemy);
        }
    }


    //每隔6s指派1个npc来攻击角色
    IEnumerator EnableEnemyAttackPlayer()
    {
        if (activeEnemy == null) yield break;
        if (activeEnemy.Count == 0) yield break;
        while (activeEnemy.Count > 0)
        {
            if (closeAttackCommandStartCoroutine)
            {
                yield break;
            }
            var index = UnityEngine.Random.Range(0, activeEnemy.Count);
            if (index < activeEnemy.Count)//防止随机选取会超过范围
            {
                GameObject temp = activeEnemy[index];
                if (temp.TryGetComponent(out EnemyCombatController enemyCombatControl))
                {
                    enemyCombatControl.SetAttackCommand();

                }
            }

            yield return waitTime;
        }
        yield break;
    }


    public void StopAllEnemyAction()
    {
        foreach (var e in activeEnemy)
        {
            EnemyCombatController enemyCombatControl;
            if (e.TryGetComponent(out enemyCombatControl))
            {
                enemyCombatControl.StopEnemyCombat();
            }
        }
    }


    public void InitAllEnemy()
    {
        foreach (var e in allEnemy)
        {
            EnemyMovementController enemyMovementControl;
            if (e.TryGetComponent(out enemyMovementControl))
            {
                enemyMovementControl.EnableChracterControl(true);
                activeEnemy.Add(e);
            }
        }

    }

    public void CloseAttackCommandStartCoroutine()      //对外修改攻击指派协程
    {
        closeAttackCommandStartCoroutine = true;
        StopCoroutine(EnableEnemyAttackPlayer());
    }

}
