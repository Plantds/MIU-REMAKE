using UnityEngine;

[CreateAssetMenu(menuName = "Impact System/Spawn Object Effect", fileName = "SpawnObjectEffect")]
public class SpawnObjectEffect : ScriptableObject
{
    public GameObject _prefab;
    public float _probability = 1.0f;
    public bool _randomizeRotation;
    [Tooltip("Zero Value will loock the rotation on this axis. Values up to 360 are sensibe for each [x.y.z]")]
    public Vector3 _randomizeRotationMultiplier = Vector3.zero;
}
