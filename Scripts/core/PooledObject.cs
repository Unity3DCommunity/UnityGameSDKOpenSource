using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PooledObject : MonoBehaviour {

    public static PooledObject current;

    public GameObject pooledObject;
    public bool willGrow = true;
    public int pooledAmount = 20;

    public List<GameObject> pooledObjects;

    void Awake()
    {
        current = this;
    }

	void Start()
    {
        pooledObjects = new List<GameObject>();
        for (int i = 0; i < pooledAmount; i++)
        {
            GameObject obj = (GameObject)Instantiate(pooledObject);
            obj.transform.parent = transform.parent;
            obj.SetActive(false);
            pooledObjects.Add(obj);
        }
    }

    public GameObject GetPooledObject(Vector3? position = null, Quaternion? rotation = null)
    {
        Vector3 _pos = Vector3.zero;
        Quaternion _rot = Quaternion.identity;

        if (position.HasValue) _pos = position.Value;
        if (rotation.HasValue) _rot = rotation.Value;

        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (pooledObjects[i] == null)
            {
                GameObject obj = (GameObject)Instantiate(pooledObject, _pos, _rot);
                obj.transform.parent = transform.parent;
                obj.SetActive(false);
                pooledObjects[i] = obj;
                return pooledObjects[i];
            }
            if (!pooledObjects[i].activeInHierarchy)
            {
                pooledObjects[i].transform.position = _pos;
                pooledObjects[i].transform.rotation = _rot;
                pooledObjects[i].transform.parent = transform.parent;
                return pooledObjects[i];
            }
        }
        if (willGrow)
        {
            GameObject obj = (GameObject)Instantiate(pooledObject, _pos, _rot);
            obj.transform.parent = transform.parent;
            pooledObjects.Add(obj);
            return obj;
        }
        return null;
    }
}
