using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Core
{
    [System.Serializable]
    public class Damage : System.Object
    {
        public GameObject explosionFinal;
        public GameObject damageView;
        public List<DamageLevel> damageLevels = new List<DamageLevel>();

        public DamageLevel ApplyDamage(string name)
        {
            DamageLevel damageLevel = damageLevels.Find(l => l.name == name);
            if (damageLevel != null)
            {
                if (damageLevel.toActivate)
                {
                    damageLevel.ToggleVisibility();
                }
                else
                {
                    foreach (DamageLevel damage in damageLevels)
                    {
                        if (damage.name != name)
                        {
                            damage.Quit();
                        }
                        else
                        {
                            if (!damage.IsSpawned()) damage.Spawn();
                        }
                    }
                }
                return damageLevel;
            }
            return null;
        }

        public void FinishHim(Transform target, Vector3 pointSpawn, Vector3 normal)
        {
            if (explosionFinal != null)
            {
                GameObject explosion = MonoBehaviour.Instantiate(explosionFinal, pointSpawn + normal, Quaternion.FromToRotation(Vector3.up, normal)) as GameObject;
            }
            if (damageView != null)
            {
                GameObject view = MonoBehaviour.Instantiate(damageView, target.position, target.rotation) as GameObject;
            }
            AudioListener al;
            al = target.GetComponentInChildren<AudioListener>();
            
            if (al)
            {
                if (al.enabled)
                {
                    GameObject alt = new GameObject("audio_listener_" + target.name);
                    alt.AddComponent<AudioListener>();
                    alt.transform.position = target.position;
                }
            }
            MonoBehaviour.Destroy(target.gameObject);
        }
    }
}
