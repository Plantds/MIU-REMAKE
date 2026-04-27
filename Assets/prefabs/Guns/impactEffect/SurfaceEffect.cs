using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Impact System/Surface Effect", fileName = "SurfaceEffect")]
public class SurfaceEffect : ScriptableObject
{
    public List<SpawnObjectEffect> _spawnObjectEffects = new List<SpawnObjectEffect>();
    public List<playAduioHitEffect> _playAduioHitEffects = new List<playAduioHitEffect>();
}
