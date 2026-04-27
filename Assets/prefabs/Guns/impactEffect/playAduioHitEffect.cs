using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Impact System/Play Audio Effect", fileName = "PlayAudioEffect")]
public class playAduioHitEffect : ScriptableObject
{
    public AudioSource _audioSourcePrefab;
    public List<AudioClip> _audioClips = new List<AudioClip>();
    [Tooltip("Values are clamped to [0 -> 1]")]
    public Vector2 _valumeRange = new Vector2(0, 1);
}
