using UnityEngine;

public class EnemyState_Delay : IState
{
    private float _waitForSecounds;
    private float _deadline;

    public EnemyState_Delay(float waitForSecounds)
    {
        this._waitForSecounds = waitForSecounds;
    }

    public void OnEnter()
    {
        _deadline = Time.time + _waitForSecounds;
    }

    public void OnExit()
    {

    }

    public void Tick()
    {

    }

    public Color GizmoColor()
    {
        return Color.white;
    }

    public bool IsDone()
    {
        return Time.time >= _deadline;
    }
}
