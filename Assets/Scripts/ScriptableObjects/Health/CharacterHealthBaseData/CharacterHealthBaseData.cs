using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZZZ.HealthData
{
    [CreateAssetMenu(fileName = "BaseHealthData", menuName = "Creat/Character/HealthData/BaseData", order = 0)]
    public class CharacterHealthBaseData : ScriptableObject
    {
        //1.记录每一种敌人的初始生命值信息，有强有弱的敌人，因此存为ScriptableObject

        [SerializeField] private float maxHP;
        [SerializeField] private float maxStrenth;

        /// <summary>
        /// 对外获取HP
        /// </summary>
        public float MaxHP =>maxHP;
        /// <summary>
        /// 对外获取体力值
        /// </summary>
        public float MaxStrenth =>maxStrenth;
    }
}
