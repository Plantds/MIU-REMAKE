using System;
using UnityEngine;

public class EnemyBrain : MonoBehaviour
{
    private EnemyReferences _enemyReferences;
    private EnemyStateMachine _enemyStateMachine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _enemyReferences = GetComponent<EnemyReferences>();
        _enemyStateMachine = new EnemyStateMachine();

        CoverArea coverArea = FindFirstObjectByType<CoverArea>();

        // States
        var runToCover = new EnemyState_RunToCover(_enemyReferences, coverArea);
        var delayAfterRun = new EnemyState_Delay(0.1f);
        var inCover = new EnemyState_Cover(_enemyReferences, _enemyStateMachine);

        _enemyStateMachine.SetState(runToCover);

        // Transition
        At(runToCover, delayAfterRun, () => runToCover.HasArrivedAtDestination());
        At(delayAfterRun, inCover, () => delayAfterRun.IsDone());
        
        void At(IState from, IState to, Func<bool> condition) => _enemyStateMachine.AddTransition(from, to, condition);
        //void Any(IState to, Func<bool> condition) => _enemyStateMachine.AddAnyTransition(to, condition);
    }

    // Update is called once per frame
    void Update()
    {
        _enemyStateMachine.Tick();
    }

    private void OnDrawGizmos()
    {
        if (_enemyStateMachine != null)
        {
            Gizmos.color = _enemyStateMachine.GetGizmoColor();
            Gizmos.DrawSphere(transform.position + Vector3.up * 3.0f, 0.4f);
        }
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 12;
        style.normal.textColor = Color.lightGray;
        GUI.Box(new Rect(5, Screen.height - 200, 200, 200), "EnemyTEST INFO:", GUI.skin.window);
        GUI.Label(new Rect(10, Screen.height - 200 + 20, 750, 40), "state: " + _enemyStateMachine._currentState, style);
        
    }
}
