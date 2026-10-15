using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RGP.Game;
public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {

        RGP.Game.GameEventManager.RegisterEvent("Event1", Check1);
        RGP.Game.GameEventManager.RegisterEvent("Event2", Check2);
        RGP.Game.GameEventManager.SubscribeEvent("Event1", response1);
        RGP.Game.GameEventManager.SubscribeEvent("Event2", response2);



   }

    private void Update()
    {
        RGP.Game.GameEventManager.Update();
    }


    private bool Check1(out object[] args)
    {
        args = new object[] { "11", 12};
        //Debug.Log("shit");
        return true;
        
    }
    private void response1(object[] args)
    {
        if(args.Length == 2)
        {
            Debug.Log("response1==" + args[0]);
            Debug.Log("response1==" + args[1]);
        }
    }


    private bool Check2(out object[] args)
    {
        args = new object[] { "11", 12,"13" };
        return true;
    }
    private void response2(object[] args)
    {
        if (args.Length == 3)
        {
            Debug.Log("response1==" + args[0]);
            Debug.Log("response1==" + args[1]);
            Debug.Log("response1==" + args[2]);
        }
    }
}
