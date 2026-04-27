using UnityEngine;
[DisallowMultipleComponent]
public class EnemyShooter : MonoBehaviour
{
    [SerializeField] private EnemyGunSelector _gunSelector;

    public void CallToFire()
    {
        if (_gunSelector._activeGun != null)
        {
            _gunSelector._activeGun.Tick(true);
        }
    }

    public bool ShouldReload()
    {
        return _gunSelector._activeGun.ShouldReload();
    }
}
