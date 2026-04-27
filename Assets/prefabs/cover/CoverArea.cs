using UnityEngine;

public class CoverArea : MonoBehaviour
{
    private Cover[] _covers;

    void Awake()
    {
        _covers = GetComponentsInChildren<Cover>();
    }

    public Cover GetRandomCover(Vector3 coverPos)
    {
        return _covers[Random.Range(0, _covers.Length - 1)];
    }
}
