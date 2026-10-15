using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "ComboData", menuName = "Create/Character/ComboData", order = 0)] //使该脚本能够成为图形化元素
public class CharacterComboDataSO : ScriptableObject
{
    [SerializeField] private string comboName;     //招式的名称 
    [SerializeField] private string[] comboHitName;  //该招式对应受击动画。有些动画一个攻击左脸一个攻击右脸，就会有两个受击动画，因此要数组
    [SerializeField] private string[] comboParryName;    //该招式对应的格挡动画



    [SerializeField] private float damage;  //招式的伤害值
    [SerializeField] private float coldTime;    //招式的冷却时间
    [SerializeField] private float positionOffset;  //招式的偏移量



    public string _comboName => comboName;
    public string[] _comboHitName => comboHitName;
    public string[] _comboParryName => comboParryName;
    public float _damage => damage;
    public float _coldTime => coldTime;
    public float _positionOffset => positionOffset;

    public int count => comboHitName.Length;


    //攻击和受击肯定是对应的，所以是HitandParry的maxcount
    public int ComboHitAndParryMaxCount() => comboHitName.Length;
}