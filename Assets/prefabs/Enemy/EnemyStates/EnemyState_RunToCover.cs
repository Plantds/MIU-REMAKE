using UnityEngine;

public class EnemyState_RunToCover : IState
{
    private EnemyReferences _enemyReferences;
    private CoverArea _coverArea;

    public EnemyState_RunToCover(EnemyReferences enemyReferences, CoverArea coverArea)
    {
        this._enemyReferences = enemyReferences;
        this._coverArea = coverArea;
    }

    public void OnEnter()
    {
        Cover nextCover = this._coverArea.GetRandomCover(_enemyReferences.transform.position);
        _enemyReferences._navMeshAgent.SetDestination(nextCover.transform.position);
    }

    public void OnExit()
    {
    }

    public void Tick()
    {
    }

    public bool HasArrivedAtDestination()
    {
        return _enemyReferences._navMeshAgent.remainingDistance <= 0.1f;
    }

    public Color GizmoColor()
    {
        return Color.blue;
    }
}
