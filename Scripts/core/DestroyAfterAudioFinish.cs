/* Author: Eleazar Celis
 * Copyright: EM Click
 * Version: 1.0
 * - All rights reserved -
 * 
 * for questions about this or any other script 
 * developed by the author, write to the following email
 * em.click@outlook.com
*/ 

using UnityEngine;
using System.Collections;

namespace Core
{
    public class DestroyAfterAudioFinish : MonoBehaviour
    {

        public bool destroyMe = false;

        private AudioSource audioSource;
        [System.NonSerialized]
        public bool isPlaying = false;
        [System.NonSerialized]
        public bool isPooled = false;
        [System.NonSerialized]
        public bool auto = false;

        void Update()
        {

            audioSource = GetComponent<AudioSource>();

            if (audioSource.isPlaying && !isPlaying)
            {
                isPlaying = true;
            }

            if (isPlaying && !audioSource.isPlaying)
            {
                destroyMe = true;
            }

            if (destroyMe)
            {
                if (isPooled)
                {
                    if (auto) gameObject.SetActive(false);
                }
                else
                {
                    if (auto) Destroy(gameObject);
                }
            }
        }
    }
}
