using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Observer
{
    public static Dictionary<string, UnityEvent> eventManager = new Dictionary<string, UnityEvent>();

    private static Dictionary<string, UnityAction> callBacks = new Dictionary<string, UnityAction>();


    public static void AddListener(string eventName, UnityAction callBack)
    {
        //查看事件管理器中是否存在事件，如果存在那么就加上对应的action，else就创建事件
        if(eventManager.TryGetValue(eventName,out UnityEvent thisEvent)){
            thisEvent.AddListener(callBack);
        }
        else
        {
            thisEvent = new UnityEvent();
            thisEvent.AddListener(callBack);
            eventManager.Add(eventName, thisEvent);

        }
    }

    public static void RemoveListener(string eventName,UnityAction callback)
    {
        if(eventManager.TryGetValue(eventName,out UnityEvent thisEvent))
        {
            thisEvent.RemoveListener(callback);
        }
    }

    public static void EmitListener(string eventName)
    {
        if(eventManager.TryGetValue(eventName,out UnityEvent thisEvent))
        {
            thisEvent.Invoke();
        }
        else
        {
            return;
        }
    }
}
