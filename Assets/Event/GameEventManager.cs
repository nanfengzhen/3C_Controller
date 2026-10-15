using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RGP.Game
{
    public static class GameEventManager
    {
        private static GameEventGroup m_Root;
        public static GameEventGroup RootGroup
        {
            get { return m_Root; }
            set { m_Root = value; }

        }
         static GameEventManager()
        {
            Init();
            RootGroup.enable = true;
            EnableAllEvent("Group1", true);
        }

        private static void Init()
        {
            RootGroup = new GameEventGroup("Root");
            GameEventGroup group1 = new GameEventGroup("Group1");
            GameEvent event1 = new GameEvent("Event1");
            GameEvent event2 = new GameEvent("Event2");
            group1.AddEvent(event1);
            group1.AddEvent(event2);

            RootGroup.AddEvent(group1);
        }

        public static void RegisterEvent(string eventName, GameEvent.CheckHandle check)
        {
            var target = RootGroup?.GetEvent(eventName);
            if (target != null && target is GameEvent temp)
            {
                temp.AddCheckHandle(check);
                
            }
        }

        public static void SubscribeEvent(string eventName, GameEvent.ResponseHandle response)
        {
            var target = RootGroup?.GetEvent(eventName);
            if (target != null && target is GameEvent temp)
            {
                temp.AddResponse(response);
            }
        }


        public static void EnableEvent(string eventName, bool _enable)
        {
            var target = RootGroup?.GetEvent(eventName);
            if (target != null)
            {
                target.enable = _enable;
            }
        }

        public static void EnableAllEvent(string eventName, bool _enable)
        {
            var target = RootGroup?.GetEvent(eventName) as GameEventGroup;
            if (target != null)
            {
                target.EnableAllEvent(_enable);
                //Debug.Log("BUWEIKONG");
            }

        }

        public static void Update()
        {
            if (RootGroup == null)
            {
                return;
            }
            else
            {
                RootGroup.Update();
            }
        }
    }
}


