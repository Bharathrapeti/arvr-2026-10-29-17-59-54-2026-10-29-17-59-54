using System.IO;
using UnityEditor;
using UnityEngine;
using AREscapeRoom.Puzzles;

namespace AREscapeRoom.Editor
{
    [InitializeOnLoad]
    public static class SetupPuzzleShapesAsset
    {
        private const string BASE_DIR = "Assets/FBX/PuzzleShapes";
        private const string TEX_DIR = BASE_DIR + "/Textures";
        private const string MAT_DIR = BASE_DIR + "/Materials";
        private const string PREFAB_DIR = BASE_DIR + "/Prefabs";

        static SetupPuzzleShapesAsset()
        {
            EditorApplication.delayCall += Initialize;
        }

        [MenuItem("EscapeRoom/Setup Puzzle Shapes Assets")]
        public static void Initialize()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || Application.isPlaying)
                return;

            if (!Directory.Exists(BASE_DIR))
                return;

            if (!Directory.Exists(MAT_DIR))
                Directory.CreateDirectory(MAT_DIR);

            if (!Directory.Exists(PREFAB_DIR))
                Directory.CreateDirectory(PREFAB_DIR);

            // Setup the 4 shapes
            SetupShapeAsset(ShapeType.Triangle, "Shape_Triangle", "Triangle_Albedo.png", "Triangle_MetallicSmoothness.png", 0.8f, 0.95f);
            SetupShapeAsset(ShapeType.Circle, "Shape_Circle", "Circle_Albedo.png", "Circle_MetallicSmoothness.png", 0.85f, 0.95f);
            SetupShapeAsset(ShapeType.Square, "Shape_Square", "Square_Albedo.png", "Square_MetallicSmoothness.png", 0.8f, 0.95f);
            SetupShapeAsset(ShapeType.Star, "Shape_Star", "Star_Albedo.png", "Star_MetallicSmoothness.png", 0.9f, 0.95f);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SetupPuzzleShapesAsset] Successfully configured all 4 3D puzzle shapes, materials, and prefabs.");
        }

        private static void SetupShapeAsset(ShapeType shapeType, string modelName, string albedoName, string msName, float metallic, float smoothness)
        {
            string fbxPath = $"{BASE_DIR}/{modelName}.fbx";
            string matPath = $"{MAT_DIR}/Mat_{modelName}.mat";
            string prefabPath = $"{PREFAB_DIR}/{modelName}.prefab";
            string albedoPath = $"{TEX_DIR}/{albedoName}";
            string msPath = $"{TEX_DIR}/{msName}";

            // 1. Material
            Material mat = GetOrCreateMaterial(matPath);
            Texture2D albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath);
            Texture2D ms = AssetDatabase.LoadAssetAtPath<Texture2D>(msPath);

            if (albedo != null)
            {
                mat.SetTexture("_MainTex", albedo);
                mat.SetColor("_Color", Color.white);
            }

            if (ms != null)
            {
                mat.SetTexture("_MetallicGlossMap", ms);
                mat.EnableKeyword("_METALLICGLOSSMAP");
            }

            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Glossiness", smoothness);
            EditorUtility.SetDirty(mat);

            // 2. Prefab
            if (File.Exists(fbxPath))
            {
                Mesh fbxMesh = null;
                var allAssets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
                foreach (var a in allAssets)
                {
                    if (a is Mesh m && !m.name.Contains("Preview"))
                    {
                        fbxMesh = m;
                        break;
                    }
                }

                GameObject fbxObj = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
                if (fbxMesh == null && fbxObj != null)
                {
                    var mf = fbxObj.GetComponentInChildren<MeshFilter>(true);
                    if (mf != null)
                        fbxMesh = mf.sharedMesh;
                }

                if (fbxMesh != null)
                {
                    GameObject instance = new GameObject(modelName);
                    var instMf = instance.AddComponent<MeshFilter>();
                    instMf.sharedMesh = fbxMesh;

                    var instMr = instance.AddComponent<MeshRenderer>();
                    instMr.sharedMaterial = mat;

                    var col = instance.AddComponent<MeshCollider>();
                    col.sharedMesh = fbxMesh;
                    col.convex = true;

                    var rb = instance.AddComponent<Rigidbody>();
                    rb.isKinematic = true;

                    var shapeComp = instance.AddComponent<ShapeObject>();
                    shapeComp.Bind(shapeType);

                    PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);

                    // Also save with alternate naming (e.g. Triangle_Shape.prefab)
                    string altName = modelName.Replace("Shape_", "") + "_Shape";
                    string altPrefabPath = $"{PREFAB_DIR}/{altName}.prefab";
                    instance.name = altName;
                    PrefabUtility.SaveAsPrefabAsset(instance, altPrefabPath);

                    if (shapeType == ShapeType.Triangle)
                    {
                        string trainglePrefabPath = $"{PREFAB_DIR}/Traingle_shape.prefab";
                        instance.name = "Traingle_shape";
                        PrefabUtility.SaveAsPrefabAsset(instance, trainglePrefabPath);
                    }

                    Object.DestroyImmediate(instance);
                }
            }
        }

        private static Material GetOrCreateMaterial(string path)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Standard");
                if (shader == null)
                    shader = Shader.Find("Universal Render Pipeline/Lit");
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }
    }
}
