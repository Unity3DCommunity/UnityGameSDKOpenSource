using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Globalization;
using Projectiles;

namespace Core
{
    public static class Extensions
    {

        public static T CustomFindChild<T>(this Transform root, string nameChild)
        {
            int count = 0;
            List<Transform> tr = new List<Transform>();
            Transform match = null;
            foreach (Transform _transform in root)
            {
                if (_transform.name == nameChild)
                {
                    match = _transform;
                }
                tr.Add(_transform);
                count++;
            }

            if (count < 1)
            {
                return (T)Convert.ChangeType(false, typeof(T), CultureInfo.InvariantCulture);
            }

            Type paramType = typeof(T);
            if (paramType == typeof(int))
            {
                return (T)Convert.ChangeType(count, typeof(T), CultureInfo.InvariantCulture);
            }
            else if (paramType == typeof(List<Transform>))
            {
                return (T)Convert.ChangeType(tr, typeof(T), CultureInfo.InvariantCulture);
            }
            else
            {
                return (T)Convert.ChangeType(match, typeof(T), CultureInfo.InvariantCulture);
            }
        }
        
        /// <summary>
        /// Calcula el valor de un porcentaje
        /// </summary>
        /// <param name="dim">Porcentaje</param>
        /// <param name="maxFactor">Valor maximo</param>
        /// <returns></returns>
        public static float ValueFromPercent(this float dim, float maxFactor)
        {
            return (maxFactor / 100) * dim;
        }

        /// <summary>
        /// Calcula el porcentaje de un valor
        /// </summary>
        /// <param name="dim">Valor</param>
        /// <param name="maxFactor">Valor maximo</param>
        /// <returns></returns>
        public static float GetPercent(this float dim, float maxFactor)
        {
            return (dim * 100) / maxFactor;
        }

        public static float Round(this float f, int d = 2)
        {
            return (float)System.Math.Round((double)f, d);
        }

        public static AudioSource FromAudioManager(this AudioSource audioSource, AudioItem audioItem)
        {
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
            return audioSource;
        }

        public static Vector3 MultiplyVector(this Vector3 from, Vector3 multiplier, bool inverse = false, Vector3 inheritVector = default(Vector3))
        {
            Vector3 result = Vector3.zero;
            Vector3 _inheritVector = Vector3.zero;
            if (inverse)
            {
                multiplier.x = (multiplier.x == 0 ? 1f : 0);
                multiplier.y = (multiplier.y == 0 ? 1f : 0);
                multiplier.z = (multiplier.z == 0 ? 1f : 0);
            }
            result.x = from.x * multiplier.x;
            result.y = from.y * multiplier.y;
            result.z = from.z * multiplier.z;

            if (inheritVector != Vector3.zero)
            {
                _inheritVector = inheritVector.MultiplyVector(multiplier, inverse);
            }
            return result + _inheritVector;
        }

        public static List<GameObject> FindGameObjectsContaining(this GameObject thisGO, string anyWord)
        {
            List<GameObject> gos = ((GameObject[])GameObject.FindObjectsOfType(typeof(GameObject))).ToList();

            if (gos.Count < 1)
                return null;

            var result = (from go in gos
                          where go.name.Contains(anyWord) || go.tag.Contains(anyWord) && go != thisGO
                          select go).ToList();

            if (result.Count > 0)
                return result;

            return null;
        }

        public static List<GameObject> FindGameObjectsContaining(this GameObject thisGO, List<string> anyWords)
        {
            List<GameObject> gos = ((GameObject[])GameObject.FindObjectsOfType(typeof(GameObject))).ToList();

            if (gos.Count < 1)
                return null;

            List<GameObject> output = new List<GameObject>();
            foreach (string word in anyWords)
            {
                var result = (from go in gos
                              where go.name.Contains(word) || go.tag.Contains(word) && go != thisGO
                              select go).ToList();
                output.AddRange(result);
            }



            if (output.Count > 0)
                return output;

            return null;
        }

        public static List<GameObject> FindGameObjectsContaining(this GameObject thisGO, List<string> anyWords, Type type)
        {
            List<GameObject> gos = ((GameObject[])GameObject.FindObjectsOfType(type)).ToList();

            if (gos.Count < 1)
                return null;

            List<GameObject> output = new List<GameObject>();
            foreach (string word in anyWords)
            {
                var result = (from go in gos
                              where go.name.Contains(word) || go.tag.Contains(word) && go != thisGO
                              select go).ToList();
                output.AddRange(result);
            }



            if (output.Count > 0)
                return output;

            return null;
        }

        public static List<Component> GetComponentsByType<T>(this GameObject thisGO)
        {
            List<Component> result = new List<Component>();
            foreach (Transform transform in thisGO.transform)
            {
                Component com = transform.GetComponent(typeof(T));
                if (com != null)
                {
                    result.Add(com);
                }
            }

            if (result.Count > 0) return result;
            return null;
        }

        public static bool ContainsAnyWord(this Transform go, string anyWord)
        {
            if (go.tag.Contains(anyWord) || go.name.Contains(anyWord))
                return true;

            return false;
        }

        public static bool ContainsAnyWord(this Transform go, List<string> anyWords)
        {
            foreach (string word in anyWords)
            {
                if (go.tag.Contains(word) || go.name.Contains(word))
                    return true;
            }

            return false;
        }

        public static bool GreaterThan(this Vector2 vecA, Vector2 vecB)
        {
            if (vecA.x > vecB.x)
            {
                if (vecA.y > vecB.y)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool LessThan(this Vector2 vecA, Vector2 vecB)
        {
            if (vecA.x < vecB.x)
            {
                if (vecA.y < vecB.y)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool Equal(this Vector2 vecA, Vector2 vecB)
        {
            if (vecA.x == vecB.x)
            {
                if (vecA.y == vecB.y)
                {
                    return true;
                }
            }
            return false;
        }

        public static TSource GetControl<TSource>(this IEnumerable<TSource> ControlList, string Name)
        {
            TSource control = ControlList.Single(i => (i as InputPlatform).keyName == Name);

            return control;
        }

        public static bool GetControl<TSource>(this IEnumerable<TSource> ControlList, string Name, out TSource control)
        {
            control = ControlList.Single(i => (i as InputPlatform).keyName == Name);
            if (control != null) return true;
            return false;
        }

        public static ImpactHit ToImpactHit(this RaycastHit hit)
        {
            ImpactHit impact = new ImpactHit();
            impact.normal = hit.normal;
            impact.point = hit.point;
            impact.distance = hit.distance;
            impact.collider = hit.collider;
            return impact;
        }

        public static ImpactHit ToImpactHit(this RaycastHit hit, GameObject gameObject)
        {
            ImpactHit impact = new ImpactHit();
            impact.attackerPos = gameObject.transform.position;
            if (gameObject.GetComponent<Projectile>())
            {
                if (gameObject.GetComponent<Projectile>().sender.transform)
                {
                    impact.senderPos = gameObject.GetComponent<Projectile>().sender.transform.position;
                }
            }
            
            impact.attacker = gameObject;
            impact.normal = hit.normal;
            impact.point = hit.point;
            impact.distance = hit.distance;
            impact.collider = hit.collider;
            return impact;
        }


        public delegate void Func<TArg0>(TArg0 element);

        public static void Update<TSource>(this TSource source, Func<TSource> update)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (update == null) throw new ArgumentNullException("update");
            if (typeof(TSource).IsValueType)
                throw new NotSupportedException("value type elements are not supported by update.");

            update(source);
        }

        public static float Occilation(this float value, float range, float frecuency)
        {
            return Mathf.Sin(Time.time * frecuency) * (range) + (value);
        }

        public static void DebugCreateCircle(this Vector3 origin, Color color, float radius = 5f, float resolution = 100f)
        {
            float theta_scale = 0.1f;
            int size = Mathf.RoundToInt((2.0f * 3.14f) / theta_scale);

            float angle = 90;
            Vector3 pos = origin;
            Vector3 lastpos = origin;


            for (int i = 0; i < (resolution + 1); i++)
            {
                pos.x = (Mathf.Sin(Mathf.Deg2Rad * angle) * 5) + origin.x;
                pos.z = (Mathf.Cos(Mathf.Deg2Rad * angle) * 5) + origin.z;

                Debug.DrawRay(lastpos, (pos - lastpos), color, 0.1f);

                lastpos = pos;
                angle += (360f / 30);
            }
            //Debug.DrawRay(gameObject.transform.position, (navAgent.destination - gameObject.transform.position), debugColor, 0.1f);
        }
    }
}
