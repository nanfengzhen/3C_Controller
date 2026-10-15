using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAnimationEvent : MonoBehaviour
{
    public void PlaySound(string name)
    {
        GamePoolManager.MainInstance.TryGetPoolItem(name, transform.position, Quaternion.identity);  //传入位置和旋转的意义就是让声音播放就在角色自身
    }
}

