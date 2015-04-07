using UnityEngine;
using System.Collections;

namespace Core
{
    public class ImpactHit
    {
        public GameObject attacker { get; set; }
        public Vector3 normal { get; set; }
        public Vector3 point { get; set; }
        public float distance { get; set; }
        public Collider collider { get; set; }
        public Vector3 attackerPos { get; set; }
        public Vector3 senderPos { get; set; }
    }
}
