using UnityEngine;
using System.Collections.Generic;
public class BodyPoolingSystem
{
    private Stack<GameObject> pool;
    public GameObject storageParent;

    public bool isEmpty { get { return pool.Count == 0; } }

    public BodyPoolingSystem()
    {
        pool = new Stack<GameObject>();
    }

    public void AddToPool(GameObject obj)
    {
        if (storageParent != null) obj.transform.parent = storageParent.transform;
        pool.Push(obj);
    }
    public GameObject GetFromPool()
    {
        if (pool.Count > 0)
        {
            GameObject gameObject = pool.Pop();
            gameObject.SetActive(true);
            return gameObject;
        }
        else
        {
            return null;
        }
    }
    public void ClearPool()
    {
        while (pool.Count > 0)
        {
            GameObject obj = pool.Pop();
            Object.Destroy(obj);
        }
        pool.Clear();
    }
}
