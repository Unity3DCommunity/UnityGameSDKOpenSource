using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Core;
using Sensors;

namespace Projectiles
{
    [RequireComponent(typeof(Rigidbody))]
    public class MissileControl : MonoBehaviour
    {
        public float damageRate = 15f;
        public float maxDist = 10000f;
        public GameObject decalHit;
        public GameObject hitExplosion;
        public float floatInFrontOfWall = 0.01f;
        public float maxAceleration = 20f;
        public float maxSpeed = 100f;
        public List<string> targetsToHit = new List<string>() { "can_hit", "Untagged" };
        public List<string> ignoreTargets = new List<string>() { "missile" };

        public float destroyAfterTime = 50f;
        public float destroyAfterTimeRandomization = 0f;

        private float time = 0f;
        //private Quaternion targetRotation = Quaternion.identity;
        private float currentSpeed = 0f;
        private float currentAceleration = 0f;
        private float waitForLaunch = 1f;
        private float waitForFollowTarget = 1.5f;
        private bool inRouted = false;

        [System.NonSerialized]
        public Transform sender;
        [System.NonSerialized]
        public Transform target;
        [System.NonSerialized]
        public Vector3 direction = Vector3.forward;
        [System.NonSerialized]
        public bool toTarget = false;

        private MissileDetection missileDetectionComponent;
        private TrailRenderer[] trailRenderers;

        void Start()
        {
            GetComponent<Rigidbody>().useGravity = false;
            GetComponent<Rigidbody>().mass = 10f;
            GetComponent<Rigidbody>().angularDrag = 0.01f;
            GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
            GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
            GetComponent<Rigidbody>().freezeRotation = true;
        }

        void Awake()
        {
            if (transform.tag != "missile")
            {
                transform.tag = "missile";
            }
            destroyAfterTime += Random.value * destroyAfterTimeRandomization;
            trailRenderers = transform.GetComponentsInChildren<TrailRenderer>();

            foreach (var item in trailRenderers)
            {
                item.enabled = false;
            }
        }

        void Update()
        {
            if (time < waitForLaunch)
            {
                transform.localPosition = Vector3.zero;
            }
        }

        void FixedUpdate()
        {
            if (toTarget && target)
            {
                Transform tt = target.FindChild("center_mass");
                if (tt != null)
                {
                    direction = tt.position;
                }
                else
                {
                    direction = target.position;
                }
            }
                

            if (currentSpeed <= maxSpeed)
            {
                if (currentAceleration <= maxAceleration)
                    currentAceleration += Time.deltaTime;

                currentSpeed = Mathf.Lerp(currentSpeed, maxSpeed, Time.deltaTime * currentAceleration);
            }

            if (time > waitForLaunch)
            {
                if (!inRouted)
                {
                    transform.parent = null;
                    foreach (var item in trailRenderers)
                    {
                        if (item != null) item.enabled = false;
                        
                    }
                }
                else
                {
                    foreach (var item in trailRenderers)
                    {
                        if (item != null) item.enabled = true;
                    }
                }
                GetComponent<Rigidbody>().velocity = transform.forward * currentSpeed;
            }

            if (time > waitForFollowTarget)
            {
                if (toTarget)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction - transform.position);
                    GetComponent<Rigidbody>().MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRotation, 20f));
                    if (target != null)
                    {
                        missileDetectionComponent = target.GetComponent<MissileDetection>();
                    }
                }
                else
                {
                    if (!inRouted)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(direction - transform.position);
                        GetComponent<Rigidbody>().MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRotation, 20f));
                        //transform.LookAt(direction);
                        if (Vector3.Angle(transform.forward, direction - transform.position) < 5f)
                        {
                            inRouted = true;
                        }
                    }
                }
            }

            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, 20f))
            {
                if (hit.transform != sender)
                {
                    Health targetHealth = hit.transform.GetComponent<Health>();
                    Entity entity = hit.transform.gameObject.GetComponent<Entity>();
                    
                    if (entity != null)
                    {
                        entity.SendMessage("OnImpactReceive", hit.ToImpactHit(gameObject), SendMessageOptions.RequireReceiver);
                    }

                    if (targetHealth != null)
                    {
                        targetHealth.hitPoint = hit.point;
                        targetHealth.hitNormal = hit.normal;
                        targetHealth.hitRotation = Quaternion.LookRotation(Vector3.forward); //Quaternion.FromToRotation(Vector3.up, hit.normal);
                        targetHealth.SendMessage("SetHealth", damageRate);
                    }
                    if (missileDetectionComponent != null)
                    {
                        missileDetectionComponent.isLocked = false;
                        missileDetectionComponent.incoming = false;
                    }
                    if (decalHit)
                    {
                        GameObject decal = Instantiate(decalHit, hit.point + (hit.normal * floatInFrontOfWall), Quaternion.FromToRotation(Vector3.up, hit.normal)) as GameObject;
                        decal.transform.parent = hit.transform;
                    }
                    if (hitExplosion)
                    {
                        GameObject explosion = Instantiate(hitExplosion, hit.point + (hit.normal * floatInFrontOfWall), Quaternion.LookRotation(Vector3.forward)) as GameObject;
                    }
                    Destroy(gameObject);
                }
            }

            time += Time.deltaTime;
            if (time >= destroyAfterTime)
                Destroy(gameObject);
        }

        public void SetToTarget(bool _value)
        {
            toTarget = _value;
        }
    }
}
