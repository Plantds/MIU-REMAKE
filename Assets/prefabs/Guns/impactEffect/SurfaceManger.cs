using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceManger : MonoBehaviour
{
    private static SurfaceManger _instance;
    public static SurfaceManger Instance
    {
        get { return _instance; }
        private set { _instance = value; }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("ERROR::MORE_THEN_ONE_SURFACEMAGNGER_ACTIVE::DESTROYING_LATEST_ONE: " + name);
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    [SerializeField] private List<SurfaceType> _surfaces = new List<SurfaceType>();
    [SerializeField] private int _defaultPoolSize = 10;
    [SerializeField] private Surface _defultSurface;

    public void HandleImpact(GameObject hitObject, Vector3 hitPoint, Vector3 hitNormal, ImpactType impact, int TriangleIndex)
    {
        if (hitObject.TryGetComponent<Terrain>(out Terrain terrain))
        {
            List<TextureAlpha> activeTextures = getActiveTexturesFromTerrain(terrain, hitPoint);
            foreach (TextureAlpha activeTexture in activeTextures)
            {
                SurfaceType surfaceType = _surfaces.Find(surface => surface._albedo == activeTexture._texture);
                if (surfaceType != null)
                {
                    foreach (Surface.SurfaceImpactTypeEffect typeEffect in surfaceType._surface.impactTypeEffects)
                    {
                        if (typeEffect._impactType == impact)
                        {
                            PlayEffects(hitPoint, hitNormal, typeEffect._surfaceEffect, activeTexture._alpha);
                        }
                    }
                }
                else
                {
                    foreach (Surface.SurfaceImpactTypeEffect typeEffect in _defultSurface.impactTypeEffects)
                    {
                        if (typeEffect._impactType == impact)
                        {
                            PlayEffects(hitPoint, hitNormal, typeEffect._surfaceEffect, activeTexture._alpha);
                        }
                    }
                }
            }
        }
        else if (hitObject.TryGetComponent<Renderer>(out Renderer renderer))
        {
            Texture activeTexture = GetActiveTextureFromRenderer(renderer, TriangleIndex);

            SurfaceType surfaceType = _surfaces.Find(_surfaces => _surfaces._albedo == activeTexture);
            if (surfaceType != null)
            {
                foreach (Surface.SurfaceImpactTypeEffect typeEffect in surfaceType._surface.impactTypeEffects)
                {
                    if (typeEffect._impactType == impact)
                    {
                        PlayEffects(hitPoint, hitNormal, typeEffect._surfaceEffect, 1);
                    }
                }
            }
            else
            {
                foreach (Surface.SurfaceImpactTypeEffect typeEffect in _defultSurface.impactTypeEffects)
                {
                    if (typeEffect._impactType == impact)
                    {
                        PlayEffects(hitPoint, hitNormal, typeEffect._surfaceEffect, 1);
                    }
                }
            }
        }
    }

    private List<TextureAlpha> getActiveTexturesFromTerrain(Terrain terrain, Vector3 hitpoint)
    {
        Vector3 terrainPos = hitpoint - terrain.transform.position;
        Vector3 splatMapPos = new Vector3(
            terrainPos.x / terrain.terrainData.size.x,
            0,
            terrainPos.z / terrain.terrainData.size.z);

        int x = Mathf.FloorToInt(splatMapPos.x * terrain.terrainData.alphamapWidth);
        int z = Mathf.FloorToInt(splatMapPos.z * terrain.terrainData.alphamapHeight);

        float[,,] alphaMap = terrain.terrainData.GetAlphamaps(x, z, 1, 1);

        List<TextureAlpha> activeTextures = new List<TextureAlpha>();
        for (int i = 0; i < alphaMap.Length; i++)
        {
            if (alphaMap[0, 0, i] > 0)
            {
                activeTextures.Add(new TextureAlpha() {
                    _texture = terrain.terrainData.terrainLayers[i].diffuseTexture,
                    _alpha = alphaMap[0, 0, i]
                });
            }
        }

        return activeTextures;
    }

    private Texture GetActiveTextureFromRenderer(Renderer renderer, int triangleIndex) {
        if (renderer.TryGetComponent<MeshFilter>(out MeshFilter meshFilter))
        {
            Mesh mesh = meshFilter.mesh;

            if (mesh.subMeshCount > 1)
            {
                int[] hitTriangleIndices = new int[]{
                    mesh.triangles[triangleIndex*3],
                    mesh.triangles[triangleIndex*3 + 1],
                    mesh.triangles[triangleIndex*3 + 2]};

                for (int i = 0; i < mesh.subMeshCount; i++)
                {
                    int[] submeshTriangles = mesh.GetTriangles(i);
                    for (int j = 0; j < submeshTriangles.Length; j += 3)
                    {
                        if (submeshTriangles[j] == hitTriangleIndices[0]
                        && submeshTriangles[j + 1] == hitTriangleIndices[1]
                        && submeshTriangles[j + 2] == hitTriangleIndices[2])
                        {
                            return renderer.sharedMaterials[i].mainTexture;
                        }
                    }
                }
            }
        }
        else
        {
            return renderer.sharedMaterial.mainTexture;
        }

        throw new System.Exception("GetActiveTextureFromRenderer didnt return a texture");
    }

    private void PlayEffects(Vector3 hitPoint, Vector3 hitNormal, SurfaceEffect surfaceEffect, float SundOffset) {
        foreach (SpawnObjectEffect spawnObjectEffect in surfaceEffect._spawnObjectEffects)
        {
            if (spawnObjectEffect._probability > UnityEngine.Random.value)
            {
                ObjectPool pool = ObjectPool.CreateInstance(spawnObjectEffect._prefab.GetComponent<PoolAbleObject>(), _defaultPoolSize);

                PoolAbleObject instance = pool.GetObject(hitPoint + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal));

                instance.transform.forward = hitNormal;
                if (spawnObjectEffect._randomizeRotation)
                {
                    Vector3 offset = new Vector3(
                        Random.Range(0, 180 * spawnObjectEffect._randomizeRotationMultiplier.x),
                        Random.Range(0, 180 * spawnObjectEffect._randomizeRotationMultiplier.y),
                        Random.Range(0, 180 * spawnObjectEffect._randomizeRotationMultiplier.z));

                    instance.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + offset);
                }
            }
        }

        foreach (playAduioHitEffect playAduioHitEffect in surfaceEffect._playAduioHitEffects)
        {
            AudioClip clip = playAduioHitEffect._audioClips[Random.Range(0, playAduioHitEffect._audioClips.Count)];
            ObjectPool pool = ObjectPool.CreateInstance(playAduioHitEffect._audioSourcePrefab.GetComponent<PoolAbleObject>(), _defaultPoolSize);
            AudioSource audioSource = pool.GetObject().GetComponent<AudioSource>();

            audioSource.transform.position = hitPoint;
            audioSource.PlayOneShot(clip, SundOffset * Random.Range(playAduioHitEffect._valumeRange.x, playAduioHitEffect._valumeRange.y));
            StartCoroutine(DisableAudioSource(audioSource, clip.length));
        }
    }

    private IEnumerator DisableAudioSource(AudioSource audioSource, float time) {
        yield return new WaitForSeconds(time);
        audioSource.gameObject.SetActive(false);
    }

    private class TextureAlpha
    {
        public float _alpha;
        public Texture _texture;
    }
}
