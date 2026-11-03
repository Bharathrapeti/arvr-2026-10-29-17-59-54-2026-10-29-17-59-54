using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace AREscapeRoom.Editor
{
    [InitializeOnLoad]
    public static class InspectMallHierarchy
    {
        static InspectMallHierarchy()
        {
            EditorApplication.delayCall += CleanMallDuplicates;
            EditorApplication.delayCall += AutoOpenSceneOnLoad.EnsureLightforgedPedestalVisible;
            EditorApplication.delayCall += AutoOpenSceneOnLoad.EnsureDeskTableWhiteVisible;
        }

        [InitializeOnLoadMethod]
        private static void OnDomainReload()
        {
            EditorApplication.delayCall += CleanMallDuplicates;
            EditorApplication.delayCall += AutoOpenSceneOnLoad.EnsureLightforgedPedestalVisible;
            EditorApplication.delayCall += AutoOpenSceneOnLoad.EnsureDeskTableWhiteVisible;
        }

        [MenuItem("EscapeRoom/🔄 Restore Standard Layout (Hierarchy, Scene, Project)", priority = -100)]
        public static void RestoreStandardLayout()
        {
            var windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (var w in windows)
            {
                if (w != null && (w.titleContent.text.Contains("Version Control") || w.GetType().Name.Contains("Plastic")))
                {
                    try { w.Close(); } catch { }
                }
            }

            EditorApplication.ExecuteMenuItem("Window/Layouts/Default");

            var currentScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(currentScene.path) || !currentScene.path.Contains("RealisticMallScene"))
            {
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/RealisticMallScene.unity", UnityEditor.SceneManagement.OpenSceneMode.Single);
            }
        }

        [MenuItem("EscapeRoom/Setup Lightforged Pedestal with Room 6 Jobs", priority = 10)]
        public static void SetupLightforgedPedestalJobs()
        {
            AutoOpenSceneOnLoad.EnsureLightforgedPedestalVisible();
            Debug.Log("[LightforgedPedestal] Configured with all Room 6 Pedestal jobs (FinalVaultController, interaction, and slots)!");
        }

        [MenuItem("EscapeRoom/Clean Mall Duplicate Z-Fighting Meshes")]
        public static void CleanMallDuplicates()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || Application.isPlaying)
            {
                return;
            }

            GameObject mall = GameObject.Find("Ice_Scream_Shopping_Center_Map");
            if (mall == null) mall = GameObject.Find("Einkaufszentrum Map Ice Scream 3 FBX");
            if (mall == null)
            {
                Debug.Log("[AntiGlitch] Mall root not found in current scene; skipping inspection.");
                return;
            }

            MeshRenderer[] renderers = mall.GetComponentsInChildren<MeshRenderer>(true);
            Debug.Log($"[AntiGlitch] Total MeshRenderers found under mall: {renderers.Length}");

            HashSet<string> baseNames = new HashSet<string>();
            foreach (var mr in renderers)
            {
                if (mr != null && !mr.name.EndsWith(".001"))
                {
                    baseNames.Add(mr.name);
                }
            }

            int deactivatedCount = 0;
            List<GameObject> toDeactivate = new List<GameObject>();
            foreach (var mr in renderers)
            {
                if (mr != null && mr.name.EndsWith(".001"))
                {
                    string baseName = mr.name.Substring(0, mr.name.Length - 4);
                    if (baseNames.Contains(baseName))
                    {
                        toDeactivate.Add(mr.gameObject);
                    }
                }
            }

            Debug.Log($"[AntiGlitch] Found {toDeactivate.Count} duplicate .001 meshes with base counterparts in mall!");

            if (toDeactivate.Count > 0)
            {
                Undo.RecordObjects(toDeactivate.ToArray(), "Deactivate Duplicate Z-Fighting Meshes");
                foreach (var go in toDeactivate)
                {
                    if (go.activeSelf)
                    {
                        go.SetActive(false);
                        deactivatedCount++;
                    }
                    MeshRenderer mr = go.GetComponent<MeshRenderer>();
                    if (mr != null && mr.enabled)
                    {
                        mr.enabled = false;
                    }
                }

                Debug.Log($"[AntiGlitch] Successfully deactivated {deactivatedCount} duplicate Z-fighting meshes! Mall is now clean of duplicate geometry.");

                if (deactivatedCount > 0)
                {
                    var activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
                    if (activeScene.IsValid() && activeScene.isLoaded && !EditorApplication.isPlayingOrWillChangePlaymode && !Application.isPlaying)
                    {
                        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(activeScene);
                        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(activeScene);
                        Debug.Log($"[AntiGlitch] Saved scene '{activeScene.name}' with duplicate meshes deactivated.");
                    }
                }
            }
        }
    }
}
