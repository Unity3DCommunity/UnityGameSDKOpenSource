using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Vehicles;
using Weapons;
using Projectiles;

namespace Core.AI
{
    public class AI : MonoBehaviour
    {
        #region enums
        
        public enum Behavior
        { Defender, Explorer, Patrol }
        public enum States
        { Alert, Patrol, StandBy, Chase, Attack, UnderAttack, Critical }
        public enum Difficulty
        { Easy, Normal, Hard, Hardest }
        
        #endregion

        #region public vars

        public List<string> NameKeyWordsIsEnemy;
        public Behavior behavior;
        public float patrolSpeed = 10f;
        public float chaseSpeed = 15f;
        public float evadeSpeed = 15f;
        public float turnSpeedOnChase = 8f;
        [Range(10f, 120f)]
        public float FOV = 40f;
        [Range(0f, 30f)]
        public float FOVForFire = 10f;
        [Range(10f, 360f)]
        public float SearchGradesOnAlert = 30f;
        public float speedLookAround = 10f;
        public float perceptionRadius = 200f;
        public SphereCollider triggerPerception;
        public float maxTimeLost = 10f;
        public GameObject sector;
        public float patrolWaitTime = 1f;
        [Range(0f, 50f)]
        public float minHealthToRecharge = 15f;
        public bool ShowDebugMsgs = false;
        public bool ShowLinesPerceptions = false;
        public bool ShowNavPoints = false;
        public bool allowFire = true;
        public Color debugColor;

        // non serialized
        [System.NonSerialized]
        public Entity entity;
        [System.NonSerialized]
        public States state;
        [System.NonSerialized]
        public Difficulty difficulty;

        #endregion

        #region private vars

        private List<Enemies> enemysInRange = new List<Enemies>();
        private List<Enemies> enemiesInFOV  = new List<Enemies>();
        private float timeLost = 0f;
        private List<GameObject> patrolWayPoints;
        private float patrolTimer;
        private float chaseTimer;
        private float evadeTimer;
        private int wayPointIndex = 0;
        private int lastWayPointIndex = 0;
        private bool enemyInSight = false;
        private bool firing = false;
        private Vector3 disturbancePoint;
        private float timeSearch;
        public float turnTimes = 0;
        private float timeEvade;
        private Vector3 targetDir;
        private Vector3 lastEulerAngle = Vector3.zero;
        private bool lookToHitDir = false;
        private bool lookingToHitDir = false;
        private float storageStoppingDistance;
        private bool evade = false;
        private Vector3 resetPosition = new Vector3(1000f, 1000f, 1000f);
        private Vector3 lastEnemySightingPos = new Vector3(1000f, 1000f, 1000f);
        private Vector3 enemyPreviousSighting;
        private Vector3 personalLastSighting;

        // Components
        private Health health;
        private ShipControl shipControlComponent;
        private EMInput emInputComponent;
        private NavMeshAgent navAgent;
        private Entity entityFollowing;
        private Entity lastEntityFollowing;
        private ImpactHit currentImpactHit;
        #endregion

        public virtual void Awake()
        {
            if (FOV == 0) FOV = 60f;
            if (FOVForFire == 0) FOVForFire = 10f;
            if (SearchGradesOnAlert == 0) SearchGradesOnAlert = 30f;
            if (minHealthToRecharge == 0) minHealthToRecharge = 15f;
            

            entity = GetComponent<Entity>();
            health = GetComponent<Health>();
            shipControlComponent = GetComponent<ShipControl>();
            emInputComponent = GetComponent<EMInput>();
            navAgent = gameObject.GetComponent<NavMeshAgent>();
            if (triggerPerception == null)
            {
                triggerPerception = GetComponent<SphereCollider>();
                if (triggerPerception == null)
                {
                    triggerPerception = GetComponentInChildren<SphereCollider>();
                }
            }

            storageStoppingDistance = navAgent.stoppingDistance;

            triggerPerception.isTrigger = true;
            triggerPerception.radius = perceptionRadius;
            
            entity.ImpactReceive += entity_ImpactReceive;

            patrolWayPoints = new List<GameObject>();

            foreach (Transform child in sector.transform)
            {
                patrolWayPoints.Add(child.gameObject);
            }

            if (behavior == Behavior.Patrol)
            {
                state = States.Patrol;
            }

            enemyPreviousSighting = resetPosition;
            personalLastSighting = resetPosition;
        }

        public virtual void Update()
        {
            if (ShowDebugMsgs && gameObject && navAgent && shipControlComponent) Debug.Log("[" + gameObject.name + "]\nState=" + state + "\nEvade=" + evade + "\nSpeed=" + navAgent.speed + "\nMovement=" + shipControlComponent.emInput.movement);

            if (lastEnemySightingPos != enemyPreviousSighting)
                personalLastSighting = lastEnemySightingPos;

            enemyPreviousSighting = lastEnemySightingPos;

            GoToRecharge();

            if (navAgent)
            {
                navAgent.baseOffset = Mathf.Lerp(navAgent.baseOffset, gameObject.GetComponent<Rigidbody>().velocity.y, Time.deltaTime);
                navAgent.height = navAgent.baseOffset;
            }

            if (behavior == Behavior.Patrol && state == States.StandBy)
            {
                state = States.Patrol;
            }

            Perception();

            Evade(currentImpactHit);

            if (state == States.StandBy || state == States.Patrol)
            {
                switch (behavior)
                {
                    case Behavior.Defender:
                        Defender();
                        break;
                    case Behavior.Explorer:
                        Explorer();
                        break;
                    case Behavior.Patrol:
                        Patrol();
                        break;
                }
            }
            else
            {
                if (state == States.Alert)
                {
                    OnAlert();
                }
                else if (state == States.Chase)
                {
                    Chasing();
                }
            }

            if (ShowNavPoints)
            {
                navAgent.destination.DebugCreateCircle(debugColor);
                Debug.DrawRay(gameObject.transform.position, (navAgent.destination - gameObject.transform.position), debugColor, 0.1f);
            }
        }

        public virtual void OnTriggerStay(Collider other)
        {
            try
            {
                if (NameKeyWordsIsEnemy.Any(nk => other.transform.root.tag.Contains(nk) || other.transform.root.name.Contains(nk)))
                {
                    if (!enemysInRange.Any(e => e.entity.Equals(other.transform.root.GetComponent<Entity>())))
                    {
                        enemysInRange.Add(new Enemies()
                        {
                            entity = other.transform.root.GetComponent<Entity>(),
                            distance = Vector3.Distance(transform.position, other.transform.root.position)
                        });
                    }
                    else
                    {
                        enemysInRange.Find(e => e.entity.Equals(other.transform.root.GetComponent<Entity>())).
                            Update(e => { e.distance = Vector3.Distance(transform.position, other.transform.root.position); });
                    }
                }
                else
                {
                    HealthCharge hch = other.GetComponent<HealthCharge>();
                    if (hch && 
                        entity.health.health < 99 &&
                        (other.transform.position - transform.position).sqrMagnitude < (perceptionRadius / 3) && 
                        state != States.Critical)
                    {
                        state = States.Critical;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }

        public virtual void OnTriggerExit(Collider other)
        {
            if (NameKeyWordsIsEnemy.Any(nk => other.transform.root.tag.Contains(nk) || other.transform.root.name.Contains(nk)))
            {
                if (!enemysInRange.Any(e => e.entity.gameObject.Equals(other.transform.root.gameObject)))
                {
                    enemysInRange.Remove(enemysInRange.Find(ef => ef.entity.Equals(other.transform.root.GetComponent<Entity>())));
                }
            }
        }

        public virtual void OnDisable()
        {
            if (entity)
            {
                entity.ImpactReceive += entity_ImpactReceive;
            }
        }

        public float CalculatePathLength(Vector3 targetPosition)
        {
            // Create a path and set it based on a target position.
            NavMeshPath path = new NavMeshPath();
            if (navAgent.enabled)
                navAgent.CalculatePath(targetPosition, path);

            // Create an array of points which is the length of the number of corners in the path + 2.
            Vector3[] allWayPoints = new Vector3[path.corners.Length + 2];

            // The first point is the enemy's position.
            allWayPoints[0] = transform.position;

            // The last point is the target position.
            allWayPoints[allWayPoints.Length - 1] = targetPosition;

            // The points inbetween are the corners of the path.
            for (int i = 0; i < path.corners.Length; i++)
            {
                allWayPoints[i + 1] = path.corners[i];
            }

            // Create a float to store the path length that is by default 0.
            float pathLength = 0;

            // Increment the path length by an amount equal to the distance between each waypoint and the next.
            for (int i = 0; i < allWayPoints.Length - 1; i++)
            {
                pathLength += Vector3.Distance(allWayPoints[i], allWayPoints[i + 1]);
            }

            return pathLength;
        }

        #region private methods

        void Perception()
        {
            RaycastHit hit;
            enemyInSight = false;
            enemysInRange.RemoveAll(e => e.entity == null || e.entity.transform == null);
            // por cada enemigo en la escena
            foreach (var item in enemysInRange)
            {
                // verifica si el enemigo no existe
                if (item == null || item.entity == null || item.entity.transform == null) break;

                Vector3 itemDir =  (item.entity.transform.position - gameObject.transform.position);
                // verifica el angulo del enemigo con respecto al actor
                float angle = Vector3.Angle(gameObject.transform.forward, itemDir);

                if (CalculatePathLength(item.entity.transform.position) <= perceptionRadius)
                {
                    //Debug.Log(item.entity.name + " PERCIBIDO!");
                }

                // si esta dentro del campo visual
                if (angle < (FOV * 0.5f))
                {
                    // busca el centro de masa asignado o sino el punto de posicion
                    Vector3 itemFollowingPos = Vector3.zero;
                    Vector3 aimHelperPos = Vector3.zero;

                    Vehicle vehicleEnemy = item.entity.GetComponent<Vehicle>();
                    Vehicle vehiclePersonal = GetComponent<Vehicle>();
                    if (vehicleEnemy != null)
                    {
                        itemFollowingPos = vehicleEnemy.centerOfMass.position;
                        aimHelperPos = vehiclePersonal.aimHelper.position;
                    }
                    else
                    {
                        itemFollowingPos = item.entity.transform.position;
                        aimHelperPos = transform.position;
                    }

                    Vector3 enemyDir = (itemFollowingPos - aimHelperPos);

                    // verifica que el enemigo se encuentre visible y en el rango de distancia
                    if (Physics.Raycast(aimHelperPos, enemyDir, out hit, perceptionRadius))
                    {
                        if (ShowLinesPerceptions) Debug.DrawRay(aimHelperPos, enemyDir, new Color(Color.red.r, Color.red.g, Color.red.b, 0.15f), 0.1f);
                        if (item.entity.gameObject == hit.transform.gameObject)
                        {
                            if (ShowLinesPerceptions) Debug.DrawRay(aimHelperPos, enemyDir, new Color(Color.green.r, Color.green.g, Color.green.b, 0.3f), 0.1f);
                            if (enemiesInFOV.Any(e => e.entity.Equals(item.entity)))
                            {
                                enemiesInFOV.Find(e => e.entity.Equals(item.entity)).Update(e => { e.entity = item.entity; e.distance = hit.distance; });
                            }
                            else
                            {
                                enemiesInFOV.Add(new Enemies()
                                {
                                    entity = item.entity,
                                    distance = hit.distance
                                });
                            }
                        }
                    }
                }
            }
            
            if (enemiesInFOV.Count > 0)
            {
                // quita de la lista los que esten muertos
                enemiesInFOV.RemoveAll(e => e.entity == null || e.entity.transform == null);
                if (enemiesInFOV.Count > 0)
                {
                    // ordena los enemigos por distancia
                    enemiesInFOV = enemiesInFOV.OrderBy(e => e.distance).ToList();

                    // ajusta el estado y guarda la entidad del enemigo
                    if (state != States.Critical) state = States.Chase;

                    entityFollowing = enemiesInFOV.First().entity;
                    lastEnemySightingPos = entityFollowing.transform.position;

                    if (CalculatePathLength(entityFollowing.transform.position)  <= perceptionRadius)
                    {
                        //Debug.Log(entityFollowing.name + " PERCIBIDO!");
                        personalLastSighting = entityFollowing.transform.position;
                    }
                }
            }
        }

        void Evade(ImpactHit hit)
        {
            if (evade && disturbancePoint != null)
            {
                timeEvade += Time.deltaTime;
                float phase = Mathf.Sin(timeEvade);
                float dirX = 0;
                if (phase > 0) dirX = 1;
                else dirX = -1;
                
                emInputComponent.SetMovement(true, dirX, CoreEnums.Axis2.X);

                float dirY = 0;
                float angle = Vector3.Angle(gameObject.transform.forward, (disturbancePoint - gameObject.transform.position));
                if (angle > 90) dirY = 1;
                else dirY = -1;

                emInputComponent.SetMovement(true, dirY, CoreEnums.Axis2.Y);
                evadeTimer += Time.deltaTime;
                if (evadeTimer > 5f)
                {
                    evade = false;
                    evadeTimer = 0;
                }
            }
        }

        void Chasing(bool noChangeState = false)
        {
            // si el enemigo no esta muerto
            if (entityFollowing && entityFollowing.transform != null)
            {
                // si esta siguiendo a un enemigo
                if (state == States.Chase)
                {

                    if ((entityFollowing.transform.position - transform.position).sqrMagnitude > 4f)
                        navAgent.destination = entityFollowing.transform.position;

                    navAgent.speed = chaseSpeed;

                }

                // si pasa mucho tiempo sin contacto visual resetea el comportamiento
                if (timeLost > maxTimeLost)
                {
                    if (!noChangeState) state = States.StandBy;
                    entityFollowing = null;
                    timeLost = 0;
                }
                else
                {
                    // rota a la posicion del enemigo que esta siguiendo
                    Vector3 relativePos = entityFollowing.transform.position - gameObject.transform.position;
                    relativePos.y = 0;
                    if (state != States.Critical)
                        gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, Quaternion.LookRotation(relativePos), Time.deltaTime * turnSpeedOnChase);

                    // verifica el angulo del enemigo con respecto al actor
                    float angle = Vector3.Angle(gameObject.transform.forward, (entityFollowing.transform.position - gameObject.transform.position));

                    // si el enemigo esta dentro del rango de tiro
                    if (angle < (FOVForFire * 0.5f))
                    {
                        timeLost = 0;
                        enemyInSight = true;
                    }
                    else
                    {
                        timeLost += Time.deltaTime;
                        enemyInSight = false;
                    }
                }
            }
            else
            {
                if (!noChangeState) state = States.StandBy;
                entityFollowing = null;
                timeLost = 0;
            }

            if (allowFire)
            {
                foreach (LaserWeapon lw in shipControlComponent.laserWeapons)
                {
                    lw.firing = enemyInSight;
                    firing = enemyInSight;
                }
            }
        }

        void GoToRecharge()
        {
            if (entity && entity.health.health < minHealthToRecharge)
            {
                state = States.Critical;
            }

            if (state == States.Critical)
            {
                List<ImpactHit> rechargePoints = (from e in GameObject.FindGameObjectsWithTag("recharge_point")
                                                  select new ImpactHit()
                                                  {
                                                      attacker = e,
                                                      distance = Vector3.Distance(gameObject.transform.position, e.transform.position)
                                                  }).ToList();

                if (rechargePoints.Count > 0)
                {
                    rechargePoints.OrderBy(rp => rp.distance);
                    if (navAgent)
                    {
                        navAgent.speed = chaseSpeed;
                        navAgent.ResetPath();
                        navAgent.Resume();
                        navAgent.SetDestination(rechargePoints.First().attacker.transform.position);
                    }
                }

                if (entity.health.health > 99)
                {
                    state = States.StandBy;
                }

                Chasing(true);
            }
        }

        void entity_ImpactReceive(MonoBehaviour mono, ImpactHit hit)
        {
            Projectile bullet = hit.attacker.GetComponent<Projectile>();
            if (bullet != null)
            {
                if (state != States.Chase && state != States.Alert)
                {
                    if (state != States.Critical)
                    {
                        state = States.Alert;
                    }
                    disturbancePoint = hit.point;
                    timeSearch = 0f;
                    targetDir = (hit.senderPos - transform.position);
                    targetDir.y = 0;
                }
                currentImpactHit = hit;
                evade = true;
            }
        }

        IEnumerator LookToDisturbance()
        {
            lookingToHitDir = true;
            Quaternion rotation = Quaternion.LookRotation(targetDir);
            float elapsedTime = 0;
            while (Quaternion.Angle(transform.rotation, rotation) > 1)
            {
                if (state != States.Alert)
                {
                    yield break;
                }
                Debug.Log(Quaternion.Angle(transform.rotation, rotation));
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, elapsedTime / speedLookAround);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            Debug.Log("OK");
            lookToHitDir = true;
            lookingToHitDir = false;
            lastEulerAngle = transform.eulerAngles;
        }

        #endregion

        protected virtual void Defender()
        {

        }

        protected virtual void Explorer()
        {

        }

        protected virtual void Patrol()
        {
            navAgent.Resume();
            navAgent.speed = patrolSpeed;

            Vector3 aPos = Vector3.zero;
            
            if (navAgent.remainingDistance < navAgent.stoppingDistance)
            {
                patrolTimer += Time.deltaTime;

                if (patrolTimer >= patrolWaitTime)
                {
                    // ... increment the wayPointIndex.
                    if (wayPointIndex == patrolWayPoints.Count - 1) wayPointIndex = 0;
                    else
                    {
                        lastWayPointIndex = wayPointIndex;
                        wayPointIndex++;
                    }

                    // Reset the timer.
                    patrolTimer = 0;
                }
            }
            else
            {
                patrolTimer = 0;
            }

            //if (wayPointIndex == 0 && wayPointIndex == lastWayPointIndex) aPos = startPos;
            //else aPos = patrolWayPoints[lastWayPointIndex].transform.position;

            //Debug.DrawLine(aPos, patrolWayPoints[wayPointIndex].transform.position, Color.green);
            navAgent.destination = patrolWayPoints[wayPointIndex].transform.position;

            state = States.Patrol;
        }

        protected virtual void OnAlert()
        {
            if (state == States.Alert)
            {
                if (disturbancePoint != null)
                {
                    navAgent.Stop();

                    if (!lookToHitDir)
                    {
                        if (!lookingToHitDir) StartCoroutine(LookToDisturbance());
                    }
                    if (lookToHitDir)
                    {
                        if (turnTimes < SearchGradesOnAlert)
                        {
                            timeSearch += Time.deltaTime;
                            float phase = Mathf.Sin(timeSearch);
                            turnTimes += Mathf.Abs(phase);

                            gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, Quaternion.Euler(new Vector3(0, lastEulerAngle.y + phase * (SearchGradesOnAlert * 0.5f), 0)), Time.deltaTime * speedLookAround);
                        }
                        else if (turnTimes >= SearchGradesOnAlert)
                        {
                            timeSearch = 0;
                            turnTimes = 0;
                            lookToHitDir = false;
                            state = States.StandBy;
                        }
                    }
                }
                else
                {
                    state = States.StandBy;
                }
            }
        }
    }

}