using GGG.Tool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ZZZ.HealthData
{
    [CreateAssetMenu(fileName = "CharacterHealthInfo", menuName = "Creat/Character/HealthData/CharacterHealthInfo", order = 0)]
    public class CharacterHealthInfo : ScriptableObject
    {
        //1.最大生命值
        //2.最大体力值
        //3.当前生命值
        //4.当前体力值
        //5.是否死亡
        //6.体力是否充沛

        private float maxHP;
        private float maxStrenth;
        private float currentHP;
        private float currentStrenth;
        private bool isDie => (currentHP <= 0);
        private bool strenthFull;   //体力值是否充足
        [SerializeField]private CharacterHealthBaseData healthData;

        public float MaxHP => maxHP;
        public float MaxStrenth => maxStrenth;
        public float CurrentHP => currentHP;
        public float CurrentStrenth => currentStrenth;
        public bool IsDie => isDie;
        public bool StrenthFull => strenthFull;


        public void InitCharacterHealthInfo()
        {
            maxHP = healthData.MaxHP;
            maxStrenth = healthData.MaxStrenth;
            currentHP = maxHP;
            currentStrenth = maxStrenth;
            strenthFull = true;

        }

        public void DamageHP(float damage)
        {
            //1.如果体力值不满了，那么就扣除体力值
            if(strenthFull)
            {
                currentStrenth = Clamp(currentStrenth, damage, 0, maxStrenth);
                DevelopmentToos.WTF("当前敌人的strenth值"+currentStrenth);
                if (currentStrenth <= 0)
                {
                    strenthFull = false;
                }
                return;
            }
            currentHP = Clamp(currentHP, damage, 0, maxHP);
        }

        //体力值扣除
        public void DamageStrenth(float damage)
        {
            if (currentHP <= 0) return; //生命值为负
            if (currentStrenth <= 0)    
            {
                strenthFull = false;
                return;
            }
            currentStrenth = Clamp(currentStrenth, damage, 0, maxStrenth);
        }

        //恢复生命值
        public void RecoverHP(float hp)
        {
            currentHP = Clamp(currentHP,hp, 0, maxHP,true);
        }


        //恢复体力值
        public void RecoverStrenth(float strenth)
        {

            currentStrenth = Clamp(currentStrenth,strenth, 0, maxStrenth,true);
            if(currentStrenth >=maxStrenth)
            {
                strenthFull=true;
            }
        }


        private float Clamp(float currentValue,float offssetValue,float min,float max,bool add =false)
        {
            return Mathf.Clamp(add?currentValue+offssetValue:currentValue-offssetValue, min, max);
        }
    }
}
