using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RGP.Game
{
    public abstract class GamebaseEvent
    {
        private string _name;
        public string name
        {
            get { return _name; }
            set { _name = value; }//value是类似item这种，自定义的string类型
        }

        private bool _enable;
        public bool enable
        {
            get { return _enable; }
            set { _enable = value; }//value是类似item这种，自定义的string类型
        }

        // Update is called once per frame
        public virtual void Update()
        {

        }
    }
}

