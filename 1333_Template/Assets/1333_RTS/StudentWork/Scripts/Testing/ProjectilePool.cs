using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(ArcingProjectile))]
public class ProjectilePool : MonoBehaviour
{
    [SerializeField] public ArcingProjectile ProjectilePrefab;

    IObjectPool<ArcingProjectile> m_Pool;
    public PoolType poolType;

    public bool collectionChecks = true;
    public int maxPoolSize = 10;

    public enum PoolType
    {
        Stack,
    }

    public IObjectPool<ArcingProjectile> Pool
    {
        get
        {
            if (m_Pool == null)
            {
                if (poolType == PoolType.Stack)
                    m_Pool = new ObjectPool<ArcingProjectile>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, collectionChecks, 10, maxPoolSize);

            }
            return m_Pool;
        }
    }


    ArcingProjectile CreatePooledItem()
    {
        var proj =
             Instantiate(ProjectilePrefab, transform.position, Quaternion.identity);



        return proj;
    }

    // Called when an item is returned to the pool using Release
    void OnReturnedToPool(ArcingProjectile proj)
    {
        proj.gameObject.SetActive(false);
    }

    // Called when an item is taken from the pool using Get
    void OnTakeFromPool(ArcingProjectile proj)
    {
        proj.gameObject.SetActive(true);
    }

    // If the pool capacity is reached then any items returned will be destroyed.
    // We can control what the destroy behavior does, here we destroy the GameObject.
    void OnDestroyPoolObject(ArcingProjectile proj)
    {
        Destroy(proj.gameObject);
    }

}


