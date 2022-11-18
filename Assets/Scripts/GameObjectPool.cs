using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public abstract class GameObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject[] objects;
    [SerializeField] private bool collectionChecks = true;
    [SerializeField] private int initPoolSize = 10;
    [SerializeField] private int maxPoolSize = 500;
    
    ObjectPool<GameObject> m_Pool;

    public ObjectPool<GameObject> Pool
    {
        get
        {
            if (m_Pool == null)
            {
                m_Pool = new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, collectionChecks, initPoolSize, maxPoolSize);
            }

            return m_Pool;
        }
    }

    protected virtual GameObject CreatePooledItem()
    {
        if (objects.Length == 0) return null;
        return Instantiate(objects.Length > 1 ? objects[Random.Range(0, objects.Length)] : objects[0]);
    }

    protected virtual void OnTakeFromPool(GameObject obj)
    {
        obj.SetActive(true);
    }

    protected virtual void OnReturnedToPool(GameObject obj)
    {
        obj.SetActive(false);
    }

    protected void OnDestroyPoolObject(GameObject obj)
    {
        Debug.LogWarning($"Object pool {name} deleted an instance of object {obj.name}. Increase size of pool.");
        Destroy(obj);
    }
    
    

}
