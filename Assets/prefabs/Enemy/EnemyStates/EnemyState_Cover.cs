using System;
using UnityEngine;

public class EnemyState_Cover : IState
{
    private EnemyReferences _enemyReferences;
    private EnemyStateMachine _enemyStateMachine;

    public EnemyState_Cover(EnemyReferences enemyReferences, EnemyStateMachine stateMachine)
    {
        this._enemyReferences = enemyReferences;
        _enemyStateMachine = stateMachine;

        var enemyShoot = new EnemyState_Shoot(enemyReferences);
        var enemyDelay = new EnemyState_Delay(1.0f);
        var enemyReload = new EnemyState_Reload(enemyReferences);

        At(enemyShoot, enemyReload, () => _enemyReferences._shooter.ShouldReload());
        At(enemyReload, enemyDelay, () => !_enemyReferences._shooter.ShouldReload());
        At(enemyDelay, enemyShoot, () => enemyDelay.IsDone());

        _enemyStateMachine.SetState(enemyShoot);

        void At(IState from, IState to, Func<bool> condition) => _enemyStateMachine.AddTransition(from, to, condition);
        //void Any(IState to, Func<bool> condition) => _enemyStateMachine.AddAnyTransition(to, condition);
    }

    public void OnEnter()
    {
    }

    public void OnExit()
    {
    }

    public void Tick()
    {
        _enemyStateMachine.Tick();
    }

    public Color GizmoColor()
    {
        return _enemyStateMachine.GetGizmoColor();
    }
}
