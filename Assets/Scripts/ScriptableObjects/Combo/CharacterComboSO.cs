using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Combo", menuName = "Create/Character/ComboSO", order = 0)]
public class CharacterComboSO : ScriptableObject
{
    [SerializeField] public List<CharacterComboDataSO> allComboData = new List<CharacterComboDataSO>();

    //咏春拳名字
    public string GetComboName(int index)
    {
        if (allComboData.Count == 0) return null;
        return allComboData[index]._comboName;

    }
    //咏春拳的受击动画
    public string GetComboHitName(int index, int hitindex)
    {
        if (allComboData.Count == 0) return null;
        if (allComboData[index].ComboHitAndParryMaxCount() == 0) return null;
        return allComboData[index]._comboHitName[hitindex];

    }
    //咏春拳的格挡动画
    public string GetComboParryName(int index, int parryindex)
    {
        if (allComboData.Count == 0) return null;
        if (allComboData[index].ComboHitAndParryMaxCount() == 0) return null;
        return allComboData[index]._comboParryName[parryindex];

    }

    //咏春拳造成的伤害
    public float GetDamage(int index)
    {
        if (allComboData.Count == 0) return 0;
        return allComboData[index]._damage;
    }

    //咏春拳需要的冷却时间
    public float GetColdTime(int index)
    {
        if (allComboData.Count == 0) return 0;
        return allComboData[index]._coldTime;
    }

    //咏春拳需要的偏移量
    public float GetPositionOffset(int index)
    {
        if (allComboData.Count == 0) return 0;
        return allComboData[index]._positionOffset;
    }

    //咏春拳的对应每一招的连招数
    public int GetComboHitAndParryMaxCount(int index)
    {
        if (allComboData.Count == 0) return 0;
        return allComboData[index].ComboHitAndParryMaxCount();
    }


    //咏春拳一共有几招
    public int GetAllComboCounts()
    {
        if (allComboData.Count == 0) return 0;
        return allComboData.Count;
    }
}
