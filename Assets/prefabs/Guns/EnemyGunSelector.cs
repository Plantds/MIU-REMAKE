using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemyGunSelector : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private GunType _gunType;
    [SerializeField] private Transform _gunParent;
    [SerializeField] private List<GunScriptableObject> _guns;
    // [SerializeField] private enemyIK _inverseKinematics;

    [Header("Runtime Filled")]
    public GunScriptableObject _activeGun;

    private void Start()
    {
        GunScriptableObject gun = _guns.Find(gun => gun._type == _gunType);

        if (gun == null)
        {
            Debug.LogError($"ERROR::NO_GUNSCRIPTABLEOBJECT_FOUND_FOR_GUNTYPE::{gun}");
            return;
        }

        _activeGun = gun;
        gun.Spawn(_gunParent, this);

        //IK STUFF HERE
    }
}
