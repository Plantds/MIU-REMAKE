using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Shoot Config", menuName = "Guns/shoot Config", order = 2)]
public class ShootConfigurationScriptableObject : ScriptableObject
{
    [Header("Shoot settings")]
    public LayerMask _hitMask;
    public float _fireRate = 0.25f;
    public float _recoilRecoveryRate = 1.0f;

    [Header("Simple Spread")]
    public float _maxSpreadTime = 1.0f;
    public BulletSpreadType _spreadType = BulletSpreadType.Simple;
    public Vector3 _spread = new Vector3(0.1f, 0.1f, 0.1f);
    [Header("Texture-Based Spread")]
    [Range(0.001f, 5f)] public float _spreadMultiplier = 0.1f;
    public Texture2D _spreadTexture;

    public Vector3 GetSpread(float shootTime = 0)
    {
        Vector3 spread = Vector3.zero;

        switch (_spreadType)
        {
            case BulletSpreadType.None:
                return spread;
            case BulletSpreadType.Simple:
                spread = Vector3.Lerp(Vector3.zero, new Vector3(
                Random.Range(-_spread.x, _spread.x),
                Random.Range(-_spread.y, _spread.y),
                Random.Range(-_spread.z, _spread.z)),
                Mathf.Clamp01(shootTime / _maxSpreadTime));
                spread.Normalize();
                return spread;
            case BulletSpreadType.TextureBased:
                spread = GetTextureDir(shootTime);
                spread *= _spreadMultiplier;
                return spread;
        }

        throw new System.Exception("Spread Type not set");
    }

    public Vector3 GetTextureDir(float shootTime)
    {
        Vector2 halfSize = new Vector2(_spreadTexture.width / 2.0f, _spreadTexture.height / 2.0f);
        int halfSquareExtents = Mathf.CeilToInt(Mathf.Lerp(0.01f, halfSize.x, Mathf.Clamp01(shootTime / _maxSpreadTime)));

        int minX = Mathf.FloorToInt(halfSize.x) - halfSquareExtents;
        int minY = Mathf.FloorToInt(halfSize.y) - halfSquareExtents;

        Color[] sampleColors = _spreadTexture.GetPixels( // samples colors form texture using an expending "band" /// Might want to switch to getting raw data for optimazation perperses 
            minX,
            minY,
            halfSquareExtents * 2,
            halfSquareExtents * 2
        );

        float[] colorsASGrey = System.Array.ConvertAll(sampleColors, (Color) => Color.grayscale); // converts all the colors on texture to grayscale
        float totalGreyValue = colorsASGrey.Sum();

        float grey = Random.Range(0, totalGreyValue);
        int i = 0;
        for (; i < colorsASGrey.Length; i++)
        {
            grey -= colorsASGrey[i];
            if (grey <= 0) break;
        }

        int x = minX + i % (halfSquareExtents * 2);
        int y = minY + i / (halfSquareExtents * 2);

        Vector2 targetPos = new Vector2(x, y);
        Vector2 dir = (targetPos - halfSize) / halfSize.x;

        return dir;
    }
}
