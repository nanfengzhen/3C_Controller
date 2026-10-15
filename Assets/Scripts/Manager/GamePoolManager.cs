using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class GamePoolManager : Singleton<GamePoolManager>
{
    //1.需要先缓存我们要缓存的对象（我们外面先配置对象）
    //2.缓存
    //3.让外部可以获取对象
    //4.回收对象

    [System.Serializable]

    public class ItemPool   //不同的池子存的内容（比如子弹池子，金币池子等）
    {
        public string name;
        public GameObject item;
        public int initMaxCount;    //子弹数量，金币数量
    }

    [SerializeField] private List<ItemPool> itemPool = new List<ItemPool>();        //不同对象的对象池的总池。比如子弹，金币等等
    /*为不同对象对象池开辟存储gameobject的空间，比如：
     *金币池需要20个空间，子弹池需要100个空间
     *
     */
    private Dictionary<string, Queue<GameObject>> poolCenter = new Dictionary<string, Queue<GameObject>>();
    private GameObject itemPoolParent;



    private void Start()
    {
        itemPoolParent = new GameObject("itemPoolParent");
        itemPoolParent.transform.SetParent(this.transform);
        InitPool();

    }

    //初始化各个对象池
    private void InitPool()
    {
        if (itemPool.Count == 0) return;    //如果对象池里没有东西
        for (int i = 0; i < itemPool.Count; i++)
        {
            for (int j = 0; j < itemPool[i].initMaxCount; j++)
            {
                var item = Instantiate(itemPool[i].item);
                item.transform.SetParent(itemPoolParent.transform);
                item.SetActive(false);
                if (!poolCenter.ContainsKey(itemPool[i].name))
                {
                    //没有对象池，创建一个对象池
                    poolCenter.Add(itemPool[i].name, new Queue<GameObject>()); //<金币池，金币数量>,<子弹池，子弹数量>
                    poolCenter[itemPool[i].name].Enqueue(item);
                }
                else
                {
                    poolCenter[itemPool[i].name].Enqueue(item);
                }

            }
        }

        //DevelopmentToos.WTF("池子个数："+itemPool.Count);
        //DevelopmentToos.WTF("池子内数量：" + poolCenter["ATKSound"].Count);
    }


    public void TryGetPoolItem(string name, Vector3 position, Quaternion rotation)
    {
        if (poolCenter.ContainsKey(name))
        {
            var item = poolCenter[name].Dequeue();  //让ATKSound全部出队并且激活
            item.transform.position = position;
            item.transform.rotation = rotation;
            item.SetActive(true);
            poolCenter[name].Enqueue(item); //又回收
        }
        else
        {
            Debug.Log("当前的池子可能不存在");
        }
    }


    public GameObject TryGetPoolItem(string name)
    {
        if (poolCenter.ContainsKey(name))
        {
            var item = poolCenter[name].Dequeue();
            item.SetActive(true);
            poolCenter[name].Enqueue(item);
            return item;

        }
        Debug.Log("当前的池子可能不存在");
        return null;

    }

}
