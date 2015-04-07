using UnityEngine;
using System.Collections;
using Core;

namespace Projectiles
{
    [System.Serializable]
    public class Missile : System.Object
    {
        public string alias;
        public GameObject IconUI;
        public GameObject prefab;
        public AudioManager fxAudio;
        //public KeyCode keyToActivate = KeyCode.Alpha2;
        public Transform[] tagOrigins;
        //public float damageRate;
        [System.NonSerialized]
        public bool isActive = false;


        public Transform randomizeTagOrigin()
        {
            if (tagOrigins.Length > 0)
            {

                return tagOrigins[Random.Range(0, tagOrigins.Length)];
            }
            return null;
        }
    }
}
