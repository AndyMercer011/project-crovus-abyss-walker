using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// A weapon class aimed specifically at allowing the creation of various projectile weapons, from shotgun to machine gun, via plasma gun or rocket launcher
    /// </summary>
    public class ProjectileWeaponOverride : WeaponOverride, MMEventListener<TopDownEngineEvent>
    {
        [MMInspectorGroup("Projectiles", true, 22)]
        /// the number of projectiles to spawn per shot
        [Tooltip("the number of projectiles to spawn per shot")]
        public int ProjectilesPerShot = 1;

        [Header("Spawn Transforms")]
        /// a list of transforms that can be used a spawn points, instead of the ProjectileSpawnOffset. Will be ignored if left emtpy 
        [Tooltip("a list of transforms that can be used a spawn points, instead of the ProjectileSpawnOffset. Will be ignored if left emtpy")]
        public List<Transform> SpawnTransforms = new List<Transform>();
        /// the selected mode for spawn transforms. Sequential will go through the list sequentially, while Random will pick a random one every shot
        [Tooltip("the selected mode for spawn transforms. Sequential will go through the list sequentially, while Random will pick a random one every shot")]
        public ProjectileWeapon.SpawnTransformsModes SpawnTransformsMode = ProjectileWeapon.SpawnTransformsModes.Sequential;

        [Header("Spread")]
        /// whether or not the spread should be random 
        [Tooltip("whether or not the spread should be random ")]
        public bool RandomSpread = true;
        /// the spread (in degrees) to apply randomly (or not) on each angle when spawning a projectile
        [Tooltip("the spread (in degrees) to apply randomly (or not) on each angle when spawning a projectile")]
        public Vector3 Spread = Vector3.zero;
        /// 瞄准时的扩散角度
        [Tooltip("瞄准时的扩散角度")]
        public Vector3 AimSpread = Vector3.zero;
        /// 是否启用多子弹的叠加扩散角度
        [Tooltip("是否启用多子弹的叠加扩散角度 ")]
        public bool MultiProjectileRandomSpread;
        /// 瞄准时的扩散角度
        [Tooltip("多子弹的叠加扩散角度")]
        public Vector3 MultiProjectileSpread = Vector3.zero;
        /// whether or not the weapon should rotate to align with the spread angle
        [Tooltip("whether or not the weapon should rotate to align with the spread angle")]
        public bool RotateWeaponOnSpread = false;
        /// the projectile's spawn position
        [MMReadOnly]
        [Tooltip("the projectile's spawn position")]
        public Vector3 SpawnPosition = Vector3.zero;
        /// the object pooler used to spawn projectiles
        public MMObjectPooler ObjectPooler { get; set; }

        [Header("Spawn Feedbacks")]
        public List<MMFeedbacks> SpawnFeedbacks = new List<MMFeedbacks>();

        [MMInspectorButton("TestShoot")]
        /// a button to test the shoot method
        public bool TestShootButton;

        [MMInspectorGroup("Recoil", true)]
        /// <summary>
        /// 是否启用武器后坐力旋转
        /// </summary>
        [Tooltip("是否启用武器后坐力旋转")]
        public bool UseWeapomRecoilRotation;
        /// <summary>
        /// 武器后坐力角度
        /// </summary>
        [Tooltip("武器后坐力角度")]
        public Vector2 WeapomRecoilAngle;
        /// <summary>
        /// 武器后坐力随机角度
        /// </summary>
        [Tooltip("武器后坐力随机角度")]
        public Vector2 WeapomRecoilRandomRange;
        /// <summary>
        /// 武器后座角度回复速度
        /// </summary>
        [Tooltip("武器后座角度回复速度")]
        public Vector2 WeapomRecoilRevertSpeed;
        /// <summary>
        /// 武器回正速度根据武器准星和瞄准点的偏移的倍率曲线
        /// </summary>
        [Tooltip("武器回正速度根据武器准星和瞄准点的偏移的倍率曲线")]
        public AnimationCurve MultiplyCurve;
        /// <summary>
        /// 武器回正最大时间，超过这个时间后重置后坐力旋转
        /// </summary>
        [Tooltip("武器回正最大时间，超过这个时间后重置后坐力旋转")]
        public float WeapomRecoilRevertTime;

        protected Vector3 _randomSpreadDirection;
        protected Vector3 _multiProjectileRandomSpreadDirection;
        protected bool _poolInitialized = false;
        protected Transform _projectileSpawnTransform;
        protected int _spawnArrayIndex = 0;

        /// <summary>
        /// A test method that triggers the weapon
        /// </summary>
        protected virtual void TestShoot()
        {
            if (WeaponState.CurrentState == WeaponStates.WeaponIdle)
            {
                WeaponInputStart();
            }
            else
            {
                WeaponInputStop();
            }
        }

        /// <summary>
        /// Initialize this weapon
        /// </summary>
        public override void Initialization()
        {
            base.Initialization();
            _weaponAim = GetComponent<WeaponAim>();

            if (!_poolInitialized)
            {
                if (GetComponent<MMMultipleObjectPooler>() != null)
                {
                    ObjectPooler = GetComponent<MMMultipleObjectPooler>();
                }
                if (GetComponent<MMSimpleObjectPooler>() != null)
                {
                    ObjectPooler = GetComponent<MMSimpleObjectPooler>();
                }
                if (ObjectPooler == null)
                {
                    Debug.LogWarning(this.name + " : no object pooler (simple or multiple) is attached to this Projectile Weapon, it won't be able to shoot anything.");
                    return;
                }
                _poolInitialized = true;
            }
        }

        /// <summary>
        /// Called everytime the weapon is used
        /// </summary>
        public override void WeaponUse()
        {
            base.WeaponUse();

            DetermineSpawnPosition();
            _randomSpreadDirection = Vector3.zero;
            _multiProjectileRandomSpreadDirection = Vector3.zero;

            for (int i = 0; i < ProjectilesPerShot; i++)
            {
                SpawnProjectile(SpawnPosition, i, ProjectilesPerShot, true);
                PlaySpawnFeedbacks();
            }
        }

        /// <summary>
        /// Spawns a new object and positions/resizes it
        /// </summary>
        protected virtual GameObject SpawnProjectile(Vector3 spawnPosition, int projectileIndex, int totalProjectiles, bool triggerObjectActivation = true)
        {
            /// we get the next object in the pool and make sure it's not null
            GameObject nextGameObject = ObjectPooler.GetPooledGameObject();

            // mandatory checks
            if (nextGameObject == null) { return null; }
            if (nextGameObject.GetComponent<MMPoolableObject>() == null)
            {
                throw new Exception(gameObject.name + " is trying to spawn objects that don't have a PoolableObject component.");
            }
            // we position the object
            nextGameObject.transform.position = spawnPosition;
            if (_projectileSpawnTransform != null)
            {
                nextGameObject.transform.position = _projectileSpawnTransform.position;
            }
            // we set its direction

            Projectile projectile = nextGameObject.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.SetWeapon(this);
                if (Owner != null)
                {
                    projectile.SetOwner(Owner.gameObject);
                }
            }
            // we activate the object
            nextGameObject.gameObject.SetActive(true);

            if (projectile != null)
            {
                if (RandomSpread)
                {
                    if (projectileIndex == 0)
                    {
                        if (!Aiming)
                        {
                            _randomSpreadDirection.x = UnityEngine.Random.Range(-Spread.x, Spread.x);
                            _randomSpreadDirection.y = UnityEngine.Random.Range(-Spread.y, Spread.y);
                            _randomSpreadDirection.z = UnityEngine.Random.Range(-Spread.z, Spread.z);
                        }
                        else
                        {
                            _randomSpreadDirection.x = UnityEngine.Random.Range(-AimSpread.x, AimSpread.x);
                            _randomSpreadDirection.y = UnityEngine.Random.Range(-AimSpread.y, AimSpread.y);
                            _randomSpreadDirection.z = UnityEngine.Random.Range(-AimSpread.z, AimSpread.z);
                        }
                    }


                }

                if (totalProjectiles > 1)
                {
                    if (MultiProjectileRandomSpread)
                    {
                        _multiProjectileRandomSpreadDirection.x = UnityEngine.Random.Range(-MultiProjectileSpread.x, MultiProjectileSpread.x);
                        _multiProjectileRandomSpreadDirection.y = UnityEngine.Random.Range(-MultiProjectileSpread.y, MultiProjectileSpread.y);
                        _multiProjectileRandomSpreadDirection.z = UnityEngine.Random.Range(-MultiProjectileSpread.z, MultiProjectileSpread.z);
                    }
                }
                Quaternion spread;
                if (totalProjectiles > 1)
                {
                    spread = Quaternion.Euler(_randomSpreadDirection + _multiProjectileRandomSpreadDirection);
                }
                else
                {
                    spread = Quaternion.Euler(_randomSpreadDirection);
                }

                if (Owner == null)
                {
                    projectile.SetDirection(spread * WeaponUseTransform.forward, WeaponUseTransform.rotation, true);
                }
                else
                {
                    if (Owner.CharacterDimension == Character.CharacterDimensions.Type3D)
                    {
                        projectile.SetDirection(spread * WeaponUseTransform.forward, WeaponUseTransform.rotation, true);
                    }
                }

                if (RotateWeaponOnSpread && !Aiming)
                {
                    this.transform.rotation = this.transform.rotation * spread;
                }
            }

            if (triggerObjectActivation)
            {
                if (nextGameObject.GetComponent<MMPoolableObject>() != null)
                {
                    nextGameObject.GetComponent<MMPoolableObject>().TriggerOnSpawnComplete();
                }
            }
            return (nextGameObject);
        }

        /// <summary>
        /// This method is in charge of playing feedbacks on projectile spawn
        /// </summary>
        protected virtual void PlaySpawnFeedbacks()
        {
            if (SpawnFeedbacks.Count > 0)
            {
                SpawnFeedbacks[_spawnArrayIndex]?.PlayFeedbacks();
            }

            _spawnArrayIndex++;
            if (_spawnArrayIndex >= SpawnTransforms.Count)
            {
                _spawnArrayIndex = 0;
            }
        }

        /// <summary>
        /// Sets a forced projectile spawn position
        /// </summary>
        /// <param name="newSpawnTransform"></param>
        public virtual void SetProjectileSpawnTransform(Transform newSpawnTransform)
        {
            _projectileSpawnTransform = newSpawnTransform;
        }

        /// <summary>
        /// Determines the spawn position based on the spawn offset and whether or not the weapon is flipped
        /// </summary>
        public virtual void DetermineSpawnPosition()
        {
            if (WeaponUseTransform != null)
            {
                SpawnPosition = WeaponUseTransform.position;
            }
            else
            {
                SpawnPosition = transform.position;
            }

            if (SpawnTransforms.Count > 0)
            {
                if (SpawnTransformsMode == ProjectileWeapon.SpawnTransformsModes.Random)
                {
                    _spawnArrayIndex = UnityEngine.Random.Range(0, SpawnTransforms.Count);
                    SpawnPosition = SpawnTransforms[_spawnArrayIndex].position;
                }
                else
                {
                    SpawnPosition = SpawnTransforms[_spawnArrayIndex].position;
                }
            }
        }

        /// <summary>
        /// When the weapon is selected, draws a circle at the spawn's position
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
        {
            DetermineSpawnPosition();

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(SpawnPosition, 0.2f);
        }

        public void OnMMEvent(TopDownEngineEvent engineEvent)
        {
            switch (engineEvent.EventType)
            {
                case TopDownEngineEventTypes.LevelStart:
                    _poolInitialized = false;
                    Initialization();
                    break;
            }
        }

        /// <summary>
        /// On enable we start listening for events
        /// </summary>
        protected virtual void OnEnable()
        {
            this.MMEventStartListening<TopDownEngineEvent>();
        }

        /// <summary>
        /// On disable we stop listening for events
        /// </summary>
        protected virtual void OnDisable()
        {
            this.MMEventStopListening<TopDownEngineEvent>();
        }

        protected override void ApplyRecoil()
        {
            if (!UseWeapomRecoilRotation || !Aiming)
            {
                return;
            }
            base.ApplyRecoil();
            float randomAngleX = WeapomRecoilAngle.x + UnityEngine.Random.Range(-WeapomRecoilRandomRange.x, WeapomRecoilRandomRange.x);
            float randomAngleY = WeapomRecoilAngle.y + UnityEngine.Random.Range(-WeapomRecoilRandomRange.y, WeapomRecoilRandomRange.y);

            WeaponAim3DOverride.AddRecoilRotation(new Vector2(randomAngleX, randomAngleY), WeapomRecoilRevertSpeed, MultiplyCurve, WeapomRecoilRevertTime);
        }
    }
}
