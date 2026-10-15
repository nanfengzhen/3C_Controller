using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IPoolItem
{
    void Spawn();
    void Recycle();

}
public abstract class PoolItembase : MonoBehaviour, IPoolItem
{
    public virtual void OnEnable()
    {
        Spawn();
        //DevelopmentToos.WTF("≥…π¶≤•∑≈“Ù∆µ");
    }
    public virtual void OnDestroy()
    {
        Recycle();
    }


    public virtual void Recycle()
    {

    }

    public virtual void Spawn()
    {

    }

}
