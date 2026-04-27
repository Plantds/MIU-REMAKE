using UnityEngine;
using KinematicCharacterController;
using SimpleMan.VisualRaycast;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
public struct weaponInput
{
    public bool Cut;
    public bool Parry;
    public bool Sheath;
};

public class cutAndParry : MonoBehaviour
{
    [Header("Miscellaneous")]
    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private CinemachineCamera cam;
    [SerializeField] private GameObject attackBoxCenter;
    [SerializeField] private LayerMask enemy;

    [Header("Cut")]
    [SerializeField] private Vector3 cutBoxSize;
    [SerializeField] private float timeBetweenCuts = 0.3f;

    [Header("Parry")]
    [SerializeField] private Vector3 parryBoxSize;
    [SerializeField] private float parrySpeed = 0.3f;

    [Header("Sheath")]
    [SerializeField] private float sheathSpeed = 1.5f;

    private bool _requestedCut;
    private bool _requestedParry;
    private bool _requestedSheath;
    private bool _storedSheath;

    private float _betweenCutsTimer;
    private float _sheathTimer;

    private List<RaycastHit> _enemies;
    private int _enemiesCount;

    public void Initialize()
    {
        _enemies = new List<RaycastHit>();
        _betweenCutsTimer = 0.0f;
    }

    public void UpdateWeaponInput(weaponInput input) //doubles as normal update
    {
        _requestedCut = input.Cut;
        _requestedParry = input.Parry;
        _requestedSheath = input.Sheath;
        _betweenCutsTimer -= Time.deltaTime;
        if (_requestedCut && _betweenCutsTimer < 0.0f) Cut();

        _sheathTimer -= Time.deltaTime;
        if (_requestedSheath && _sheathTimer < 0.0f) { _sheathTimer = sheathSpeed; _storedSheath = true; }
        if (_sheathTimer < 0.0f && _storedSheath) Sheath();

        _enemiesCount = _enemies is null ? 0 : _enemies.Count();
    }

    private void Cut()
    {
        _betweenCutsTimer = timeBetweenCuts;
        Vector3 pos = new Vector3(attackBoxCenter.transform.position.x, attackBoxCenter.transform.position.y / 2, attackBoxCenter.transform.position.z);
        var result = this.BoxCast(true, pos, cam.transform.forward, cutBoxSize, cam.transform.rotation, 0.0f, enemy, true);
        for (int i = 0; i < result.Hits.Count() && !_enemies.Exists(x => x.colliderInstanceID == result.Hits[i].colliderInstanceID); i++)
        {
            Debug.Log("observe"); // QUANTUM BALDE WORKS WITH ACTUAL QUANTUM MECHANICS.. somehow
            _enemies.Add(result.Hits[i]);
        }
    }
    private void Parry() // the platypus
    {

    }

    private void Sheath()
    {
        _storedSheath = false;
        foreach (RaycastHit rh in _enemies)
        {
            rh.collider.gameObject.GetComponent<EnemyScript>().Kill();
        }
        _enemies.Clear();
    }


    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 12;
        style.normal.textColor = Color.lightGray;
        GUI.Box(new Rect(Screen.width - 210, 5, 200, 200), "QUANTUMBALDE INFO:", GUI.skin.window);
        GUI.Label(new Rect(Screen.width - 205, 20, 750, 40), "enemys in list: " + _enemiesCount, style);
        GUI.Label(new Rect(Screen.width - 205, 35, 750, 40), "cut timer: " + _betweenCutsTimer, style);
        GUI.Label(new Rect(Screen.width - 205, 50, 750, 40), "sheath timer: " + _sheathTimer, style);
        
    }
}
