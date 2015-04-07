using UnityEngine;
using System.Collections;
using System;

namespace Core
{
    [RequireComponent(typeof(Health))]
    public class Entity : MonoBehaviour
    {
        public enum Types
        {
            Actor,
            Vehicle,
            Item,
            Accessory,
        }

        public delegate void ImpactReceiveAction(MonoBehaviour mono, ImpactHit hit);
        public event ImpactReceiveAction ImpactReceive;

        public Types type;

        [System.NonSerialized]
        public NetworkView myNetworkView;
        [System.NonSerialized]
        public GameControl gameControl;

        internal Health health;

        public virtual void Awake()
        {
            myNetworkView = GetComponent<NetworkView>();
            gameControl = GameObject.FindObjectOfType<GameControl>();
        }

        public virtual void Start()
        {
            if (gameControl.gameMode == CoreEnums.GameModes.Missions || myNetworkView.isMine)
            {
                health = GetComponent<Health>();
            }
        }

        public virtual void Update()
        {

        }

        public virtual void FixedUpdate()
        {

        }

        public void OnImpactReceive(ImpactHit hit)
        {
            if (gameControl.gameMode == CoreEnums.GameModes.Missions || myNetworkView.isMine)
            {
                if (ImpactReceive != null && hit != null)
                {
                    ImpactReceive(this, hit);
                }
            }
        }
    }
}
