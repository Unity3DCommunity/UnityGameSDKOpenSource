using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Core
{
    [System.Serializable]
    public class DamageLevel : System.Object
    {
        public string name = "";
        public GameObject fxPrefab;
        public Transform tagSpawn;
        public bool toActivate = false;
        public bool attach = true;
        public float damageRate = 0f;

        public bool ToggleVisibility()
        {
            tagSpawn.gameObject.SetActive(!tagSpawn.gameObject.activeSelf);

            return tagSpawn.gameObject.activeSelf;
        }

        public GameObject Spawn()
        {
            if (fxPrefab != null)
            {
                GameObject fx = MonoBehaviour.Instantiate(fxPrefab, tagSpawn.position, tagSpawn.rotation) as GameObject;
                fx.name = name;
                if (attach)
                {
                    fx.transform.parent = tagSpawn;
                }
                return fx;
            }
            return null;
        }

        public bool Quit()
        {
            if (fxPrefab != null && tagSpawn != null)
            {
                Transform fx = tagSpawn.FindChild(name);
                if (fx != null)
                {
                    MonoBehaviour.Destroy(fx.gameObject);
                    return true;
                }
            }
            return false;
        }

        public bool IsSpawned()
        {
            Transform fx = tagSpawn.FindChild(name);
            if (fx != null)
            {
                MonoBehaviour.Destroy(fx.gameObject);
                return true;
            }
            return false;
        }
    }
}
