using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

namespace Core
{
    public class Health : MonoBehaviour
    {
        public float health = 100f;
        public float resistance = 10f;
        public Damage damage;
        public bool autoRegeneration = false;
        public float regenerationSpeed = 0.2f;

        public GameObject healtUI;
        public GameObject warningUI;

        [System.NonSerialized]
        public bool applHitPoint = false;
        [System.NonSerialized]
        public Vector3 hitPoint = Vector3.zero;
        [System.NonSerialized]
        public Vector3 hitNormal = Vector3.zero;
        [System.NonSerialized]
        public Quaternion hitRotation = Quaternion.identity;

        public int currentDamage = 0;
        private int lastDamage = 0;

        Entity thisEntity;

        void Awake()
        {
            thisEntity = GetComponent<Entity>();
        }

        void Update()
        {
            if (thisEntity.gameControl.gameMode == CoreEnums.GameModes.Missions || thisEntity.myNetworkView.isMine)
            {
                lastDamage = currentDamage;

                if (autoRegeneration)
                {
                    if (health > 0 && health < 100f)
                    {
                        health += Time.deltaTime * regenerationSpeed;
                    }
                }

                if (healtUI)
                {
                    RectTransform rt = healtUI.GetComponent<RectTransform>();
                    if (rt)
                    {
                        Vector3 ls = rt.localScale;
                        ls.x = (health / 100);
                        rt.localScale = ls;
                    }
                }

                if (damage.damageLevels.Count > 0)
                {
                    currentDamage = damage.damageLevels.FindLastIndex(l => l.damageRate > health);

                    if (warningUI)
                    {

                    }
                }

                if (health < 1f) damage.FinishHim(transform, hitPoint, hitNormal);

                ShiftDamage();

                applHitPoint = false;
            }
        }

        void ShiftDamage()
        {
            if (thisEntity.gameControl.gameMode == CoreEnums.GameModes.Missions || thisEntity.myNetworkView.isMine)
            {
                if (lastDamage != currentDamage && currentDamage > -1)
                {
                    if (damage.damageLevels[currentDamage].tagSpawn == null) applHitPoint = true;

                    if (applHitPoint)
                    {
                        GameObject objTag = new GameObject();
                        objTag.transform.position = hitPoint + hitNormal;
                        objTag.transform.rotation = hitRotation;
                        objTag.transform.parent = transform;
                        damage.damageLevels[currentDamage].tagSpawn = objTag.transform;
                    }
                    damage.ApplyDamage(damage.damageLevels[currentDamage].name);
                }
                else if (currentDamage < 0)
                {
                    damage.damageLevels[0].Quit();
                }
            }
        }

        public float SetHealth(float damageRate)
        {
            if (thisEntity.gameControl.gameMode == CoreEnums.GameModes.Missions || thisEntity.myNetworkView.isMine)
            {
                health -= (damageRate / resistance);
            }
            return health;
        }
    }
}