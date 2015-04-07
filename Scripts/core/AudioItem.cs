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
    [System.Serializable]
    public class AudioItem : System.Object
    {

        public string name;
        public AudioClip audioClip;
        public bool mute = false;
        public bool bypassEffects = false;
        public bool bypassListenerEffects = false;
        public bool bypassReverbZones = false;
        public bool playOnAwake = false;
        public bool loop = false;
        [Range(-3f, 3f)]
        public float pitch = 1f;
        [Range(0f, 1f)]
        public float volume = 0.5f;
        [Range(0,255)]
        public int priority = 128;
        public AudioRolloffMode audioRolloffMode = AudioRolloffMode.Linear;
        [Range(0f, 5f)]
        public float dopplerLevel = 1f;
        [Range(-1f, 1f)]
        public float stereoPan = 0f;
        [Range(0.0f, 1.0f)]
        public float spatialBlend = 0f;
        [Range(0.0f, 1.1f)]
        public float reverbZoneMix = 1f;
        [Range(0f, 360f)]
        public float spread = 360f;
        public float minDistance = 0f;
        public float maxDistance = 500f;
        public Transform tagSpawn;
        public string findString = "";
        public bool inRoot = true;
        public bool isUnique = true;
        public bool isAutoDisable = false;

        public float VolumeProperty { get; private set; }

        [System.NonSerialized]
        public GameObject gameObject;

        private MonoBehaviour monoBehaviour;
        private AudioSource audioSource;
        private bool fadeInInit = false;

        public void Inizialice()
        {
            this.VolumeProperty = this.volume;
        }

        public void Inizialice(MonoBehaviour monoBehaviour)
        {
            this.VolumeProperty = this.volume;
            this.monoBehaviour = monoBehaviour;
        }

        public void Inizialice(AudioItem audioItem)
        {
            VolumeProperty = this.volume;

            this.audioClip = audioItem.audioClip;
            this.mute = audioItem.mute;
            this.bypassEffects = audioItem.bypassEffects;
            this.bypassListenerEffects = audioItem.bypassListenerEffects;
            this.bypassReverbZones = audioItem.bypassReverbZones;
            this.playOnAwake = audioItem.playOnAwake;
            this.loop = audioItem.loop;
            this.pitch = audioItem.pitch;
            this.volume = audioItem.volume;
            this.priority = audioItem.priority;
            this.dopplerLevel = audioItem.dopplerLevel;
            this.minDistance = audioItem.minDistance;
            this.spatialBlend = audioItem.spatialBlend;
            this.stereoPan = audioItem.stereoPan;
            this.spread = audioItem.spread;
            this.maxDistance = audioItem.maxDistance;
            this.reverbZoneMix = audioItem.reverbZoneMix;
            this.audioRolloffMode = audioItem.audioRolloffMode;
        }

        public IEnumerator FadeIn(AudioSource audioSource, float seconds)
        {
            this.audioSource = audioSource;
            if(!fadeInInit)
            {
                this.audioSource.volume = 0;
                fadeInInit = true;
            }

            float elapsedTime  = 0;
            while (elapsedTime < seconds)
            {
                this.audioSource.volume = Mathf.Lerp(this.audioSource.volume, this.VolumeProperty, (elapsedTime / seconds));
                elapsedTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForSeconds(seconds);

            Reset(audioSource);
        }

        public IEnumerator FadeOut(AudioSource audioSource, float seconds)
        {
            this.audioSource = audioSource;

            float elapsedTime = 0;
            while (elapsedTime < seconds)
            {
                this.audioSource.volume = Mathf.InverseLerp(0, this.audioSource.volume, (elapsedTime / seconds));
                elapsedTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForSeconds(seconds);

            this.audioSource.gameObject.SetActive(false);
            Reset(audioSource);
        }

        public void Reset(AudioSource audioSource)
        {
            if (audioSource != null)
                audioSource.volume = this.VolumeProperty;
            fadeInInit = false;
        }
    }
}
