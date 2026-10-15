using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GGG.Tool;

public class GameEventManager : Singleton<GameEventManager>
{


    interface IEventHelp
    {

    }

    private class EventHelp : IEventHelp
    {
        private event Action _action;
        public EventHelp(Action action) //依赖注入
        {
            _action = action;
        }

        public void AddCall(Action action)
        {
            _action += action;
        }

        public void Call()
        {
            _action?.Invoke();
        }

        public void RemoveCall(Action action)
        {
            _action -= action;
        }

    }
    private class EventHelp<T> : IEventHelp
    {
        private event Action<T> _action;
        public EventHelp(Action<T> action) //依赖注入
        {
            _action = action;
        }

        public void AddCall(Action<T> action)
        {
            _action += action;
        }

        public void Call(T value)
        {
            _action?.Invoke(value); //将值通过invoke传入到函数中，回调函数
        }

        public void RemoveCall(Action<T> action)
        {
            _action -= action;
        }

    }       //可以传入一个参数,比如float来修改
    private class EventHelp<T1, T2> : IEventHelp
    {
        private event Action<T1, T2> _action;
        public EventHelp(Action<T1, T2> action) //依赖注入
        {
            _action = action;
        }

        public void AddCall(Action<T1, T2> action)
        {
            _action += action;
        }

        public void Call(T1 t1, T2 t2)
        {
            _action?.Invoke(t1, t2);
        }

        public void RemoveCall(Action<T1, T2> action)
        {
            _action -= action;
        }

    }
    private class EventHelp<T1, T2, T3> : IEventHelp
    {
        private event Action<T1, T2, T3> _action;
        public EventHelp(Action<T1, T2, T3> action)  //依赖注入
        {
            _action = action;
        }
        public void AddCall(Action<T1, T2, T3> action)
        {
            _action += action;
        }
        public void Call(T1 t1, T2 t2, T3 t3)
        {
            _action?.Invoke(t1, t2, t3);
        }

        public void RemoveCall(Action<T1, T2, T3> action)
        {
            _action -= action;
        }

    }

    private class EventHelp<T1, T2, T3, T4> : IEventHelp
    {
        private Action<T1, T2, T3, T4> _action;
        public EventHelp(Action<T1, T2, T3, T4> action)
        {
            _action = action;
        }

        public void AddCall(Action<T1, T2, T3, T4> action)
        {
            _action += action;
        }
        public void Call(T1 t1, T2 t2, T3 t3, T4 t4)
        {
            _action?.Invoke(t1, t2, t3, t4);
        }

        public void RemoveCall(Action<T1, T2, T3, T4> action)
        {
            _action -= action;
        }
    }

    private class EventHelp<T1, T2, T3, T4, T5> : IEventHelp
    {
        private event Action<T1, T2, T3, T4, T5> _action;
        public EventHelp(Action<T1, T2, T3, T4, T5> action) //依赖注入,创建一个事件以后，构造函数会自动注册该事件与函数的映射关系
        {
            _action = action;
        }

        public void AddCall(Action<T1, T2, T3, T4, T5> action)
        {
            _action += action;
        }

        public void Call(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
        {
            _action?.Invoke(t1, t2, t3, t4, t5);
        }

        public void RemoveCall(Action<T1, T2, T3, T4, T5> action)
        {
            _action -= action;
        }

    }


    private Dictionary<string, IEventHelp> eventCenter = new Dictionary<string, IEventHelp>();


    //先完成委托与字典映射，再完成委托与函数映射
    public void AddEventListener(string name, Action action)
    {
        if (eventCenter.TryGetValue(name, out var e))
        {
            (e as EventHelp).AddCall(action);   //若委托存在字典里，那么就直接取出开始注册函数
        }
        else
        {
            eventCenter.Add(name, new EventHelp(action)); //如不在，那么就把声明新委托先添加进字典
        }
    }
    public void AddEventListener<T>(string name, Action<T> action)
    {
        if (eventCenter.TryGetValue(name, out var e))
        {
            (e as EventHelp<T>).AddCall(action);
        }
        else
        {
            eventCenter.Add(name, new EventHelp<T>(action));
        }
    }
    public void AddEventListener<T1, T2>(string name, Action<T1, T2> action)
    {
        if (eventCenter.TryGetValue(name, out var e))   //out可以不初始化保留修改返回
        {
            (e as EventHelp<T1, T2>).AddCall(action);
        }
        else
        {
            eventCenter.Add(name, new EventHelp<T1, T2>(action));
        }
    }

    public void AddEventListener<T1, T2, T3>(string name, Action<T1, T2, T3> action)
    {
        if (eventCenter.TryGetValue(name, out var e))
        {
            (e as EventHelp<T1, T2, T3>).AddCall(action);
        }
        else
        {
            eventCenter.Add(name, new EventHelp<T1, T2, T3>(action));
        }

    }
    public void AddEventListener<T1, T2, T3, T4>(string name, Action<T1, T2, T3, T4> action)
    {
        if (eventCenter.TryGetValue(name, out var e))
        {
            (e as EventHelp<T1, T2, T3, T4>).AddCall(action);
        }
        else
        {
            eventCenter.Add(name, new EventHelp<T1, T2, T3, T4>(action));
        }



    }

    public void AddEventListener<T1, T2, T3, T4, T5>(string name, Action<T1, T2, T3, T4, T5> action)
    {
        if (eventCenter.TryGetValue(name, out var e))
        {
            (e as EventHelp<T1, T2, T3, T4, T5>).AddCall(action);
        }
        else
        {
            eventCenter.Add(name, new EventHelp<T1, T2, T3, T4, T5>(action));
        }
    }


    public void CallEvent(string name)
    {
        if (eventCenter.TryGetValue(name, out var e))
        {
            (e as EventHelp)?.Call();
        }
        else
        {
            DevelopmentToos.WTF($"当前未找到{name}的事件,无法回调");
        }
    }
    public void CallEvent<T>(string name, T value)
    {
        if (eventCenter.TryGetValue(name, out var e))
        {
            (e as EventHelp<T>)?.Call(value);
        }
        else
        {
            DevelopmentToos.WTF($"当前未找到{name}的事件,无法回调");
        }
    }
    public void CallEvent<T1, T2>(string name, T1 t1, T2 t2)
    {
        if (eventCenter.TryGetValue(name, out var e))
        {
            (e as EventHelp<T1, T2>)?.Call(t1, t2);
        }
        else
        {
            DevelopmentToos.WTF($"当前未找到{name}的事件,无法回调");
        }
    }

    public void CallEvent<T1, T2, T3>(string name, T1 t1, T2 t2, T3 t3)
    {
        if (eventCenter.TryGetValue(name, out var e))
        {
            (e as EventHelp<T1, T2, T3>).Call(t1, t2, t3);
        }
        else
        {
            DevelopmentToos.WTF($"当前未找到{name}的事件,无法回调");
        }
    }

    public void CallEvent<T1, T2, T3, T4>(string name, T1 t1, T2 t2, T3 t3, T4 t4)
    {
        if (eventCenter.TryGetValue(name, out var e))
        {
            (e as EventHelp<T1, T2, T3, T4>).Call(t1, t2, t3, t4);
        }
        else
        {
            DevelopmentToos.WTF($"当前未找到{name}的事件,无法回调");
        }
    }

    public void CallEvent<T1, T2, T3, T4, T5>(String name, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
    {
        if (eventCenter.TryGetValue(name, out var e))
        {
            (e as EventHelp<T1, T2, T3, T4, T5>)?.Call(t1, t2, t3, t4, t5);
        }
        else
        {
            DevelopmentToos.WTF($"当前未找到{name}的事件,无法回调");
        }

    }


    public void RemoveEvent(string name, Action action)
    {
        if (eventCenter.TryGetValue(name, out var e))
        {
            (e as EventHelp)?.RemoveCall(action);
        }
        else
        {
            DevelopmentToos.WTF($"当前未找到{name}的事件，无法移除");
        }
    }
    public void RemoveEvent<T>(string name, Action<T> action)
    {
        if (eventCenter.TryGetValue(name, out var e))
        {
            (e as EventHelp<T>)?.RemoveCall(action);
        }
        else
        {
            DevelopmentToos.WTF($"当前未找到{name}的事件，无法移除");
        }
    }
    public void RemoveEvent<T1, T2>(string name, Action<T1, T2> action)
    {
        if (eventCenter.TryGetValue(name, out var e))
        {
            (e as EventHelp<T1, T2>)?.RemoveCall(action);
        }
        else
        {
            DevelopmentToos.WTF($"当前未找到{name}的事件，无法移除");
        }
    }

    public void RemoveEvent<T1, T2, T3>(string name, Action<T1, T2, T3> action)
    {
        if (eventCenter.TryGetValue(name, out var e))
        {
            (e as EventHelp<T1, T2, T3>).RemoveCall(action);
        }
        else
        {
            DevelopmentToos.WTF($"当前未找到{name}的事件,无法移除");
        }
    }

    public void RemoveEvent<T1, T2, T3, T4>(string name, Action<T1, T2, T3, T4> action)
    {
        if (eventCenter.TryGetValue(name, out var e))
        {
            (e as EventHelp<T1, T2, T3, T4>).RemoveCall(action);
        }
        else
        {
            DevelopmentToos.WTF($"当前未找到{name}的事件,无法移除");
        }
    }

    public void RemoveEvent<T1, T2, T3, T4, T5>(string name, Action<T1, T2, T3, T4, T5> action)
    {
        if (eventCenter.TryGetValue(name, out var e))
        {
            (e as EventHelp<T1, T2, T3, T4, T5>)?.RemoveCall(action);
        }
        else
        {
            DevelopmentToos.WTF($"当前未找到{name}的事件，无法移除");
        }
    }
}
