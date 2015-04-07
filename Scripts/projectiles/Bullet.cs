using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Core;

namespace Projectiles
{
    [RequireComponent(typeof(Rigidbody))]
    public class Bullet : MonoBehaviour
    {
        public float damageRate = 0.5f;
        public float checkDist = 20f;
        public GameObject decalHitWall;
        public GameObject explosion;
        public float floatInFrontOfWall = 0.01f;
        public float speedBullet = 70f;

        public float destroyAfterTime = 50f;
        public float destroyAfterTimeRandomization = 0f;
        public List<string> canHitObjectWithTag = new List<string>() { "can_hit", "Untagged" };
        public List<string> ignoreObjectWithTag = new List<string>() { "bullet" };
        
        private float countToTime = 0f;
        [System.NonSerialized]
        public GameObject sender;
        [System.NonSerialized]
        public bool fire = false;
        [System.NonSerialized]
        public Vector3 target = Vector3.zero;
        [System.NonSerialized]
        public bool inRoute = false;
        [System.NonSerialized]
        public float? explosionForce;
        [System.NonSerialized]
        public float? explosionSize;

        void Start()
        {
            GetComponent<Rigidbody>().mass = 1f;
            GetComponent<Rigidbody>().angularDrag = 0.01f;
            GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
            GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        void Update()
        {
            if (fire)
            {
                if (transform.tag != "bullet")
                {
                    transform.tag = "bullet";
                }
                destroyAfterTime += Random.value * destroyAfterTimeRandomization;
                GetComponent<Rigidbody>().AddForce(transform.forward * speedBullet, ForceMode.Impulse);

                fire = false;
                inRoute = true;
            }

            if (inRoute)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.forward, out hit, checkDist))
                {
                    if (hit.transform != null && !hit.transform.gameObject.Equals(sender) && canHitObjectWithTag.Contains(hit.transform.tag) && !ignoreObjectWithTag.Contains(hit.transform.tag))
                    {
                        Health targetHealth = hit.transform.GetComponent<Health>();
                        
                        Entity entity = hit.transform.gameObject.GetComponent<Entity>();
                        if (entity != null) entity.OnImpactReceive(hit.ToImpactHit(gameObject));

                        if (targetHealth != null)
                        {
                            targetHealth.hitPoint = hit.point;
                            targetHealth.hitNormal = hit.normal;
                            targetHealth.hitRotation = Quaternion.LookRotation(Vector3.forward); //Quaternion.FromToRotation(Vector3.up, hit.normal);
                            targetHealth.SendMessage("SetHealth", damageRate);
                        }

                        if (decalHitWall != null)
                        {
                            GameObject decalObj = Instantiate(decalHitWall, hit.point + (hit.normal * floatInFrontOfWall), Quaternion.FromToRotation(Vector3.up, hit.normal)) as GameObject;
                            decalObj.transform.parent = hit.transform;
                        }
                        if (explosion != null)
                        {
                            GameObject explObj = Instantiate(explosion, hit.point + (hit.normal * floatInFrontOfWall), Quaternion.FromToRotation(Vector3.up, hit.normal)) as GameObject;
                            explObj.transform.parent = hit.transform;
                            if (explosionForce.HasValue && explosionSize.HasValue)
                            {
                                Explosion expl = explObj.GetComponent<Explosion>();
                                expl.forcePower = explosionForce.Value;
                                expl.size = explosionSize.Value;
                            }
                        }
                        Destroy(gameObject);
                    }
                }

                countToTime += Time.deltaTime;
                if (countToTime >= destroyAfterTime)
                    Destroy(gameObject);
            }
            else
            {
                if (sender == null)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
