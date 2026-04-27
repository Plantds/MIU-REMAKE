using UnityEngine;

[CreateAssetMenu(fileName = "Trail Config", menuName = "Guns/Gun Trails Config", order = 4)]
public class TrailConfigScritableObject : ScriptableObject
{
    [Header("Visual Settings")]
    public Material       _material;
    public AnimationCurve _widthCurve;
    public float          _duration = 0.5f;
    public float          _minVertexDistance = 0.1f;
    public Gradient       _color;
    public bool           _emitting = false;
    public bool           _shadow = false;

    [Header("Distance/Speed Settings")]
    public float _missDistance = 100.0f;
    public float _simulationSpeed = 100.0f;
}
