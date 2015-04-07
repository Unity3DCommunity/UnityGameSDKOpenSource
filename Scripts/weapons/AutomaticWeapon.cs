using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Core;
using Projectiles;

namespace Weapons
{
    public class AutomaticWeapon : MonoBehaviour
    {
        public float fireTime = 0.05f;
        public Transform tagSpawn;

        [System.NonSerialized]
        public AudioManager fxAudio;
        [System.NonSerialized]
        public bool firing = false;

        Entity thisEntity;

        void Awake()
        {
            thisEntity = GetComponent<Entity>();
			fxAudio = GetComponent<AudioManager>();
        }

        void Start()
        {
            if (thisEntity.gameControl.gameMode == CoreEnums.GameModes.Missions || thisEntity.myNetworkView.isMine)
            {
                InvokeRepeating("Fire", fireTime, fireTime);
            }
        }

        void Fire()
        {
            if (thisEntity.gameControl.gameMode == CoreEnums.GameModes.Missions || thisEntity.myNetworkView.isMine)
            {
                if (firing)
                {
                    if (fxAudio != null)
                        fxAudio.PlayStartWith("Shoot Single");

                    GameObject obj = GameObject.Find("bullet_pooledbase").GetComponent<PooledObject>().GetPooledObject(tagSpawn.position, tagSpawn.rotation);

                    if (obj == null) return;

                    obj.transform.rotation = tagSpawn.rotation;
                    obj.transform.position = tagSpawn.position;
                    obj.GetComponent<BulletPooled>().sender = tagSpawn.root.gameObject;
                    obj.SetActive(true);
                }
            }
        }
    }
}