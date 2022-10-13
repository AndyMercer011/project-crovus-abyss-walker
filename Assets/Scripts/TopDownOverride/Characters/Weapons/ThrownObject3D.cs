using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// A thrown object type of projectile, useful for grenades and such
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class ThrownObject3D : Projectile
    {
        /// <summary>
        /// 投掷物模型
        /// </summary>
        [Tooltip("投掷物模型")]
        [SerializeField] private GameObject Model;
        /// <summary>
        /// 投掷物碰撞体
        /// </summary>
        [Tooltip("投掷物碰撞体")]
        [SerializeField] private Collider Collider;
        /// <summary>
        /// 是否已经爆炸
        /// </summary>
        [Tooltip("是否已经爆炸")]
        [MMReadOnly]
        public bool Exploded;

        /// 是否绘制爆炸范围
        [Tooltip("是否绘制爆炸范围")]
        public bool UseExplosionRangeRing;

        /// 爆炸范围绘制
        [Tooltip("爆炸范围绘制")]
        public ParticleSystem explosionRangeRingParticle;

        /// 爆炸范围绘制偏移
        [Tooltip("爆炸范围绘制偏移")]
        public Vector3 ExplosionRangeRingParticleOffset;

        /// 环形旋转速度
        [Tooltip("环形旋转速度")]
        public float RingRotationSpeed;

        protected Vector3 _throwingForce;
        protected bool _forceApplied = false;
        protected ForceMode _forceMode;
        protected float explosionDelayTimeCounter;
        protected float explosionRadius;

        protected ExplosionDamage explosionDamage;

        public ExplosionDamage TargetExplosionDamage { get => explosionDamage; protected set => explosionDamage = value; }

        /// <summary>
        /// On init, we grab our rigidbody
        /// </summary>
        protected override void Initialization()
        {
            base.Initialization();
            if (_collider != null)
            {
                _collider.enabled = false;
            }
            if (_collider2D != null)
            {
                _collider2D.enabled = false;
            }
            explosionDamage = GetComponent<ExplosionDamage>();
            _rigidBody = this.GetComponent<Rigidbody>();
        }

        /// <summary>
        /// On enable, we reset the object's speed
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            _forceApplied = false;
            Exploded = false;
            Model.SetActive(true);
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (explosionDelayTimeCounter > 0)
            {
                explosionDelayTimeCounter -= Time.deltaTime;
            }
            else
            {
                if (!Exploded)
                {
                    StartExplosion();
                }
            }
        }

        protected override void Update()
        {
            base.Update();
            MoveRangeRing();
        }

        protected virtual void StartRing()
        {
            if (UseExplosionRangeRing)
            {
                explosionRangeRingParticle.gameObject.SetActive(true);
            }
            else
            {
                explosionRangeRingParticle.Clear();
                explosionRangeRingParticle.gameObject.SetActive(false);
            }
        }

        protected virtual void SetRings()
        {
            if (UseExplosionRangeRing)
            {
                ParticleSystem.ShapeModule shape = explosionRangeRingParticle.shape;
                shape.radius = explosionRadius;

                ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = explosionRangeRingParticle.velocityOverLifetime;
                velocityOverLifetime.orbitalY = RingRotationSpeed;
            }
        }

        protected virtual void MoveRangeRing()
        {
            if (UseExplosionRangeRing)
            {
                explosionRangeRingParticle.transform.rotation = Quaternion.LookRotation(Vector3.forward);
                explosionRangeRingParticle.transform.position = transform.position + ExplosionRangeRingParticleOffset;
            }
        }

        /// <summary>
        /// 设置投掷速度
        /// </summary>
        /// <param name="speed">速度</param>
        /// <param name="forceMode">投掷力模式</param>
        public virtual void SetSpeed(float speed, ForceMode forceMode)
        {
            _forceMode = forceMode;
            _initialSpeed = speed;
            Speed = speed;
        }

        /// <summary>
        /// 设置投掷物
        /// </summary>
        /// <param name="explosionDelayTime">爆炸延迟</param>
        /// <param name="explosionRadius">爆炸半径</param>
        public virtual void SetThrown(float explosionDelayTime, float explosionRadius, float resistance)
        {
            explosionDelayTimeCounter = explosionDelayTime;
            this.explosionRadius = explosionRadius;
            if (Collider.material == null)
            {
                Collider.material = new PhysicMaterial();
            }
            Collider.material.dynamicFriction = resistance;
            Collider.material.staticFriction = resistance;
            SetRings();
            StartRing();
        }

        /// <summary>
        /// Handles the projectile's movement, every frame
        /// </summary>
        public override void Movement()
        {
            if (!_forceApplied && (Direction != Vector3.zero))
            {
                _rigidBody.velocity = Vector3.zero;
                _throwingForce = Direction * Speed;
                _rigidBody.AddForce(_throwingForce, _forceMode);
                _forceApplied = true;
            }
        }

        public override void StopAt()
        {
            if (!Exploded)
            {
                StartExplosion();
            }
            _shouldMove = false;
        }

        protected void StartExplosion()
        {
            Exploded = true;
            explosionDamage.Explosion(explosionRadius);
            Model.SetActive(false);
            explosionRangeRingParticle.gameObject.SetActive(false);
        }
    }
}
