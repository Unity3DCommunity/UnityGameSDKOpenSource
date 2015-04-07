using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Projectiles
{
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        public float damageRate = 0.5f;
        public float speedBullet = 50f;
        public List<string> canHitObjectWithTag = new List<string>() { "can_hit", "Untagged" };
        public List<string> ignoreObjectWithTag = new List<string>() { "bullet" };
        public GameObject decalHitWall;
        public GameObject explosion;
        public float floatInFrontOfWall = 0.01f;

        [System.NonSerialized]
        public GameObject sender;

        public virtual void Start()
        { }
        public virtual void Update()
        { }
        public virtual void FixedUpdate()
        { }
        public virtual void OnEnable()
        { }
        public virtual void OnDisable()
        { }
        public virtual void Destroy()
        { }
    }
}
