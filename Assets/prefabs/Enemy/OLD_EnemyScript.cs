using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class EnemyScript : MonoBehaviour
{
    [Header("Miscellaneous")]
    [SerializeField] private Transform _target;
    [SerializeField] private Player _player;
    private EnemyShooter _enemyShooter;
    private EnemyReferences _enemyReferences;
    private float _pathUpdateDeadline;
    private float _shootingDistance;

    enum enemyState
    {
        pathing,
        looking
    }
    enemyState _currState;
    bool _inRage;
    private void Awake()
    {
        _enemyReferences = GetComponent<EnemyReferences>();
        _enemyShooter = GetComponent<EnemyShooter>();
    }

    void Start()
    {
        _shootingDistance = _enemyReferences._navMeshAgent.stoppingDistance;
    }

    // Update is called once per frame
    void Update()
    {

        if (_target != null)
        {
            _inRage = Vector3.Distance(transform.position, _player.GetPos()) <= _shootingDistance;
            if (_inRage)
            {
                LookAtTarget();
                _enemyShooter.CallToFire();
            }
            else UpdatePath();
            
            
        }
    }

    private void LookAtTarget()
    {
        _currState = enemyState.looking;

        Vector3 lookPos = _player.GetPos() - transform.position;
        lookPos.y = 0.0f;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.2f);
        
    }

    private void UpdatePath()
    {
        if (Time.time >= _pathUpdateDeadline)
        {
            _currState = enemyState.pathing;
            _pathUpdateDeadline = Time.time + _enemyReferences._pathUpdateDelay;
            _enemyReferences._navMeshAgent.SetDestination(_player.GetPos());
        }
  
    }

    public void Kill()
    {
        Destroy(gameObject);
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 12;
        style.normal.textColor = Color.lightGray;
        GUI.Box(new Rect(5, Screen.height - 200, 200, 200), "EnemyTEST INFO:", GUI.skin.window);
        GUI.Label(new Rect(10, Screen.height - 200 + 15, 750, 40), "Path Update Deadline: " + _pathUpdateDeadline, style);
        GUI.Label(new Rect(10, Screen.height - 200 + 30, 750, 40), "state: " + _currState, style);
        GUI.Label(new Rect(10, Screen.height - 200 + 45, 750, 40), "inRange: " + _inRage, style);
    }
}
