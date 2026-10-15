using UnityEngine;

public class SingletonExample : MonoBehaviour
{
    // 静态实例(private保证了这个实例只在这个静态类中进行instance的修改）
    private static SingletonExample instance;
    public static SingletonExample Instance
    {
        get
        {
            GameObject existingGameObject = GameObject.Find("GameObject");
            //懒汉式
            if (instance == null)
            {
                //instance = new SingletonExample();  //为什么不能这样，因为在继承mono下，每一个gameobj都是一个实例了，如果再new一个，就不再符合单例
                instance = existingGameObject.GetComponent<SingletonExample>();
            }
            return instance;
        }
    }




    public void test01()
    {
        Debug.Log("测试成功");
    }

   
    
}
