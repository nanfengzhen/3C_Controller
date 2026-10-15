using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObserverExample : MonoBehaviour
{
    public Button button;
    
    // Start is called before the first frame update
    void Start()
    {
        Observer.AddListener("按下按钮", OnButtonClicked);
        //button.onClick.AddListener(OnButtonClicked);
    }
    private void Update()
    {
       
    }


    private void OnDestroy()
    {
        Observer.RemoveListener("按下按钮", OnButtonClicked);
    }

    public void OnButtonClicked()
    {
        Debug.Log("成功完成观察者模式");
    }


}
