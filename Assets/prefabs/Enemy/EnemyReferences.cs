using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class EnemyReferences : MonoBehaviour
{
    [HideInInspector] public NavMeshAgent _navMeshAgent;
    [HideInInspector] public EnemyShooter _shooter;
    [Header("Stats")]
    public float _pathUpdateDelay = 0.2f;
    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _shooter = GetComponent<EnemyShooter>();
    }
}
