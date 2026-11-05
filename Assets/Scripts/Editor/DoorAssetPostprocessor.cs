using UnityEngine;
using UnityEditor;

namespace AREscapeRoom.Editor
{
    [InitializeOnLoad]
    public static class DoorAssetPostprocessor
    {
        static DoorAssetPostprocessor()
        {
            EditorApplication.delayCall += EnsureDoorAssetConfigured;
        }

        [InitializeOnLoadMethod]
        public static void EnsureDoorAssetConfigured()
        {
            string fbxPath = "Assets/Door/Door.fbx";
            string matPath = "Assets/Door/Materials/Door_Material.mat";
            string defaultMatPath = "Assets/Door/Materials/Default.mat";
            string prefabPath = "Assets/Door/Prefabs/Door.prefab";

            Texture2D albedo = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Door/Textures/Door_Default_Albedo.png");
            Texture2D normal = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Door/Textures/Door_Default_Normal.png");
            Texture2D metallic = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Door/Textures/Door_Default_MetallicSmoothness.png");
            Texture2D ao = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Door/Textures/Door_Default_AO.png");

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat != null)
            {
                bool matDirty = false;
                if (albedo != null && mat.GetTexture("_MainTex") != albedo) { mat.SetTexture("_MainTex", albedo); matDirty = true; }
                if (normal != null && mat.GetTexture("_BumpMap") != normal) { mat.SetTexture("_BumpMap", normal); mat.EnableKeyword("_NORMALMAP"); matDirty = true; }
                if (metallic != null && mat.GetTexture("_MetallicGlossMap") != metallic) { mat.SetTexture("_MetallicGlossMap", metallic); mat.EnableKeyword("_METALLICGLOSSMAP"); matDirty = true; }
                if (ao != null && mat.GetTexture("_OcclusionMap") != ao) { mat.SetTexture("_OcclusionMap", ao); matDirty = true; }

                if (matDirty)
                {
                    EditorUtility.SetDirty(mat);
                }
            }

            Material defMat = AssetDatabase.LoadAssetAtPath<Material>(defaultMatPath);
            if (defMat != null)
            {
                bool defDirty = false;
                if (albedo != null && defMat.GetTexture("_MainTex") != albedo) { defMat.SetTexture("_MainTex", albedo); defDirty = true; }
                if (normal != null && defMat.GetTexture("_BumpMap") != normal) { defMat.SetTexture("_BumpMap", normal); defMat.EnableKeyword("_NORMALMAP"); defDirty = true; }
                if (metallic != null && defMat.GetTexture("_MetallicGlossMap") != metallic) { defMat.SetTexture("_MetallicGlossMap", metallic); defMat.EnableKeyword("_METALLICGLOSSMAP"); defDirty = true; }
                if (ao != null && defMat.GetTexture("_OcclusionMap") != ao) { defMat.SetTexture("_OcclusionMap", ao); defDirty = true; }

                if (defDirty)
                {
                    EditorUtility.SetDirty(defMat);
                }
            }

            // Ensure Prefab is created with the textured material
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab == null)
            {
                GameObject fbxObj = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
                if (fbxObj != null)
                {
                    GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(fbxObj);
                    if (instance != null)
                    {
                        Material appliedMat = mat != null ? mat : defMat;
                        if (appliedMat != null)
                        {
                            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
                            for (int i = 0; i < renderers.Length; i++)
                            {
                                renderers[i].sharedMaterial = appliedMat;
                            }
                        }

                        PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                        Object.DestroyImmediate(instance);
                        Debug.Log($"[DoorAssetPostprocessor] Successfully created textured Door prefab at {prefabPath}");
                    }
                }
            }

            AssetDatabase.SaveAssets();
        }
    }
}
