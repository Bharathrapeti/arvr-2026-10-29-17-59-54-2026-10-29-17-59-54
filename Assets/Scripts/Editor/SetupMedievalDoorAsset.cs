using System.IO;
using UnityEditor;
using UnityEngine;

namespace AREscapeRoom.Editor
{
    [InitializeOnLoad]
    public static class SetupMedievalDoorAsset
    {
        private const string FBX_DIR = "Assets/FBX/MedievalDoor_Lynx";
        private const string TEX_DIR = FBX_DIR + "/Textures";
        private const string MAT_DIR = FBX_DIR + "/Materials";
        private const string PREFAB_PATH = FBX_DIR + "/MedievalDoor_Lynx.prefab";
        private const string MODEL_PATH = FBX_DIR + "/MedievalDoor_Lynx.fbx";

        static SetupMedievalDoorAsset()
        {
            EditorApplication.delayCall += Initialize;
        }

        [MenuItem("EscapeRoom/Setup Medieval Lynx Door Asset")]
        public static void Initialize()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || Application.isPlaying)
                return;

            if (!Directory.Exists(TEX_DIR))
                return;

            if (!Directory.Exists(MAT_DIR))
                Directory.CreateDirectory(MAT_DIR);

            // 1. Mark normal maps as NormalMap type
            ConfigureTexture(TEX_DIR + "/Door_Normal_4K_fixed_JPG.jpg", true);
            ConfigureTexture(TEX_DIR + "/Normal_Stone_1UV_4K_JPG.jpg", true);
            ConfigureTexture(TEX_DIR + "/lynx_normal_muchcontrast_jpg_2k.jpg", true);

            // 2. Build Materials
            Material matWood = GetOrCreateMaterial(MAT_DIR + "/Mat_MedievalDoor_Wood.mat");
            AssignTexturesToMaterial(
                matWood,
                TEX_DIR + "/50na50.jpg",
                TEX_DIR + "/Door_Normal_4K_fixed_JPG.jpg",
                TEX_DIR + "/metallic_4k.jpg",
                TEX_DIR + "/Door_AO_4K_JPG_jasniejszy.jpg"
            );

            Material matStone = GetOrCreateMaterial(MAT_DIR + "/Mat_MedievalDoor_Stone.mat");
            AssignTexturesToMaterial(
                matStone,
                TEX_DIR + "/Base_color_Stone_1UV_4K_JPG.jpg",
                TEX_DIR + "/Normal_Stone_1UV_4K_JPG.jpg",
                null,
                TEX_DIR + "/SIMPLE_AO_lighter_Stone_1UV_4K_JPG.jpg"
            );

            Material matIron = GetOrCreateMaterial(MAT_DIR + "/Mat_MedievalDoor_Iron.mat");
            AssignTexturesToMaterial(
                matIron,
                TEX_DIR + "/lynx_Base_color_muchcontrast_jpg_2k.jpg",
                TEX_DIR + "/lynx_normal_muchcontrast_jpg_2k.jpg",
                TEX_DIR + "/lynx_Metallic.png",
                TEX_DIR + "/lynx_ao_lighter_4k.jpg"
            );
            matIron.SetFloat("_Metallic", 0.9f);
            matIron.SetFloat("_Glossiness", 0.6f);

            AssetDatabase.SaveAssets();

            // 3. Create Prefab if FBX exists
            if (File.Exists(MODEL_PATH) && !File.Exists(PREFAB_PATH))
            {
                GameObject fbxObj = AssetDatabase.LoadAssetAtPath<GameObject>(MODEL_PATH);
                if (fbxObj != null)
                {
                    GameObject instance = Object.Instantiate(fbxObj);
                    instance.name = "MedievalDoor_Lynx";

                    var renderers = instance.GetComponentsInChildren<MeshRenderer>(true);
                    foreach (var r in renderers)
                    {
                        string n = r.gameObject.name;
                        if (n.Contains("Stone") || n.Contains("Pillar") || n.Contains("Arch"))
                        {
                            r.sharedMaterial = matStone;
                        }
                        else if (n.Contains("Plank") || n.Contains("Door_Top"))
                        {
                            r.sharedMaterial = matWood;
                        }
                        else
                        {
                            r.sharedMaterial = matIron;
                        }
                    }

                    PrefabUtility.SaveAsPrefabAsset(instance, PREFAB_PATH);
                    Object.DestroyImmediate(instance);
                    Debug.Log($"[MedievalDoor] Successfully created prefab at {PREFAB_PATH}");
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

        private static void AssignTexturesToMaterial(Material mat, string albedoPath, string normalPath, string metallicPath, string occlusionPath)
        {
            if (!string.IsNullOrEmpty(albedoPath))
            {
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath);
                if (tex != null) mat.SetTexture("_MainTex", tex);
            }
            if (!string.IsNullOrEmpty(normalPath))
            {
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
                if (tex != null)
                {
                    mat.SetTexture("_BumpMap", tex);
                    mat.EnableKeyword("_NORMALMAP");
                }
            }
            if (!string.IsNullOrEmpty(metallicPath))
            {
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(metallicPath);
                if (tex != null)
                {
                    mat.SetTexture("_MetallicGlossMap", tex);
                    mat.EnableKeyword("_METALLICGLOSSMAP");
                }
            }
            if (!string.IsNullOrEmpty(occlusionPath))
            {
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(occlusionPath);
                if (tex != null) mat.SetTexture("_OcclusionMap", tex);
            }
            EditorUtility.SetDirty(mat);
        }
    }
}
