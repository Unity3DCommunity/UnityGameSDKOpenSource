using UnityEngine;
using System.Collections;
using Core;
using Projectiles;
using ParticleSystems.Effects;
using ParticleSystems.Detonator;

namespace Weapons
{
    [System.Serializable]
    public class SpecialGun : System.Object
    {
        public AudioManager fxAudio;
        public Transform barrel;
        public float timeToFire = 10f;
        [Range(0.3f, 1.0f)]
        public float minPower = 0.3f;
        public float speedCharge = 5f;
        public Transform tagBullets;
        public Transform tagMuzzel;
        public GameObject[] muzzlePrefabs;
        public GameObject bulletsPrefab;
        public GameObject energyUI;
        public GameObject chargeUI;

        [System.NonSerialized]
        public float waitTilNextFire = 0;

        [System.NonSerialized]
        public int currentMuzzleIndex = 0;
        [System.NonSerialized]
        public float power;
        private bool fire = false;
        private ParticleSystem particle;
        private Light light;
        private float? lightIntensity;
        private float lightRange;
        private float size;
        private GameObject bullet;
        private Bullet bulletComponent;
        private float bulletSpeed;

        public GameObject GetMuzzleRandom()
        {
            currentMuzzleIndex = Random.Range(0, muzzlePrefabs.Length);
            return muzzlePrefabs[currentMuzzleIndex];
        }

        public void Fire(bool charge)
        {
            if (barrel != null)
            {
                if (energyUI)
                {
                    float energy = Mathf.Clamp(waitTilNextFire, 0, timeToFire).GetPercent(timeToFire) / 100;
                    RectTransform rt = energyUI.GetComponent<RectTransform>();
                    if (rt)
                    {
                        Vector3 ls = rt.localScale;
                        ls.x = energy;
                        rt.localScale = ls;
                    }
                }

                if (chargeUI)
                {
                    float _power = Mathf.Clamp(power, 0, 10).GetPercent(10) / 100;
                    RectTransform rt = chargeUI.GetComponent<RectTransform>();
                    if (rt)
                    {
                        Vector3 ls = rt.localScale;
                        ls.x = _power;
                        rt.localScale = ls;
                    }
                }

                if (waitTilNextFire >= timeToFire)
                {
                    // cargando
                    if (charge)
                    {
                        // si el poder es menor que 10
                        if (power < 10f)
                        {
                            power += Time.deltaTime * speedCharge;
                            if (bullet == null && (power * 0.1f) > minPower)
                            {
                                bullet = MonoBehaviour.Instantiate(bulletsPrefab, tagBullets.position, tagBullets.rotation) as GameObject;
                                bulletComponent = bullet.GetComponent<Bullet>();
                                bulletComponent.sender = tagBullets.root.gameObject;
                                bulletComponent.fire = false;
                                particle = bullet.GetComponentInChildren<ParticleSystem>();
                                light = bullet.GetComponentInChildren<Light>();
                                if (!lightIntensity.HasValue)
                                {
                                    lightIntensity = light.intensity;
                                    lightRange = light.range;
                                }
                            }

                            if (fxAudio && fxAudio.AudioExistsStartWith("Power Charging"))
                                fxAudio.PlayStartWith("Power Charging");
                        }
                        else
                        {
                            if (fxAudio &&
                                fxAudio.AudioExistsStartWith("Power Stay") &&
                                fxAudio.IsPlayingStartWith("Power Charging"))
                            {
                                fxAudio.BlendClipsStartWith("Power Charging", "Power Stay", 3f, true);
                            }
                        }

                        if (bullet && bulletComponent)
                        {
                            Bullet bp = bulletsPrefab.GetComponent<Bullet>();
                            float dr = bp.damageRate;

                            bullet.transform.position = tagBullets.position;
                            bullet.transform.rotation = tagBullets.rotation;
                            particle.startSize = (power * 0.1f);
                            light.intensity = power.GetPercent(10).ValueFromPercent(lightIntensity.Value);
                            light.range = power.GetPercent(10).ValueFromPercent(lightRange);
                            bulletComponent.damageRate = (dr / 10) * power;
                        }
                    }
                    else if ((power * 0.1f) < minPower && !charge) // si el poder es menor a minpower y no se esta cargando
                    {
                        power = 0f;
                        if (fxAudio && fxAudio.IsPlayingStartWith("Power Charging"))
                            fxAudio.StopFadeOutStartWith("Power Charging", 2f);
                    }
                }
                else
                {
                    power = 0f;
                }
                GameObject holdMuzzelFlash = null;
                // si el poder es mayor a minpower y no se esta cargando
                if ((power * 0.1f) > minPower && !charge)
                {
                    if (bulletsPrefab != null)
                    {
                        if (bullet && bulletComponent)
                        {
                            bullet.transform.position = tagBullets.position;
                            bulletComponent.fire = true;
                            Bullet bp = bulletsPrefab.GetComponent<Bullet>();

                            if (bp)
                            {
                                Explosion expl = bp.explosion.GetComponent<Explosion>();
                                if (expl)
                                {
                                    float explSize = expl.size;
                                    float explForce = expl.forcePower;
                                    bp.explosionSize = (explSize / 10) * power;
                                    bp.explosionForce = (explForce / 10) * power;
                                }
                            }
                        }
                    }
                    if (muzzlePrefabs != null && muzzlePrefabs.Length > 0)
                        holdMuzzelFlash = MonoBehaviour.Instantiate(GetMuzzleRandom(), tagMuzzel.position, tagMuzzel.rotation) as GameObject;

                    waitTilNextFire = 0;

                    power = 0f;
                }

                if (fxAudio && bulletComponent && bulletComponent.fire)
                {
                    if (fxAudio.IsPlayingStartWith("Power Charging"))
                        fxAudio.StopStartWith("Power Charging");

                    if (fxAudio.IsPlayingStartWith("Power Stay"))
                        fxAudio.StopStartWith("Power Stay");

                    if (fxAudio.AudioExistsStartWith("Power Shoot"))
                        fxAudio.PlayStartWith("Power Shoot");
                }
                if (waitTilNextFire < timeToFire)
                    waitTilNextFire += Time.deltaTime;

                if (holdMuzzelFlash != null)
                    holdMuzzelFlash.transform.parent = tagMuzzel;

            }
        }
    }
}