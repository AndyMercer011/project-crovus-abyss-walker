using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MoreMountains.TopDownEngine.ThrownWeaponAim3D;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 投掷武器
    /// </summary>
    public class ThrownWeapon : WeaponOverride, MMEventListener<TopDownEngineEvent>
    {
        /// <summary>
        /// 投掷抛物线模式
        /// </summary>
        public enum ThrownParabolaMode
        {
            /// <summary>
            /// 计算最省力的抛物线
            /// </summary>
            LowEnergy,
            /// <summary>
            /// 计算指定速度高度较高的抛物线
            /// </summary>
            Max,
            /// <summary>
            /// 计算指定速度高度较低的抛物线
            /// </summary>
            Min
        }

        [MMInspectorGroup("Projectiles", true, 22)]

        /// 爆炸延迟时间
        [Tooltip("爆炸延迟时间")]
        public float ExplosionDelayTime;
        /// 爆炸半径
        [Tooltip("爆炸半径")]
        public float ExplosionRadius;
        /// 投掷物阻力
        [Tooltip("投掷物阻力")]
        public float ThrownResistance;
        /// 快速投掷物阻力
        [Tooltip("快速投掷物阻力")]
        public float FastThrownResistance;

        [Header("投掷设置")]

        /// 抛物线模式
        [Tooltip("抛物线模式")]
        public ThrownParabolaMode ParabolaMode;
        /// 投掷力模式
        [Tooltip("投掷力模式")]
        public ForceMode ThrownForceMode;

        /// 默认投掷速度
        [Tooltip("默认投掷速度")]
        public float DefaultThrownSpeed = 10f;
        /// 投掷速度
        [Tooltip("投掷速度")]
        public float ThrowSpeed;
        /// 最大投掷距离
        [Tooltip("最大投掷距离")]
        public float MaxThrownDistance;
        /// 快速投掷双击时间限制
        [Tooltip("快速投掷双击时间限制")]
        public float FastThrowDoubleClickTimeLimit = 0.25f;
        [MMReadOnly]
        /// 投掷速度向量
        [Tooltip("投掷速度向量")]
        public Vector3 ThrownVelocity;
        [MMReadOnly]
        /// 是否进入投掷模式
        [Tooltip("是否进入投掷模式")]
        public bool Throwing;

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

        /// the projectile's spawn position
        [MMReadOnly]
        [Tooltip("the projectile's spawn position")]
        public Vector3 SpawnPosition = Vector3.zero;
        /// the object pooler used to spawn projectiles
        public MMObjectPooler ObjectPooler { get; set; }

        [Header("Spawn Feedback")]
        /// 生成投掷物的Feedback
        [Tooltip("生成投掷物的Feedback")]
        public MMFeedbacks SpawnFeedback;

        [MMInspectorButton("TestThrown")]
        /// a button to test the shoot method
        public bool TestThrownButton;

        protected ThrownWeaponAim3D thrownWeaponAim3D;
        protected Vector3 _randomSpreadDirection;
        protected bool _poolInitialized = false;
        protected Coroutine inputCoroutine;
        protected bool throwInput;
        protected bool fastThrow;

        /// <summary>
        /// A test method that triggers the weapon
        /// </summary>
        protected virtual void TestThrown()
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
            thrownWeaponAim3D = GetComponent<ThrownWeaponAim3D>();
            if (inputCoroutine == null)
            {
                inputCoroutine = StartCoroutine(InputListener());
            }

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

        protected virtual IEnumerator InputListener()
        {
            while (enabled)
            {
                if (throwInput)
                {
                    throwInput = false;
                    yield return InputEvent();
                }

                yield return null;
            }
        }

        protected virtual IEnumerator InputEvent()
        {
            yield return new WaitForEndOfFrame();

            float count = 0f;
            while (count < FastThrowDoubleClickTimeLimit)
            {
                if (throwInput)
                {
                    throwInput = false;
                    DoubleInput();
                    yield break;
                }
                count += Time.deltaTime;
                yield return null;
            }
            SingleInput();
        }

        protected virtual void SingleInput()
        {
            if (!Throwing)
            {
                ThrowStart();
            }
            else
            {
                ThrowStop();
            }
        }

        protected virtual void DoubleInput()
        {
            if (!Throwing)
            {
                fastThrow = true;
            }
            base.WeaponInputStart();
        }

        protected override void LateUpdate()
        {
            ProcessWeaponState();
            if (WeaponState.CurrentState != (WeaponStates.WeaponStart | WeaponStates.WeaponUse))
            {
                fastThrow = false;
            }
        }


        public override void AimInputStart()
        {
            if (Throwing)
            {
                return;
            }
            base.AimInputStart();
        }

        public override void AimInputStop()
        {
            if (Throwing)
            {
                return;
            }
            base.AimInputStop();
        }

        public override void WeaponInputStart()
        {
            if (!Throwing)
            {
                return;
            }
            base.WeaponInputStart();
        }

        /// <summary>
        /// Called everytime the weapon is used
        /// </summary>
        public override void WeaponUse()
        {
            base.WeaponUse();

            DetermineSpawnPosition();
            _randomSpreadDirection = Vector3.zero;
            SpawnProjectile(SpawnPosition, true);
            PlaySpawnFeedbacks();
        }

        /// <summary>
        /// Spawns a new object and positions/resizes it
        /// </summary>
        protected virtual GameObject SpawnProjectile(Vector3 spawnPosition, bool triggerObjectActivation = true)
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
            // we set its direction

            ThrownObject3D ThrownObject = nextGameObject.GetComponent<ThrownObject3D>();
            if (ThrownObject != null)
            {
                ThrownObject.SetWeapon(this);
                if (Owner != null)
                {
                    ThrownObject.SetOwner(Owner.gameObject);
                }
            }
            // we activate the object
            nextGameObject.gameObject.SetActive(true);

            if (ThrownObject != null)
            {
                if (RandomSpread)
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
                Quaternion spread;
                spread = Quaternion.Euler(_randomSpreadDirection);

                if (Owner == null || !Aiming)
                {
                    ThrownObject.SetDirection(spread * WeaponUseTransform.forward, WeaponUseTransform.rotation, true);
                    ThrownObject.SetSpeed(DefaultThrownSpeed, ThrownForceMode);
                }
                else
                {
                    if (Owner.CharacterDimension == Character.CharacterDimensions.Type3D)
                    {
                        ThrownObject.SetDirection(spread * ThrownVelocity.normalized, WeaponUseTransform.rotation, true);
                        ThrownObject.SetSpeed(ThrownVelocity.magnitude, ThrownForceMode);
                    }
                }
                ThrownObject.SetThrown(ExplosionDelayTime, ExplosionRadius, fastThrow ? FastThrownResistance : ThrownResistance);
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
            SpawnFeedback?.PlayFeedbacks();
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
        }

        /// <summary>
        /// 投掷输入
        /// </summary>
        public virtual void ThrowInput()
        {
            throwInput = true;
        }

        /// <summary>
        /// 进入投掷模式
        /// </summary>
        public virtual void ThrowStart()
        {
            if (!Throwing)
            {
                Throwing = true;
                base.AimInputStart();
                WeaponAim3DOverride._weaponAimReticleUsing = true;
            }
        }

        /// <summary>
        /// 退出投掷模式
        /// </summary>
        public virtual void ThrowStop()
        {
            if (Throwing)
            {
                Throwing = false;
                WeaponAim3DOverride._weaponAimReticleUsing = false;
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
    }
}
