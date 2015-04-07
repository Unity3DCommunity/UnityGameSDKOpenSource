using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Core
{
    public class PooledItem : MonoBehaviour
    {
        public bool autoDisable = true;
        public float timeToDisable = 2f;

        void OnEnable()
        {
            if (autoDisable)
            {
                Invoke("Disable", timeToDisable);
            }
        }

        void Disable()
        {
            gameObject.SetActive(false);
        }

        void OnDisable()
        {
            if (autoDisable)
            {
                CancelInvoke();
            }
        }
    }
}
