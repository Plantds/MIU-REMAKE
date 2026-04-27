using UnityEngine;

public class EnemyState_Shoot : IState
{
    private EnemyReferences _enemyReferences;
    private Transform _traget;

    public EnemyState_Shoot(EnemyReferences enemyReferences) { this._enemyReferences = enemyReferences; }

    public void Tick()
    {
        if (_traget != null)
        {
            Vector3 lookPos = _traget.position - _enemyReferences.transform.position;
            lookPos.y = 0.0f;
            Quaternion rotation = Quaternion.LookRotation(lookPos);
            _enemyReferences.transform.rotation = Quaternion.Slerp(_enemyReferences.transform.rotation, rotation, 0.2f);
        }
    }
    public void OnEnter()
    {
        _traget = GameObject.FindWithTag("player").transform;
    }
    public void OnExit()
    {
        _traget = null;
    }
    public Color GizmoColor()
    {
        return Color.orange;
    }
}
