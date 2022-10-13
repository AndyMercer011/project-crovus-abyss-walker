using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 投掷物瞄准
    /// </summary>
    public class ThrownWeaponAim3D : WeaponAim3DOverride
    {
        [Header("抛物线绘制")]
        /// 是否使用抛物线
        [Tooltip("是否绘制抛物线")]
        public bool UseParabola;
        /// 抛物线渲染器
        [Tooltip("抛物线渲染器")]
        public LineRenderer lineRenderer;
        /// 抛物线绘制时间步长
        [Tooltip("抛物线绘制时间步长")]
        public float drawTimeStep;
        /// 抛物线绘制最大时间
        [Tooltip("抛物线绘制最大时间")]
        public float drawMaxTime;
        /// 抛物线落点
        [Tooltip("抛物线落点")]
        [MMReadOnly]
        public Vector3 ThrownParabolaDropPoint;
        /// 抛物线落点法线
        [Tooltip("抛物线落点法线")]
        [MMReadOnly]
        public Vector3 ThrownParabolaDropPointNormal;

        [Header("环形范围绘制")]
        /// 是否绘制投掷范围
        [Tooltip("是否绘制投掷范围")]
        public bool UseThrowRangeRing;
        /// 投掷范围预制体
        [Tooltip("投掷范围预制体")]
        public GameObject ThrowRangeRingParticlePrefab;
        /// 投掷范围绘制偏移
        [Tooltip("投掷范围绘制偏移")]
        public Vector3 ThrowRangeRingParticleOffset;
        /// 是否绘制爆炸范围
        [Tooltip("是否绘制爆炸范围")]
        public bool UseExplosionRangeRing;
        /// 爆炸范围预制体
        [Tooltip("爆炸范围预制体")]
        public GameObject ExplosionRangeRingParticlePrefab;
        /// 爆炸范围绘制偏移
        [Tooltip("爆炸范围绘制偏移")]
        public Vector3 ExplosionRangeRingParticleOffset;
        /// 环形旋转速度
        [Tooltip("环形旋转速度")]
        public float RingRotationSpeed;

        protected ThrownWeapon thrownWeapon;
        protected Vector3[] parabolaPoints;
        protected ParticleSystem throwRangeRingParticle;
        protected ParticleSystem explosionRangeRingParticle;
        protected bool spawnRings;
        protected float lastMaxThrownDistance;
        protected float lastExplosionRadius;
        protected float lastRingRotationSpeed;

        protected override void Initialization()
        {
            if (_initialized)
            {
                return;
            }
            base.Initialization();
            _weapon = _weapon.GetComponent<Weapon>();
            thrownWeapon = _weapon as ThrownWeapon;
            SpawnRings();
        }

        protected override void Update()
        {
            base.Update();
            if (GameManager.HasInstance && GameManager.Instance.Paused)
            {
                return;
            }
            parabolaPoints = GetParabola();
            MoveParabola(parabolaPoints);
            SetRings();
            MoveRangeRing();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            DestroyRings();
        }

        protected virtual void SpawnRings()
        {
            if (!spawnRings)
            {
                if (UseThrowRangeRing)
                {
                    throwRangeRingParticle = Instantiate(ThrowRangeRingParticlePrefab, thrownWeapon.Owner.transform).GetComponent<ParticleSystem>();
                }
                if (UseExplosionRangeRing)
                {
                    explosionRangeRingParticle = Instantiate(ExplosionRangeRingParticlePrefab, thrownWeapon.Owner.transform).GetComponent<ParticleSystem>();
                }
                spawnRings = true;
            }
        }

        protected virtual void SetRings()
        {
            if (!spawnRings)
            {
                return;
            }

            if (lastMaxThrownDistance == thrownWeapon.MaxThrownDistance
                && lastExplosionRadius == thrownWeapon.ExplosionRadius
                && lastRingRotationSpeed == RingRotationSpeed)
            {
                return;
            }
            else
            {
                lastMaxThrownDistance = thrownWeapon.MaxThrownDistance;
                lastExplosionRadius = thrownWeapon.ExplosionRadius;
                lastRingRotationSpeed = RingRotationSpeed;
            }

            if (UseThrowRangeRing)
            {
                ParticleSystem.ShapeModule shape = throwRangeRingParticle.shape;
                shape.radius = thrownWeapon.MaxThrownDistance;
                ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = throwRangeRingParticle.velocityOverLifetime;
                velocityOverLifetime.orbitalY = RingRotationSpeed;
            }
            if (UseExplosionRangeRing)
            {
                ParticleSystem.ShapeModule shape = explosionRangeRingParticle.shape;
                shape.radius = thrownWeapon.ExplosionRadius;
                ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = explosionRangeRingParticle.velocityOverLifetime;
                velocityOverLifetime.orbitalY = RingRotationSpeed;
            }
        }

        protected virtual void DestroyRings()
        {
            if (UseThrowRangeRing)
            {
                Destroy(throwRangeRingParticle.gameObject);
            }
            if (UseExplosionRangeRing)
            {
                Destroy(explosionRangeRingParticle.gameObject);
            }
        }

        protected virtual void MoveRangeRing()
        {
            if (!spawnRings)
            {
                return;
            }

            if (UseThrowRangeRing)
            {
                throwRangeRingParticle.transform.position = _weapon.Owner.transform.position + ThrowRangeRingParticleOffset;
                throwRangeRingParticle.transform.rotation = _weapon.Owner.transform.rotation;
            }
            if (UseExplosionRangeRing)
            {
                explosionRangeRingParticle.transform.rotation =
                    Quaternion.LookRotation(ThrownParabolaDropPointNormal) * Quaternion.Euler(90, 0, 0);
                explosionRangeRingParticle.transform.position =
                    ThrownParabolaDropPoint + explosionRangeRingParticle.transform.rotation * ExplosionRangeRingParticleOffset;
            }
        }

        protected override void GetWeaponAimReticlePoint()
        {
            _weaponAimReticle.SetActive(_weaponAimReticleEnable);
            if (UseParabola)
            {
                lineRenderer.gameObject.SetActive(_weaponAimReticleEnable);
            }
            else
            {
                lineRenderer.gameObject.SetActive(false);
            }
            if (UseThrowRangeRing)
            {
                throwRangeRingParticle.gameObject.SetActive(_weaponAimReticleEnable);
            }
            if (UseExplosionRangeRing)
            {
                explosionRangeRingParticle.gameObject.SetActive(_weaponAimReticleEnable);
            }
            if (!UseWeaponAimReticle || !_weaponAimReticleUsing)
            {
                DisableWeaponAimReticle();
                return;
            }
            else
            {
                EnableWeaponAimReticle();
                _weaponAimReticlePoint = ThrownParabolaDropPoint;
            }
        }

        protected virtual Vector3[] GetParabola()
        {
            if (thrownWeapon.MaxThrownDistance == 0)
            {
                return null;
            }

            Vector3 thrownPoint;
            if (_weaponAimCurrentAim.magnitude > thrownWeapon.MaxThrownDistance)
            {
                thrownPoint = Vector3.ClampMagnitude(_weaponAimCurrentAim, thrownWeapon.MaxThrownDistance);
                thrownPoint += transform.position;
                thrownPoint.y = thrownWeapon.Owner.transform.position.y;
            }
            else
            {
                thrownPoint = _weaponAimCurrentAim;
                thrownPoint += transform.position;
            }

            Vector3[] thrownPath;
            if (thrownWeapon.Throwing)
            {
                thrownWeapon.ThrownVelocity = GetLaunchVelocity(thrownWeapon.WeaponUseTransform.position, thrownPoint, thrownWeapon.ThrowSpeed, Physics.gravity, thrownWeapon.ParabolaMode);
                thrownPath = GetParabolaPoints(thrownWeapon.WeaponUseTransform.position, thrownWeapon.ThrownVelocity, Physics.gravity, drawTimeStep,
                    drawMaxTime, ReticleObstacleMask.value, out ThrownParabolaDropPoint, out ThrownParabolaDropPointNormal);
            }
            else
            {
                thrownWeapon.ThrownVelocity = thrownWeapon.WeaponUseTransform.forward * thrownWeapon.DefaultThrownSpeed;
                thrownPath = GetParabolaPoints(thrownWeapon.WeaponUseTransform.position, thrownWeapon.ThrownVelocity, Physics.gravity, drawTimeStep,
                    drawMaxTime, ReticleObstacleMask.value, out ThrownParabolaDropPoint, out ThrownParabolaDropPointNormal);
            }

            for (int i = 0; i < thrownPath.Length; i++)
            {
                thrownPath[i] = lineRenderer.transform.InverseTransformPoint(thrownPath[i]);
            }

            return thrownPath;
        }

        protected virtual void MoveParabola(Vector3[] thrownPath)
        {
            if (thrownWeapon.Throwing)
            {
                if (thrownPath != null)
                {
                    _weaponAimReticleUsing = true;
                    lineRenderer.positionCount = thrownPath.Length;
                    lineRenderer.SetPositions(thrownPath);
                }
                else
                {
                    _weaponAimReticleUsing = false;
                }
            }
        }

        protected static Vector3 GetLaunchVelocity(Vector3 start, Vector3 end, float speed, Vector3 gravity, ThrownWeapon.ThrownParabolaMode thrownMode)
        {
            Vector3 toTarget = end - start;
            Vector3 launchVelocity;
            float gSquared = gravity.sqrMagnitude;
            float b = speed * speed + Vector3.Dot(toTarget, gravity);
            float discriminant = b * b - gSquared * toTarget.sqrMagnitude;
            if (discriminant >= 0)
            {
                float discRoot = Mathf.Sqrt(discriminant);
                float time = 0;
                switch (thrownMode)
                {
                    case ThrownWeapon.ThrownParabolaMode.LowEnergy:
                        time = Mathf.Sqrt(Mathf.Sqrt(toTarget.sqrMagnitude * 4 / gSquared));
                        break;
                    case ThrownWeapon.ThrownParabolaMode.Max:
                        time = Mathf.Sqrt((b + discRoot) * 2 / gSquared);
                        break;
                    case ThrownWeapon.ThrownParabolaMode.Min:
                        time = Mathf.Sqrt((b - discRoot) * 2 / gSquared);
                        break;
                    default:
                        break;
                }
                launchVelocity = toTarget / time - gravity * time / 2;
                return launchVelocity;
            }
            else
            {
                return Vector3.zero;
            }
        }

        protected static Vector3[] GetParabolaPoints(Vector3 start, Vector3 startVelocity, Vector3 gravity, float timestep,
            float maxTime, int obstacleLayer, out Vector3 dropPoint, out Vector3 dropPointNormal)
        {
            dropPoint = Vector3.zero;
            dropPointNormal = Vector3.zero;
            if (timestep == 0 || maxTime == 0 || startVelocity == Vector3.zero)
            {
                return null;
            }
            List<Vector3> thrownPath = new List<Vector3>();
            Vector3 prev = start;
            thrownPath.Add(start);
            bool isCast = false;
            for (int i = 1; ; i++)
            {
                float t = timestep * i;
                if (t > maxTime || isCast) break;
                Vector3 pos = start + startVelocity * t + gravity * t * t * 0.5f;
                if (Physics.Linecast(prev, pos, out RaycastHit raycastHit, obstacleLayer))
                {
                    dropPoint = raycastHit.point;
                    dropPointNormal = raycastHit.normal;
                    isCast = true;
                }
                Debug.DrawLine(prev, pos, Color.red);
                prev = pos;
                thrownPath.Add(pos);
            }
            return thrownPath.ToArray();
        }
    }
}
