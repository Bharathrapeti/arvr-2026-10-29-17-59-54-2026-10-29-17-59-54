using UnityEngine;
using UnityEditor;

namespace AREscapeRoom.Editor
{
    [InitializeOnLoad]
    public static class SatanAssetPostprocessor
    {
        static SatanAssetPostprocessor()
        {
            EditorApplication.delayCall += EnsureSatanAssetConfigured;
        }

        [InitializeOnLoadMethod]
        public static void EnsureSatanAssetConfigured()
        {
            string fbxPath = "Assets/Satan_Feuchere/Satan_Feuchere.fbx";
            string matPath = "Assets/Satan_Feuchere/Materials/Satan_Material.mat";
            string mainMatPath = "Assets/Satan_Feuchere/Materials/main.mat";
            string prefabPath = "Assets/Satan_Feuchere/Prefabs/Satan_Feuchere.prefab";

            Texture2D albedo = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Satan_Feuchere/Textures/Scaniverse.jpg");

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat != null && albedo != null)
            {
                if (mat.GetTexture("_MainTex") != albedo)
                {
                    mat.SetTexture("_MainTex", albedo);
                    EditorUtility.SetDirty(mat);
                }
            }

            Material mainMat = AssetDatabase.LoadAssetAtPath<Material>(mainMatPath);
            if (mainMat != null && albedo != null)
            {
                if (mainMat.GetTexture("_MainTex") != albedo)
                {
                    mainMat.SetTexture("_MainTex", albedo);
                    EditorUtility.SetDirty(mainMat);
                }
            }

            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab == null)
            {
                GameObject fbxObj = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
                if (fbxObj != null)
                {
                    GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(fbxObj);
                    if (instance != null)
                    {
                        Material appliedMat = mat != null ? mat : mainMat;
                        if (appliedMat != null)
                        {
                            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
                            for (int i = 0; i < renderers.Length; i++)
                            {
                                renderers[i].sharedMaterial = appliedMat;
                            }
                        }

                        // Add MeshCollider so it interacts with environment and raycasts
                        MeshFilter mf = instance.GetComponentInChildren<MeshFilter>(true);
                        if (mf != null && mf.sharedMesh != null)
                        {
                            MeshCollider mc = mf.gameObject.GetComponent<MeshCollider>();
                            if (mc == null) mc = mf.gameObject.AddComponent<MeshCollider>();
                            mc.sharedMesh = mf.sharedMesh;
                        }

                        PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                        Object.DestroyImmediate(instance);
                        Debug.Log($"[SatanAssetPostprocessor] Successfully created textured Satan prefab at {prefabPath}");
                    }
                }
            }

            AssetDatabase.SaveAssets();
        }
    }
}
