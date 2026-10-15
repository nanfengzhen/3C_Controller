using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RGP.Game
{
    public class GameEventGroup : GamebaseEvent
    {
        List<GamebaseEvent> m_Events;
        public GameEventGroup(string eventName)
        {
            name = eventName;
        }

        public void AddEvent(GamebaseEvent gameEvent)
        {
            if (m_Events == null)
            {
                m_Events = new List<GamebaseEvent>();
                
            }
            if(m_Events.Find(e=>e.name==gameEvent.name) != null)
            {
                return;
            }
            m_Events.Add(gameEvent);
        }

        public GamebaseEvent GetEvent(string eventName)
        {
            Queue<GamebaseEvent> q = new Queue<GamebaseEvent>();
            q.Enqueue(this);
            while (q.Count > 0)
            {
                GameEventGroup temp = q.Dequeue() as GameEventGroup;
                if (temp !=null &&temp.m_Events!=null && temp.m_Events.Count>0)
                {
                    var children = temp.m_Events;
                    foreach (GamebaseEvent eventItem in children)
                    {
                        if(eventItem.name == eventName)
                        {
                            return eventItem;
                        }
                        q.Enqueue(eventItem);
                    }
                }
            }
            return null;
            
        }

        public void EnableAllEvent(bool _enable)
        {
            enable = _enable;
            if (m_Events == null) return;
            foreach(GamebaseEvent eventItem in m_Events)
            {
                if(eventItem is GameEventGroup)
                {
                    (eventItem as GameEventGroup).EnableAllEvent(_enable);
                }
                else
                {
                    eventItem.enable = _enable;
                }
            }

        }

        public void RemoveEvent(string eventName)
        {
            if (m_Events == null) return;
            foreach(GamebaseEvent eventItem in m_Events)
            {
                if(eventItem.name == eventName)
                {
                    m_Events.Remove(eventItem);
                    return;
                }
                else
                {
                    if(eventItem is GameEventGroup)
                    {
                        (eventItem as GameEventGroup).RemoveEvent(eventName);
                    }
                }
            }
        }

        public override void Update()
        {
            if(!enable || m_Events == null)
            {
                return;
            }

            foreach(GamebaseEvent eventItem in m_Events)
            {
                if(eventItem != null)
                {
                    eventItem.Update();
                }
            }
        }
    }
}
