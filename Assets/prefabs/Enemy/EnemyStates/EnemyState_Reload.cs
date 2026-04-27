using UnityEngine;

public class EnemyState_Reload : IState
{
    private EnemyReferences _enemyReferences;
    public EnemyState_Reload(EnemyReferences enemyReferences) { this._enemyReferences = enemyReferences; }
    public void Tick()
    {

    }
    public void OnEnter()
    {
        
    }
    public void OnExit()
    {
        
    }
    public Color GizmoColor()
    {
        return Color.black;
    }
}
