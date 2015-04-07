using UnityEngine;
using System.Collections;
using Core;
using System.Collections.Generic;

namespace Projectiles
{
    public class BulletPooled : Projectile
    {
        public override void OnEnable()
        {
            base.OnEnable();
            if (gameObject.tag != "bullet")
            {
                gameObject.tag = "bullet";
            }
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().AddForce(transform.forward * speedBullet, ForceMode.Impulse);
            Invoke("Destroy", 2f);
        }

        public override void Update()
        {
            base.Update();
            RaycastHit hit;

            if (Physics.Raycast(transform.position, transform.forward, out hit, 10f))
            {

                if (!hit.transform.gameObject.Equals(sender.gameObject) &&
                    canHitObjectWithTag.Contains(hit.transform.tag) &&
                    !ignoreObjectWithTag.Contains(hit.transform.root.tag))
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

                    if (decalHitWall != null)
                    {
                        GameObject decalObj = Instantiate(decalHitWall, hit.point + (hit.normal * floatInFrontOfWall), Quaternion.FromToRotation(Vector3.up, hit.normal)) as GameObject;
                        decalObj.transform.parent = hit.transform;
                    }
                    if (explosion != null)
                    {
                        GameObject explObj = Instantiate(explosion, hit.point + (hit.normal * floatInFrontOfWall), Quaternion.FromToRotation(Vector3.up, hit.normal)) as GameObject;
                        explObj.transform.parent = hit.transform;
                    }
                    gameObject.SetActive(false);
                }
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            gameObject.SetActive(false);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            CancelInvoke();
        }
    }
}