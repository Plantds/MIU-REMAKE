using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectPool
{
    private GameObject _parent;
    private PoolAbleObject _prefab;
    private int _size;
    private List<PoolAbleObject> _availableObjectPool;
    private static Dictionary<PoolAbleObject, ObjectPool> _objectPools = new Dictionary<PoolAbleObject, ObjectPool>();

    private ObjectPool(PoolAbleObject prefab, int size)
    {
        this._prefab = prefab;
        this._size = size;
        _availableObjectPool = new List<PoolAbleObject>(_size);
    }

    public static ObjectPool CreateInstance(PoolAbleObject prefab, int size)
    {
        ObjectPool pool = null;
        if (_objectPools.ContainsKey(prefab))
        {
            pool = _objectPools[prefab];
        }
        else
        {
            pool = new ObjectPool(prefab, size);

            pool._parent = new GameObject(prefab + " pool");
            pool.CreateObjects();

            _objectPools.Add(prefab, pool);
        }

        return pool;
    }

    private void CreateObjects()
    {
        for (int i = 0; i < _size; i++)
        {
            CreateObject();
        }
    }

    private void CreateObject()
    {
        PoolAbleObject poolableObject = GameObject.Instantiate(_prefab, Vector3.zero, Quaternion.identity, _parent.transform);
        poolableObject._parent = this;
        poolableObject.gameObject.SetActive(false); // PoolableObject handles re-adding the object to the AvailableObjects
    }

    public PoolAbleObject GetObject(Vector3 pos, Quaternion rot)
    {
        if (_availableObjectPool.Count == 0)
        {
            CreateObject();
        }

        PoolAbleObject instance = _availableObjectPool[0];

        _availableObjectPool.RemoveAt(0);

        instance.transform.position = pos;
        instance.transform.rotation = rot;

        instance.gameObject.SetActive(true);

        return instance;
    }

    public PoolAbleObject GetObject()
    {
        return GetObject(Vector3.zero, Quaternion.identity);
    }

    public void ReturnObjectToPool(PoolAbleObject Object)
    {
        _availableObjectPool.Add(Object);
    }
}
