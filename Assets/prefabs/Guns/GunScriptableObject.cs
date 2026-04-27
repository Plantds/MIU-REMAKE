using System;
using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(fileName = "Gun", menuName = "Guns/Gun", order = 0)]
public class GunScriptableObject : ScriptableObject
{
    [Header("GunSettings")]
    public GunType _type;
    public ImpactType _impactType;
    public string _Name;
    public GameObject _modelPrefab;
    public Vector3 _spawnPoint;
    public Vector3 _spawnRotation;
    public int _ammo = 30;
    private int _currAmmo;

    [Header("External Scripts")]
    public ShootConfigurationScriptableObject _shootConfig;
    public TrailConfigScritableObject _trailConfig;

    private MonoBehaviour _activeMonoBehaviour;
    private GameObject _model;
    
    private float _lastShootTime;
    private float _initialStartTime;
    private float _stopShootingTime;
    private bool _lastFrameWantedToShoot;

    private ParticleSystem _shootSystem;
    private ObjectPool<TrailRenderer> _trailPool;

    public void Spawn(Transform parent, MonoBehaviour activeMonoBehavior)
    {
        this._activeMonoBehaviour = activeMonoBehavior;
        _lastShootTime = 0.0f; // in editor this will not be properly reset, in build its fine. UNITY WHY
        _trailPool = new ObjectPool<TrailRenderer>(CreateTrail);

        _model = Instantiate(_modelPrefab);
        _model.transform.SetParent(parent, false);
        _model.transform.localPosition = _spawnPoint;
        _model.transform.localRotation = Quaternion.Euler(_spawnRotation);

        _shootSystem = _model.GetComponentInChildren<ParticleSystem>();

        _currAmmo = _ammo;
    }

    public void Shoot()
    {
        if (ShouldReload()) { Reload(); return; }

        if (Time.time - _lastShootTime - _shootConfig._fireRate > Time.deltaTime)
        {
            float lastDur = Mathf.Clamp(
                0,
                (_stopShootingTime - _initialStartTime),
                _shootConfig._maxSpreadTime
            );
            float lerpTime = (_shootConfig._recoilRecoveryRate - (Time.time - _stopShootingTime)) / _shootConfig._recoilRecoveryRate;

            _initialStartTime = Time.time - Mathf.Lerp(0, lastDur, Mathf.Clamp01(lerpTime));
        }

        if (Time.time > _shootConfig._fireRate + _lastShootTime)
        {
            _lastShootTime = Time.time;
            _shootSystem.Play();

            Vector3 spreadAmount = _shootConfig.GetSpread(Time.time - _initialStartTime);
            _model.transform.forward += _model.transform.TransformDirection(spreadAmount);

            Vector3 shootDir = _model.transform.parent.forward;

            if (Physics.Raycast(_shootSystem.transform.position, shootDir, out RaycastHit hit, float.MaxValue, _shootConfig._hitMask))
            {
                _activeMonoBehaviour.StartCoroutine(
                    PlayTrail(_shootSystem.transform.position, hit.point, hit)
                );
            }
            else
            {
                _activeMonoBehaviour.StartCoroutine(
                    PlayTrail(_shootSystem.transform.position, _shootSystem.transform.position + (shootDir * _trailConfig._missDistance), new RaycastHit())
                );
            }

            _currAmmo--;
        }
    }

    public bool ShouldReload()
    {
        return _currAmmo <= 0;
    }

    private void Reload()
    {
        _currAmmo = _ammo;
    }

    public void Tick(bool WantsToShooot)
    {
        _model.transform.localRotation = Quaternion.Lerp(
            _model.transform.localRotation,
            Quaternion.Euler(_spawnRotation),
            Time.deltaTime * _shootConfig._recoilRecoveryRate
        );

        if (WantsToShooot) { _lastFrameWantedToShoot = true; Shoot(); }
        else if (!WantsToShooot && _lastFrameWantedToShoot) { _stopShootingTime = Time.time; _lastFrameWantedToShoot = false; }
    }

    private IEnumerator PlayTrail(Vector3 startPoint, Vector3 endPoint, RaycastHit hit)
    {
        TrailRenderer instance = _trailPool.Get();
        instance.gameObject.SetActive(true);
        instance.transform.position = startPoint;
        yield return null; // avoid

        instance.emitting = true;
        float distance = Vector3.Distance(startPoint, endPoint);
        float remainingDistance = distance;
        while (remainingDistance > 0)
        {
            instance.transform.position = Vector3.Lerp(
                a: startPoint,
                b: endPoint,
                t: Mathf.Clamp01(1 - (remainingDistance / distance))); // Mathf.Clamp01(1 - (remainingDistance / distance)) / or / Mathf.Clamp01(1.0f - Mathf.Exp(-(remainingDistance / distance) * Time.deltaTime))
            remainingDistance -= _trailConfig._simulationSpeed * Time.deltaTime;

            yield return null;
        }

        instance.transform.position = endPoint;

        

        if (hit.collider != null)
        {
            SurfaceManger.Instance.HandleImpact(hit.transform.gameObject, endPoint, hit.normal, _impactType, 0);
        }

        yield return new WaitForSeconds(_trailConfig._duration);
        yield return null;
        instance.emitting = false;
        instance.gameObject.SetActive(false);
        _trailPool.Release(instance);
    }

    private TrailRenderer CreateTrail()
    {
        GameObject instance = new GameObject("Bullet Trail");
        TrailRenderer trail = instance.AddComponent<TrailRenderer>();
        trail.colorGradient = _trailConfig._color;
        trail.material = _trailConfig._material;
        trail.widthCurve = _trailConfig._widthCurve;
        trail.time = _trailConfig._duration;
        trail.minVertexDistance = _trailConfig._minVertexDistance;

        trail.emitting = _trailConfig._emitting;
        trail.shadowCastingMode = _trailConfig._shadow ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;

        return trail;
    }
}
