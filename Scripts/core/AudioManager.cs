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
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Core
{
    public class AudioManager : MonoBehaviour
    {
        public string rootPath = "audio/";
        public List<AudioItem> audioDictionary = new List<AudioItem>();

        private Transform tRoot;
        Entity thisEntity;

        void Awake()
        {
            thisEntity = GetComponent<Entity>();
            if (thisEntity.gameControl.gameMode == CoreEnums.GameModes.Missions || thisEntity.myNetworkView.isMine)
            {
                // establece el root de audios
                tRoot = transform.FindChild(rootPath);
                if (tRoot == null)
                {
                    GameObject goRoot = new GameObject(rootPath.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries).Last());
                    goRoot.transform.localPosition = Vector3.zero;
                    goRoot.transform.localRotation = Quaternion.identity;
                }

                foreach (AudioItem audioItem in audioDictionary)
                {
                    // busca en findString del audioItem si este no tiene un tagSpawn
                    if (audioItem.tagSpawn == null)
                    {
                        if (!string.IsNullOrEmpty(audioItem.findString)) audioItem.tagSpawn = transform.Find(audioItem.findString);
                    }
                    GameObject obj = new GameObject(audioItem.name); // crea un gameobject con el nombre del audioItem
                    obj.transform.position = audioItem.tagSpawn.position;
                    obj.transform.rotation = audioItem.tagSpawn.rotation;
                    obj.transform.parent = tRoot; // establece tRoot como padre de este objeto
                    // agrega el componente audiosource
                    AudioSource audioSource = AddAudioSource(obj, audioItem, audioItem.inRoot);

                    GameObject pooledBase = new GameObject(audioItem.name + "_pooledbase"); // crea un gameobject que gestionara el pooling del audioitem
                    pooledBase.transform.position = audioItem.tagSpawn.position;
                    pooledBase.transform.rotation = audioItem.tagSpawn.rotation;

                    pooledBase.transform.parent = tRoot;
                    PooledObject pooledItem = pooledBase.AddComponent<PooledObject>();
                    pooledItem.pooledObject = obj;
                    pooledItem.willGrow = !audioItem.isUnique;
                    pooledItem.pooledAmount = 1;

                    audioItem.Inizialice(this);
                    obj.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Reproduce una pista cuyo nombre en el diccionario sea igual a la especificada en un GameObject.
        /// </summary>
        /// <param name="clipName">Nombre del item en el diccionario.</param>
        /// <returns></returns>
        public GameObject Play(string clipName)
        {
            if (tRoot == null) tRoot = transform.FindChild(rootPath);

            // obtiene el objeto AudioItem
            AudioItem audioItem = audioDictionary.Find(a => a.name == clipName);
            if (audioItem != null)
            {
                bool isUnique = false;
                Transform root = null;
                Vector3 pos = audioItem.tagSpawn.position;
                Quaternion rot = audioItem.tagSpawn.rotation;
                Transform tChild = null;
                Transform tChildPooled = null;

                // establece el transform root en el que se buscaran los objetos de audio
                if (audioItem.inRoot) root = tRoot;
                else root = audioItem.tagSpawn;
                // si es unico
                if (audioItem.isUnique)
                {
                    tChild = root.FindChild(clipName); // establece el gameobject con al componente audiosource
                    tChildPooled = root.FindChild(audioItem.name + "_pooledbase"); // establece el gameobject con el pooledobject

                    if (tChild)
                    {
                        PooledObject po = tChildPooled.GetComponent<PooledObject>(); // obtiene el componente pooledobject

                        if ((po && po.pooledObjects.Count > 0 && po.pooledObjects.Any(p => p.activeSelf)) ||
                        (tChild && tChild.gameObject.activeSelf))
                        {
                            isUnique = true;
                        }
                    }
                }
                
                if (!isUnique)
                {
                    if (!audioItem.loop)
                    {
                        PooledObject po = root.FindChild(audioItem.name + "_pooledbase").GetComponent<PooledObject>();
                        GameObject obj = po.GetPooledObject(pos, rot);
                        obj.transform.parent = tRoot;
                        if (obj == null) return null;

                        obj.transform.rotation = rot;
                        obj.transform.position = pos;
                        obj.SetActive(true);
                    }
                    else
                    {
                        root.FindChild(clipName).gameObject.SetActive(true);
                    }
                }

                return root.FindChild(clipName).gameObject;
            }
            return null;
        }

        /// <summary>
        /// Reproduce una o varias pistas cuyo nombre en el diccionario comience con una cadena especificada en un GameObject.
        /// </summary>
        /// <param name="clipName">Nombre del item en el diccionario.</param>
        public void PlayStartWith(string clipName)
        {
            List<AudioItem> audioItems = audioDictionary.FindAll(a => a.name.StartsWith(clipName));
            
            foreach (AudioItem audioItem in audioItems)
            {
                Play(audioItem.name);
            }
        }

        /// <summary>
        /// Detiene la reproduccion de una pista cuyo nombre en el diccionario sea igual a la especificada en un GameObject.
        /// </summary>
        /// <param name="clipName">Nombre del item en el diccionario.</param>
        public void Stop(string clipName)
        {
            AudioItem audioItem = audioDictionary.Find(a => a.name == clipName);
            if (audioItem != null)
            {
                if (!audioItem.loop) clipName = audioItem.name + "(Clone)";
                tRoot.FindChild(clipName).gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Detiene la reproduccion de una o varias pistas cuyo nombre en el diccionario comience con una cadena especificada en un GameObject.
        /// </summary>
        /// <param name="clipName">Nombre del item en el diccionario.</param>
        public void StopStartWith(string clipName)
        {
            List<AudioItem> audioItems = audioDictionary.FindAll(a => a.name.StartsWith(clipName));
            foreach (AudioItem audioItem in audioItems)
            {
                if (!audioItem.loop)
                    tRoot.FindChild(audioItem.name + "(Clone)").gameObject.SetActive(false);
                else
                    tRoot.FindChild(audioItem.name).gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Agrega un componente AudioSource en un GameObject especificado.
        /// </summary>
        /// <param name="audioSpawned">GameObject al cual se le agregara el componente AudioSource.</param>
        /// <param name="audioItem">Objeto AudioItem que contiene la informacion para crear el componente AudioSource.</param>
        /// <param name="inRoot"></param>
        /// <returns></returns>
        private AudioSource AddAudioSource(GameObject audioSpawned, AudioItem audioItem, bool inRoot = true)
        {
            AudioSource audioSource = audioSpawned.AddComponent<AudioSource>();
            audioSource.clip = audioItem.audioClip;
            audioSource.mute = audioItem.mute;
            audioSource.bypassEffects = audioItem.bypassEffects;
            audioSource.bypassListenerEffects = audioItem.bypassListenerEffects;
            audioSource.bypassReverbZones = audioItem.bypassReverbZones;
            audioSource.playOnAwake = audioItem.playOnAwake;
            audioSource.loop = audioItem.loop;
            audioSource.pitch = audioItem.pitch;
            audioSource.volume = audioItem.volume;
            audioSource.priority = audioItem.priority;
            audioSource.dopplerLevel = audioItem.dopplerLevel;
            audioSource.minDistance = audioItem.minDistance;
            audioSource.spatialBlend = audioItem.spatialBlend;
            audioSource.panStereo = audioItem.stereoPan;
            audioSource.spread = audioItem.spread;
            audioSource.maxDistance = audioItem.maxDistance;
            audioSource.reverbZoneMix = audioItem.reverbZoneMix;
            audioSource.rolloffMode = audioItem.audioRolloffMode;

            DestroyAfterAudioFinish destroyAfterAudioFinish = audioSpawned.AddComponent<DestroyAfterAudioFinish>();

            PooledItem pooledItem = audioSpawned.AddComponent<PooledItem>();
            if (!audioSource.loop)
            {
                if (audioItem.isAutoDisable)
                {
                    pooledItem.timeToDisable = audioItem.audioClip.length;
                }
            }
            pooledItem.autoDisable = audioItem.isAutoDisable;
            destroyAfterAudioFinish.isPooled = true;
            destroyAfterAudioFinish.auto = audioItem.isAutoDisable;
            return audioSource;
        }

        public void SetTagSpawn(string clipName, Transform tag)
        {
            if (!string.IsNullOrEmpty(clipName) && tag != null)
            {
                AudioItem audioItem = audioDictionary.Find(a => a.name == clipName);
                audioItem.tagSpawn = tag;
            }
        }

        public bool AudioExists(string clipName)
        {
            AudioItem audioItem = audioDictionary.Find(a => a.name == clipName);
            if (audioItem != null) return true;

            return false;
        }

        public bool AudioExistsStartWith(string clipName)
        {
            List<AudioItem> audioItems = audioDictionary.FindAll(a => a.name.StartsWith(clipName));
            if (audioItems.Count > 0) return true;

            return false;
        }

        public bool IsPlaying(string clipName)
        {
            AudioItem audioItem = audioDictionary.Find(a => a.name == clipName);
            if (audioItem != null)
            {
                Transform audioObject = tRoot.FindChild(audioItem.name);
                if (!audioItem.loop && audioItem.isUnique) audioObject = tRoot.FindChild(audioItem.name + "(Clone)");
                if (audioObject != null)
                {
                    AudioSource audioComponent = audioObject.GetComponent<AudioSource>();
                    if (audioComponent.isPlaying) return true;
                    else return false;
                }
            }

            return false;
        }

        public bool IsPlayingStartWith(string clipName)
        {
            List<AudioItem> audioItems = audioDictionary.FindAll(a => a.name.StartsWith(clipName));
            foreach (AudioItem audioItem in audioItems)
            {
                if (IsPlaying(audioItem.name))
                {
                    return true;
                }
            }

            return false;
        }

        public AudioSource GetAudioSource(string clipName)
        {
            AudioItem audioItem = audioDictionary.Find(a => a.name == clipName);
            if (audioItem != null)
            {
                Transform audioObject = tRoot.FindChild(audioItem.name);
                if (!audioItem.loop && audioItem.isUnique) audioObject = tRoot.FindChild(audioItem.name + "(Clone)");
                if (audioObject != null)
                {
                    AudioSource audioComponent = audioObject.GetComponent<AudioSource>();
                    if (audioComponent != null) return audioComponent;
                }
            }

            return null;
        }

        public List<AudioSource> GetAudioSourceStartWith(string clipName)
        {
            List<AudioItem> audioItems = audioDictionary.FindAll(a => a.name.StartsWith(clipName));
            List<AudioSource> output = new List<AudioSource>();
            foreach (AudioItem audioItem in audioItems)
            {
                AudioSource audioComponent = GetAudioSource(audioItem.name);
                if (audioComponent != null) output.Add(audioComponent);
            }

            return output;
        }

        public void BlendClips(string clipNameA, string clipNameB, float seconds, bool forceFinish = false)
        {
            AudioSource audioSource = GetAudioSource(clipNameA);
            if (forceFinish)
            {
                PlayFadeIn(clipNameB, seconds);
                StopFadeOut(clipNameA, seconds);
            }
            else
            {
                if (audioSource.time > (audioSource.clip.length - seconds))
                {
                    PlayFadeIn(clipNameB, seconds);
                    StopFadeOut(clipNameA, seconds);
                }
            }
        }

        public void BlendClipsStartWith(string clipNameA, string clipNameB, float seconds, bool forceFinish = false)
        {
            List<AudioItem> audioItemsA = audioDictionary.FindAll(a => a.name.StartsWith(clipNameA));
            List<AudioItem> audioItemsB = audioDictionary.FindAll(a => a.name.StartsWith(clipNameB));
            int i = 0;
            foreach (AudioItem audioItem in audioItemsA)
            {
                if (i < audioItemsB.Count)
                {
                    BlendClips(audioItem.name, audioItemsB[i].name, seconds, forceFinish);
                }
                i++;
            }
        }

        public void PlayFadeIn(string clipName, float seconds)
        {
            AudioItem audioItem = audioDictionary.Find(a => a.name == clipName);

            if (!IsPlaying(clipName)) Play(audioItem.name);
            AudioSource audioSource = GetAudioSource(audioItem.name);
            StartCoroutine(audioItem.FadeIn(audioSource, seconds));
            if (audioSource.volume >= audioItem.VolumeProperty)
            {
                //Debug.Log("PlayFadeIn Completa");
            }
        }

        public void PlayFadeInStartWith(string clipName, float seconds)
        {
            List<AudioItem> audioItems = audioDictionary.FindAll(a => a.name.StartsWith(clipName));

            foreach (AudioItem audioItem in audioItems)
            {
                PlayFadeIn(audioItem.name, seconds);
            }
        }

        public void StopFadeOut(string clipName, float seconds)
        {
            AudioItem audioItem = audioDictionary.Find(a => a.name == clipName);

            AudioSource audioSource = GetAudioSource(audioItem.name);
            StartCoroutine(audioItem.FadeOut(audioSource, seconds));
            if (audioSource.volume == 0)
            {
                audioSource.Stop();
            }
        }

        public void StopFadeOutStartWith(string clipName, float seconds)
        {
            List<AudioItem> audioItems = audioDictionary.FindAll(a => a.name.StartsWith(clipName));

            foreach (AudioItem audioItem in audioItems)
            {
                StopFadeOut(audioItem.name, seconds);
            }
        }
    }
}
