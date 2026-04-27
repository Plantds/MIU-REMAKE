using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateMachine
{
    public IState _currentState;
    private Dictionary<Type, List<Transition>> _transistion = new Dictionary<Type, List<Transition>>();
    private List<Transition> _currentTransition = new List<Transition>();
    private List<Transition> _anyTransition = new List<Transition>();
    private static List<Transition> _emptyTransition = new List<Transition>(capacity: 0);

    public void Tick() // called in update on the enemy
    {
        var transition = GetTransition();
        if (transition != null) SetState(transition.To);

        _currentState?.Tick();
    }

    public void SetState(IState state)
    {

        if (state == _currentState) { Debug.Log(state + " unchanged"); return;  }
        Debug.Log("set state " + _currentState + " to " + state);

        _currentState?.OnExit();
        _currentState = state;

        _transistion.TryGetValue(_currentState.GetType(), out _currentTransition);
        if (_currentTransition == null) _currentTransition = _emptyTransition;

        _currentState.OnEnter();
    }

    public void AddTransition(IState from, IState to, Func<bool> predicate)
    {
        if (_transistion.TryGetValue(from.GetType(), out var transitions) == false)
        {
            transitions = new List<Transition>();
            _transistion[from.GetType()] = transitions;
        }

        Debug.Log("adding transtion; from = " + from + ", to = " + to + ", condition = " + predicate);

        transitions.Add(new Transition(to, predicate)); // deleted as soon as you leave scope 
    }

    public void AddAnyTransition(IState state, Func<bool> predicate)
    {
        _anyTransition.Add(new Transition(state, predicate));
    }

    private class Transition
    {
        public Func<bool> Condition { get; }
        public IState To { get; }

        public Transition(IState to, Func<bool> condition)
        {
            To = to;
            Condition = condition;
        }
    }

    private Transition GetTransition()
    {
        foreach (var transition in _anyTransition)
            if (transition.Condition())
            {
                Debug.Log("getting transition: " + transition);
                return transition;
            }

        foreach (var transition in _currentTransition)
            if (transition.Condition())
            {
                Debug.Log("getting transition: " + transition);
                return transition;
            }

        return null;
    }

    public Color GetGizmoColor()
    {
        if (_currentState != null) return _currentState.GizmoColor();
        else return Color.gray;
    }
}
