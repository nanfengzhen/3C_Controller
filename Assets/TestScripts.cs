using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public delegate void ScriptTest(string name);   //委托是托管函数的容器，要托管的函数的传入的参数要与委托传入的参数一致


public class TestScripts : MonoBehaviour
{
    public static ScriptTest scriptTest;
    public static void test(string name)
    {
        Debug.Log("shit");
    }
    public static void test1(string name)
    {
        Debug.Log("shit1");
    }
    public static void test2(string name)
    {
        Debug.Log("shit2");
    }
    public static void test3(string name)
    {
        Debug.Log("shit3");
    }
    public static void test4(string name)
    {
        Debug.Log("shit4");
    }
    public static void test5(string name)
    {
        Debug.Log("shit5");
    }


    void Start()
    {
        
        scriptTest += test;
        scriptTest += test1;
        scriptTest += test2;
        scriptTest += test3;
        scriptTest += test4;
        scriptTest += test5;
        //scriptTest("shit");
        scriptTest.Invoke("shit");
    }




    //// Start is called before the first frame update
    //void Start()
    //{

    //}

    //// Update is called once per frame
    //void Update()
    //{
    //}


    //public void OnMove(InputValue value)
    //{


    //    Debug.Log("Player层成功连接到Input系统");
    //}
}
