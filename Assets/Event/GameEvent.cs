using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace RGP.Game
{
    public class GameEvent : GamebaseEvent
    {
        public delegate void ResponseHandle(params object[] args);
        protected ResponseHandle m_Respomse;
        public delegate bool CheckHandle(out object[] args);
        protected CheckHandle m_CheckHandle;
        object[] m_Args;
        public GameEvent(string eventName)
        {
            name = eventName;
        }

        public override void Update()
        {
            if (!enable)
                return;
            if(m_CheckHandle != null && m_CheckHandle(out m_Args))
            {
                if (m_Respomse != null)
                    m_Respomse(m_Args);
            }
        }

        public void AddCheckHandle(CheckHandle check)
        {
            m_CheckHandle += check; //+=专用于委托，多播
        }

        public void RemoveCheckHandle(CheckHandle check)
        {
            m_CheckHandle -= check;
        }

        public void AddResponse(ResponseHandle response)
        {
            m_Respomse += response;
        }

        public void RemoveResponse(ResponseHandle response)
        {
            m_Respomse -= response;
        }


    }


}

