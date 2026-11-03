using System.IO;
using UnityEditor;
using UnityEngine;

namespace AREscapeRoom.Editor
{
    [InitializeOnLoad]
    public static class SetupCeilingLightsAsset
    {
        private const string FBX_DIR = "Assets/FBX/Celling_Lights_V";
        private const string TEX_DIR = FBX_DIR + "/Textures";
        private const string MAT_DIR = FBX_DIR + "/Materials";
        private const string PREFAB_PATH = FBX_DIR + "/Celling_Lights_V.prefab";
        private const string MODEL_PATH = FBX_DIR + "/Celling_Lights_V.fbx";

        static SetupCeilingLightsAsset()
        {
            EditorApplication.delayCall += Initialize;
        }

        [MenuItem("EscapeRoom/Setup Ceiling Lights V Asset")]
        public static void Initialize()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || Application.isPlaying)
                return;

            if (!Directory.Exists(TEX_DIR))
                return;

            if (!Directory.Exists(MAT_DIR))
                Directory.CreateDirectory(MAT_DIR);

            // 1. Configure Normal Map
            ConfigureTexture(TEX_DIR + "/Aluminum_Champagne_Normal.png", true);

            // 2. Build Materials
            // Material 1: Aluminum_Anodized_Champagne
            Material matChampagne = GetOrCreateMaterial(MAT_DIR + "/Aluminum_Anodized_Champagne.mat");
            Texture2D albedoChamp = AssetDatabase.LoadAssetAtPath<Texture2D>(TEX_DIR + "/Aluminum_Champagne_Albedo.png");
            Texture2D metChamp = AssetDatabase.LoadAssetAtPath<Texture2D>(TEX_DIR + "/Aluminum_Champagne_MetallicSmoothness.png");
            Texture2D normChamp = AssetDatabase.LoadAssetAtPath<Texture2D>(TEX_DIR + "/Aluminum_Champagne_Normal.png");

            if (albedoChamp != null) matChampagne.SetTexture("_MainTex", albedoChamp);
            if (metChamp != null)
            {
                matChampagne.SetTexture("_MetallicGlossMap", metChamp);
                matChampagne.EnableKeyword("_METALLICGLOSSMAP");
            }
            if (normChamp != null)
            {
                matChampagne.SetTexture("_BumpMap", normChamp);
                matChampagne.EnableKeyword("_NORMALMAP");
            }
            matChampagne.SetFloat("_Metallic", 1.0f);
            matChampagne.SetFloat("_Glossiness", 0.95f);
            EditorUtility.SetDirty(matChampagne);

            // Material 2: PolyCarbonate (Glowing neon light)
            Material matPoly = GetOrCreateMaterial(MAT_DIR + "/PolyCarbonate.mat");
            Texture2D albedoPoly = AssetDatabase.LoadAssetAtPath<Texture2D>(TEX_DIR + "/PolyCarbonate_Albedo.png");
            Texture2D emitPoly = AssetDatabase.LoadAssetAtPath<Texture2D>(TEX_DIR + "/PolyCarbonate_Emission.png");

            if (albedoPoly != null) matPoly.SetTexture("_MainTex", albedoPoly);
            if (emitPoly != null)
            {
                matPoly.SetTexture("_EmissionMap", emitPoly);
            }
            matPoly.EnableKeyword("_EMISSION");
            // Vibrant lime-yellow glow matching Sketchfab
            Color hdrColor = new Color(0.883f * 2.5f, 0.931f * 2.5f, 0.091f * 2.5f, 1f);
            matPoly.SetColor("_EmissionColor", hdrColor);
            matPoly.SetFloat("_Glossiness", 0.9f);
            matPoly.SetFloat("_Metallic", 0.0f);
            EditorUtility.SetDirty(matPoly);

            AssetDatabase.SaveAssets();

            // 3. Create Prefab if FBX exists
            if (File.Exists(MODEL_PATH) && !File.Exists(PREFAB_PATH))
            {
                GameObject fbxObj = AssetDatabase.LoadAssetAtPath<GameObject>(MODEL_PATH);
                if (fbxObj != null)
                {
                    GameObject instance = Object.Instantiate(fbxObj);
                    instance.name = "Celling_Lights_V";

                    var renderers = instance.GetComponentsInChildren<MeshRenderer>(true);
                    foreach (var r in renderers)
                    {
                        string n = r.gameObject.name;
                        if (n.Contains("Material3"))
                        {
                            r.sharedMaterial = matPoly;
                        }
                        else
                        {
                            r.sharedMaterial = matChampagne;
                        }
                    }

                    // Add soft downward illumination
                    GameObject lightObj = new GameObject("CeilingLight_Point");
                    lightObj.transform.SetParent(instance.transform, false);
                    lightObj.transform.localPosition = new Vector3(0f, -0.5f, 0f);
                    Light l = lightObj.AddComponent<Light>();
                    l.type = LightType.Point;
                    l.color = new Color(0.92f, 0.96f, 0.55f);
                    l.range = 8.0f;
                    l.intensity = 2.0f;
                    l.shadows = LightShadows.Soft;

                    PrefabUtility.SaveAsPrefabAsset(instance, PREFAB_PATH);
                    Object.DestroyImmediate(instance);
                    Debug.Log($"[CeilingLights] Successfully created prefab at {PREFAB_PATH}");
                }
            }
        }

        private static void ConfigureTexture(string path, bool isNormal)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                bool modified = false;
                if (isNormal && importer.textureType != TextureImporterType.NormalMap)
                {
                    importer.textureType = TextureImporterType.NormalMap;
                    modified = true;
                }
                if (modified)
                {
                    importer.SaveAndReimport();
                }
            }
        }

        private static Material GetOrCreateMaterial(string path)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Standard");
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }
    }
}
