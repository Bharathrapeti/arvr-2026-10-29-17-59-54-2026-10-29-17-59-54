using System.Collections.Generic;







using UnityEditor;







using UnityEditor.SceneManagement;







using UnityEngine;







using AREscapeRoom.Puzzles;







using AREscapeRoom.Core;
using AREscapeRoom.Player;















namespace AREscapeRoom.Editor







{







    [InitializeOnLoad]







    public static class AutoOpenSceneOnLoad







    {







        private const string TARGET_SCENE = "Assets/Scenes/RealisticMallScene.unity";















        static AutoOpenSceneOnLoad()







        {







            EditorApplication.delayCall += InitializeEditorScene;
            EditorApplication.playModeStateChanged += (PlayModeStateChange state) =>
            {
                if (state == PlayModeStateChange.ExitingEditMode)
                {
                    EditorSceneManager.SaveOpenScenes();
                }
            };







        }















        [InitializeOnLoadMethod]







        private static void OnDomainReload()







        {







            EditorApplication.delayCall += InitializeEditorScene;







        }























        















        [MenuItem("EscapeRoom/Open Ice Scream Mall Scene")]







        public static void OpenIceScreamMallScene()







        {







            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())







            {







                string scenePath = "Assets/Scenes/IceScreamMallScene.unity";







                if (System.IO.File.Exists(scenePath))







                {







                    var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);







                    EnsureIceScreamMallVisible();







                    Debug.Log("[AutoOpenSceneOnLoad] Opened Ice Scream Mall Scene successfully!");







                }







                else







                {







                    Debug.LogError("[AutoOpenSceneOnLoad] Scene not found: " + scenePath);







                }







            }







        }















        [MenuItem("EscapeRoom/Place Ice Scream Mall In Current Scene")]







        public static void PlaceIceScreamMallInCurrentScene()







        {







            if (Application.isPlaying) return;















            GameObject existing = GameObject.Find("Ice_Scream_Shopping_Center_Map");







            if (existing == null) existing = GameObject.Find("Einkaufszentrum Map Ice Scream 3 FBX");















            if (existing != null)







            {







                Selection.activeGameObject = existing;







                EditorGUIUtility.PingObject(existing);







                Debug.Log("[AutoOpenSceneOnLoad] Ice Scream Mall already in scene. Selected.");







                EnsureIceScreamMallVisible();







                return;







            }















            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ice-scream-3-shopping-center-map/Ice_Scream_Shopping_Center_Map.prefab");







            if (prefab == null)







            {







                prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ice-scream-3-shopping-center-map/source/Einkaufszentrum Map Ice Scream 3 FBX.fbx");







            }















            if (prefab != null)







            {







                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);







                instance.name = "Ice_Scream_Shopping_Center_Map";







                var activeScene = EditorSceneManager.GetActiveScene();







                if (activeScene.name == "RealisticMallScene")







                {







                    instance.transform.position = new Vector3(-1.8f, 0.02f, -54.4f);







                    instance.transform.rotation = Quaternion.Euler(0f, 176.939f, 0f);







                }







                else







                {







                    instance.transform.position = new Vector3(0f, 0.02f, 0f);







                    instance.transform.rotation = Quaternion.identity;







                }







                instance.transform.localScale = Vector3.one;















                EnsureIceScreamMallVisible();















                Selection.activeGameObject = instance;







                EditorGUIUtility.PingObject(instance);















                var scene = EditorSceneManager.GetActiveScene();







                if (scene.IsValid() && scene.isLoaded)







                {







                    EditorSceneManager.MarkSceneDirty(scene);







                    EditorSceneManager.SaveScene(scene);







                    Debug.Log("[AutoOpenSceneOnLoad] Placed and saved Ice_Scream_Shopping_Center_Map in " + scene.name);







                }







            }







        }















        [MenuItem("EscapeRoom/Ensure Ice Scream Mall Materials Applied")]







        public static void EnsureIceScreamMallVisible()







        {







            if (Application.isPlaying) return;















            string matBasePath = "Assets/ice-scream-3-shopping-center-map/Materials/";







            string[] matNames = new string[] {







                "Default-Material", "IS_Mobile_CentroComercial_02", "IS_Mobile_CentroComercial_02_unlit",







                "IS_Objetos1", "IS_Mobile_CentroComercial_01", "IS_Mobile_Objetos_01",







                "candle_tex", "IS_Mobile_CentroComercial_01_luces", "gris_transparente",







                "pasted__IS_Centrocomercial", "IS_Centrocomercial_Vidrio", "IS_Centrocomercial_Agua",







                "IS_Mobile_CentroComercial_agua", "IS_Mobile_Furgoneta_01",







                "pasted__pasted__pasted__pasted__IS_Centrocomercial", "pasted__IS_Centrocomercial2",







                "IS_Mobile_CentroComercial_tienda_motos", "IS_Mobile_CentroComercial_tienda_motos_trans",







                "IS_Mobile_CentroComercial_tienda_supervivencia", "IS_Mobile_CentroComercial_tienda_supervivencia_trans",







                "carrera_minimoto", "IS_Mobile_Thumbnail", "IS_Mobile_Furgoneta_luz",







                "pasted__pasted__IS_Centrocomercial", "IS_Mobile_CentroComercial_trans", "wick"







            };















            System.Collections.Generic.Dictionary<string, Material> matsDict = new System.Collections.Generic.Dictionary<string, Material>();







            foreach (var mn in matNames)







            {







                Material m = AssetDatabase.LoadAssetAtPath<Material>(matBasePath + mn + ".mat");







                if (m != null) matsDict[mn] = m;







            }















            GameObject root = GameObject.Find("Ice_Scream_Shopping_Center_Map");







            if (root == null) root = GameObject.Find("Einkaufszentrum Map Ice Scream 3 FBX");















            if (root != null)







            {







                // Respect manual editor layout for mall root:
                // if (root.transform.position.y < 0.01f) { Vector3 pos = root.transform.position; pos.y = 0.02f; root.transform.position = pos; EditorUtility.SetDirty(root.transform); }















                MeshRenderer[] renderers = root.GetComponentsInChildren<MeshRenderer>(true);







                bool anyChanged = false;















                // Deactivate all duplicate .001 meshes that have a base counterpart to eliminate Z-fighting







                System.Collections.Generic.HashSet<string> baseNames = new System.Collections.Generic.HashSet<string>();







                foreach (var mr in renderers)







                {







                    if (mr != null && !mr.name.EndsWith(".001"))







                    {







                        baseNames.Add(mr.name);







                    }







                }







                foreach (var mr in renderers)







                {







                    if (mr != null && mr.name.EndsWith(".001"))







                    {







                        string baseName = mr.name.Substring(0, mr.name.Length - 4);







                        if (baseNames.Contains(baseName))







                        {







                            if (mr.gameObject.activeSelf)







                            {







                                mr.gameObject.SetActive(false);







                                anyChanged = true;







                            }







                            if (mr.enabled)







                            {







                                mr.enabled = false;







                                anyChanged = true;







                            }







                        }







                    }







                }















                foreach (var mr in renderers)







                {







                    // Disable shadow casting on roof/ceiling meshes to eliminate shadow cascade popping







                    string nLower = mr.gameObject.name.ToLower();







                    if (nLower.Contains("techo") || nLower.Contains("roof") || nLower.Contains("cubierta") || nLower.Contains("ceiling"))







                    {







                        if (mr.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off)







                        {







                            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;







                            EditorUtility.SetDirty(mr);







                            anyChanged = true;







                        }







                    }







                }















                // Calibrate scene ambient lighting for mall interior







                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;







                RenderSettings.ambientSkyColor = new Color(0.48f, 0.50f, 0.55f, 1f);







                RenderSettings.ambientEquatorColor = new Color(0.38f, 0.40f, 0.44f, 1f);







                RenderSettings.ambientGroundColor = new Color(0.30f, 0.32f, 0.35f, 1f);







                RenderSettings.ambientIntensity = 1f;















                RenderSettings.fog = false;















                Camera[] cams = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);







                foreach (var cam in cams)







                {







                    cam.clearFlags = CameraClearFlags.Skybox;







                    cam.backgroundColor = new Color(0.19215687f, 0.3019608f, 0.4745098f, 0f);







                    EditorUtility.SetDirty(cam);







                }















                Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);







                foreach (var l in lights)







                {







                    if (l.type == LightType.Directional)







                    {







                        l.color = new Color(1f, 0.95686275f, 0.8392157f, 1f); // warm daytime sun







                        l.intensity = 0.65f;







                        if (l.shadows != LightShadows.None)







                        {







                            l.shadowStrength = 0.25f;







                            l.shadowBias = 0.05f;







                            l.shadowNormalBias = 0.4f;







                        }







                        EditorUtility.SetDirty(l);







                    }







                }















                Material cartelMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/New Material.mat");







                if (cartelMat != null)







                {







                    foreach (var mr in renderers)







                    {







                        if (mr != null && mr.name == "Cartel15")







                        {







                            if (mr.sharedMaterial != cartelMat)







                            {







                                mr.sharedMaterial = cartelMat;







                                EditorUtility.SetDirty(mr);







                                anyChanged = true;







                                Debug.Log("[AutoOpenSceneOnLoad] Assigned New Material (AR ESCAPE ROOM sign) to Cartel15!");







                            }







                        }







                    }







                }















                foreach (var mr in renderers)







                {







                    Material[] curMats = mr.sharedMaterials;







                    bool modified = false;







                    for (int i = 0; i < curMats.Length; i++)







                    {







                        if (curMats[i] != null && matsDict.ContainsKey(curMats[i].name))







                        {







                            if (curMats[i] != matsDict[curMats[i].name])







                            {







                                curMats[i] = matsDict[curMats[i].name];







                                modified = true;







                            }







                        }







                    }







                    if (modified)







                    {







                        mr.sharedMaterials = curMats;







                        EditorUtility.SetDirty(mr);







                        anyChanged = true;







                    }







                }















                if (anyChanged)







                {







                    var scene = EditorSceneManager.GetActiveScene();







                    if (scene.IsValid() && scene.isLoaded)







                    {







                        EditorSceneManager.MarkSceneDirty(scene);







                        EditorSceneManager.SaveScene(scene);







                        Debug.Log("[AutoOpenSceneOnLoad] Ice Scream Mall materials remapped and saved in " + scene.name);







                    }







                }







            }







        }















        [MenuItem("EscapeRoom/Open Starcourt Mall Scene")]







        public static void OpenStarcourtMallScene()







        {







            string path = "Assets/Scenes/StarcourtMallScene.unity";







            if (System.IO.File.Exists(path))







            {







                EditorSceneManager.OpenScene(path, OpenSceneMode.Single);







                Debug.Log("[AutoOpen] Opened Starcourt Mall Scene!");







            }







        }















        [MenuItem("EscapeRoom/Ensure Luxe Hanger Lamp In Scene")]







        public static void ForceEnsureLuxeHangerLamp()







        {







            EnsureLuxeHangerLampVisible();







        }















        [MenuItem("EscapeRoom/Ensure Modern Entertainment Center In Scene")]







        public static void ForceEnsureEntertainmentCenter()







        {







            EnsureModernEntertainmentCenterVisible();







            EnsureLuxeHangerLampVisible();







        }















        [MenuItem("EscapeRoom/Ensure Lightforged Pedestal In Scene")]







        public static void ForceEnsureLightforgedPedestal()







        {







            EnsureLightforgedPedestalVisible();







        }















        [MenuItem("EscapeRoom/Ensure Desk Table White In Scene")]







        public static void ForceEnsureDeskTableWhite()







        {







            EnsureDeskTableWhiteVisible();

            SetupRoom5ShapesAndSockets();
            EnsurePlayerAtStartPosition();







            EnsureCautionFrameAttachedToDoor();







        }















        [MenuItem("EscapeRoom/Ensure 5 Visible Diamonds In Scene")]







        public static void ForceEnsureDiamonds()







        {







            InitializeEditorScene();







        }















        private static void InitializeEditorScene()







        {







            if (Application.isPlaying)







            {







                SceneDoorBootstrap.SetupBlueDiamond();

                // Do not override player position - preserve user placement at Main Camera







                return;







            }















            var activeScene = EditorSceneManager.GetActiveScene();







            if (string.IsNullOrEmpty(activeScene.path) || activeScene.rootCount == 0)







            {







                if (System.IO.File.Exists(TARGET_SCENE))







                {







                    Debug.Log("[AutoOpen] Opening " + TARGET_SCENE);







                    activeScene = EditorSceneManager.OpenScene(TARGET_SCENE, OpenSceneMode.Single);







                }







            }















            EnsureDiamondsVisibleInScene();







            EnsureModernEntertainmentCenterVisible();







            EnsureLuxeHangerLampVisible();







            EnsureStarcourtMallVisible();







            EnsureLightforgedPedestalVisible();







            EnsureDeskTableWhiteVisible();







        }















        public static void EnsureDiamondsVisibleInScene()







        {







            if (Application.isPlaying) return;















            Mesh diamondMesh = null;







            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath("Assets/Blue_Diamond/Blue_Diamond.obj");







            foreach (var a in allAssets)







            {







                if (a is Mesh m)







                {







                    diamondMesh = m;







                    break;







                }







            }















            Material diamondMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Blue_Diamond/Materials/Blue_Diamond_Material.mat");















            GameObject room3 = GameObject.Find("Room_3");







            Transform room3Transform = room3 != null ? room3.transform : null;















            Vector3[] diamondPositions = new Vector3[]







            {







                new Vector3(1.72f, 1.0f, 2.81f),    // Diamond 1







                new Vector3(-4.5f, 1.34f, -1.5f),   // Diamond 2







                new Vector3(4.5f, 1.6f, 1.2f),      // Diamond 3







                new Vector3(-3.2f, 1.6f, 4.4f),     // Diamond 4







                new Vector3(3.4f, 2.0f, -4.4f)      // Diamond 5







            };















            bool changed = false;















            for (int k = 1; k <= 5; k++)







            {







                string targetName = "Blue_Diamond_" + k;







                GameObject dObj = GameObject.Find(targetName);















                if (dObj == null)







                {







                    dObj = new GameObject(targetName);







                    if (room3Transform != null) dObj.transform.SetParent(room3Transform, false);







                    changed = true;







                }















                if (room3Transform != null && dObj.transform.parent != room3Transform)







                {







                    dObj.transform.SetParent(room3Transform, false);







                    changed = true;







                }















                // Respect visual editor layout coordinates for existing diamonds in scene:
                // dObj.transform.localPosition = diamondPositions[k - 1];







                // dObj.transform.localRotation = Quaternion.Euler(0f, (k - 1) * 72f, 0f);







                // dObj.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);















                MeshFilter mf = dObj.GetComponent<MeshFilter>();







                if (mf == null) { mf = dObj.AddComponent<MeshFilter>(); changed = true; }







                if (diamondMesh != null && mf.sharedMesh != diamondMesh)







                {







                    mf.sharedMesh = diamondMesh;







                    EditorUtility.SetDirty(mf);







                    changed = true;







                }















                MeshRenderer mr = dObj.GetComponent<MeshRenderer>();







                if (mr == null) { mr = dObj.AddComponent<MeshRenderer>(); changed = true; }







                if (diamondMat != null && mr.sharedMaterial != diamondMat)







                {







                    mr.sharedMaterial = diamondMat;







                    EditorUtility.SetDirty(mr);







                    changed = true;







                }







                mr.enabled = true;















                SphereCollider sc = dObj.GetComponent<SphereCollider>();







                if (sc == null) { sc = dObj.AddComponent<SphereCollider>(); changed = true; }







                sc.center = Vector3.zero;







                sc.radius = 1.0f;







                sc.isTrigger = false;







                sc.enabled = true;







                EditorUtility.SetDirty(sc);















                GemCollectible[] childGems = dObj.GetComponentsInChildren<GemCollectible>(true);







                for (int c = 0; c < childGems.Length; c++)







                {







                    if (childGems[c] != null && childGems[c].gameObject != dObj)







                    {







                        Object.DestroyImmediate(childGems[c]);







                        changed = true;







                    }







                }















                GemCollectible gem = dObj.GetComponent<GemCollectible>();







                if (gem == null) { gem = dObj.AddComponent<GemCollectible>(); changed = true; }







                gem.enabled = true;







                EditorUtility.SetDirty(gem);















                dObj.SetActive(true);







                EditorUtility.SetDirty(dObj);







            }















            GameObject standalone = GameObject.Find("Blue_Diamond");







            if (standalone != null)







            {







                standalone.SetActive(false);







                EditorUtility.SetDirty(standalone);







                changed = true;







            }















            

            // Decommission old desk interactive elements (make it a static decorative asset only)

            GameObject oldDesk = GameObject.Find("Desk");

            if (oldDesk != null)

            {

                Transform oldDrawerKey = oldDesk.transform.Find("Drawer_Key");

                if (oldDrawerKey != null)

                {

                    Transform oldKey = oldDrawerKey.Find("Key");

                    if (oldKey != null)

                    {

                        Undo.DestroyObjectImmediate(oldKey.gameObject);

                        changed = true;

                    }

                    DrawerController oldDc = oldDrawerKey.GetComponent<DrawerController>();

                    if (oldDc != null)

                    {

                        Undo.DestroyObjectImmediate(oldDc);

                        changed = true;

                    }

                }

                Transform oldDrawerEmpty = oldDesk.transform.Find("Drawer_Empty");

                if (oldDrawerEmpty != null)

                {

                    DrawerController oldDcEmpty = oldDrawerEmpty.GetComponent<DrawerController>();

                    if (oldDcEmpty != null)

                    {

                        Undo.DestroyObjectImmediate(oldDcEmpty);

                        changed = true;

                    }

                }

            }



            if (changed)







            {







                var scene = EditorSceneManager.GetActiveScene();







                if (scene.IsValid() && scene.isLoaded)







                {







                    EditorSceneManager.MarkSceneDirty(scene);







                    EditorSceneManager.SaveScene(scene);







                    Debug.Log("[AutoOpenSceneOnLoad] All 5 Blue Diamonds updated and saved in Scene view!");







                }







            }







        }







    







        public static void EnsureModernEntertainmentCenterVisible()







        {







            if (Application.isPlaying) return;















            Mesh mecMesh = null;







            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath("Assets/modern-entertainment-center-free/source/M-Entertainment.obj");







            foreach (var a in allAssets)







            {







                if (a is Mesh m)







                {







                    mecMesh = m;







                    break;







                }







            }















            Material matScreen = AssetDatabase.LoadAssetAtPath<Material>("Assets/modern-entertainment-center-free/Materials/Screen.mat");







            Material matTV = AssetDatabase.LoadAssetAtPath<Material>("Assets/modern-entertainment-center-free/Materials/TV_&_Cables.mat");







            Material matStand = AssetDatabase.LoadAssetAtPath<Material>("Assets/modern-entertainment-center-free/Materials/Stand.mat");







            Material matPlayer = AssetDatabase.LoadAssetAtPath<Material>("Assets/modern-entertainment-center-free/Materials/Player_&_Decor.mat");















            GameObject mec = GameObject.Find("Modern_Entertainment_Center");







            bool changed = false;















            if (mec == null)







            {







                mec = new GameObject("Modern_Entertainment_Center");







                mec.transform.position = new Vector3(-2.2f, 0f, -23.0f);







                mec.transform.rotation = Quaternion.Euler(0f, 90f, 0f);







                mec.transform.localScale = Vector3.one;







                changed = true;







            }















            MeshFilter mf = mec.GetComponent<MeshFilter>();







            if (mf == null) { mf = mec.AddComponent<MeshFilter>(); changed = true; }







            if (mecMesh != null && mf.sharedMesh != mecMesh)







            {







                mf.sharedMesh = mecMesh;







                EditorUtility.SetDirty(mf);







                changed = true;







            }















            MeshRenderer mr = mec.GetComponent<MeshRenderer>();







            if (mr == null) { mr = mec.AddComponent<MeshRenderer>(); changed = true; }







            







            Material[] targetMats = new Material[] { matScreen, matTV, matStand, matPlayer };







            bool matsMatch = mr.sharedMaterials != null && mr.sharedMaterials.Length == 4;







            if (matsMatch)







            {







                for (int i = 0; i < 4; i++)







                {







                    if (mr.sharedMaterials[i] != targetMats[i]) { matsMatch = false; break; }







                }







            }















            if (!matsMatch)







            {







                mr.sharedMaterials = targetMats;







                EditorUtility.SetDirty(mr);







                changed = true;







            }







            mr.enabled = true;















            BoxCollider bc = mec.GetComponent<BoxCollider>();







            if (bc == null) { bc = mec.AddComponent<BoxCollider>(); changed = true; }







            bc.size = new Vector3(2.6f, 1.8f, 0.7f);







            bc.center = new Vector3(0f, 0.9f, 0f);







            bc.enabled = true;







            EditorUtility.SetDirty(bc);















            mec.SetActive(true);







            EditorUtility.SetDirty(mec);















            if (changed)







            {







                var scene = EditorSceneManager.GetActiveScene();







                if (scene.IsValid() && scene.isLoaded)







                {







                    EditorSceneManager.MarkSceneDirty(scene);







                    EditorSceneManager.SaveScene(scene);







                    Debug.Log("[AutoOpenSceneOnLoad] Modern_Entertainment_Center fully textured and saved in Scene view!");







                }







            }







        }















        public static void EnsureLuxeHangerLampVisible()







        {







            if (Application.isPlaying) return;















            Mesh lampMesh = null;







            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath("Assets/Luxe_Hanger_Lamp/source/Luxe_Hanger_Lamp.obj");







            foreach (var a in allAssets)







            {







                if (a is Mesh m)







                {







                    lampMesh = m;







                    break;







                }







            }















            Material matMetal = AssetDatabase.LoadAssetAtPath<Material>("Assets/Luxe_Hanger_Lamp/Materials/Hanger_BlackMetal.mat");







            Material matCrystal = AssetDatabase.LoadAssetAtPath<Material>("Assets/Luxe_Hanger_Lamp/Materials/Hanger_CrystalGlass.mat");







            Material matBulb = AssetDatabase.LoadAssetAtPath<Material>("Assets/Luxe_Hanger_Lamp/Materials/Hanger_BulbGlow.mat");















            GameObject lamp = GameObject.Find("Luxe_Hanger_Lamp");







            bool changed = false;















            if (lamp == null)







            {







                lamp = new GameObject("Luxe_Hanger_Lamp");







                lamp.transform.position = new Vector3(0f, 3.48f, 0f);







                lamp.transform.rotation = Quaternion.identity;







                lamp.transform.localScale = Vector3.one;







                changed = true;







            }















            MeshFilter mf = lamp.GetComponent<MeshFilter>();







            if (mf == null) { mf = lamp.AddComponent<MeshFilter>(); changed = true; }







            if (lampMesh != null && mf.sharedMesh != lampMesh)







            {







                mf.sharedMesh = lampMesh;







                EditorUtility.SetDirty(mf);







                changed = true;







            }















            MeshRenderer mr = lamp.GetComponent<MeshRenderer>();







            if (mr == null) { mr = lamp.AddComponent<MeshRenderer>(); changed = true; }















            Material[] targetMats = new Material[] { matMetal, matCrystal, matBulb };







            bool matsMatch = mr.sharedMaterials != null && mr.sharedMaterials.Length == 3;







            if (matsMatch)







            {







                for (int i = 0; i < 3; i++)







                {







                    if (mr.sharedMaterials[i] != targetMats[i]) { matsMatch = false; break; }







                }







            }















            if (!matsMatch)







            {







                mr.sharedMaterials = targetMats;







                EditorUtility.SetDirty(mr);







                changed = true;







            }







            mr.enabled = true;















            SphereCollider sc = lamp.GetComponent<SphereCollider>();







            if (sc == null) { sc = lamp.AddComponent<SphereCollider>(); changed = true; }







            sc.radius = 0.6f;







            sc.center = new Vector3(0f, 0.1f, 0f);







            sc.enabled = true;







            EditorUtility.SetDirty(sc);















            // PointLight child







            Transform lightT = lamp.transform.Find("PointLight_WarmGlow");







            GameObject lightObj;







            if (lightT == null)







            {







                lightObj = new GameObject("PointLight_WarmGlow");







                lightObj.transform.SetParent(lamp.transform, false);







                lightObj.transform.localPosition = new Vector3(0f, 0.05f, 0f);







                changed = true;







            }







            else







            {







                lightObj = lightT.gameObject;







            }















            Light lt = lightObj.GetComponent<Light>();







            if (lt == null) { lt = lightObj.AddComponent<Light>(); changed = true; }







            lt.type = LightType.Point;







            lt.color = new Color(1.0f, 0.82f, 0.55f);







            lt.intensity = 1.8f;







            lt.range = 10f;







            lt.shadows = LightShadows.Soft;







            lt.enabled = true;







            EditorUtility.SetDirty(lt);















            lamp.SetActive(true);







            EditorUtility.SetDirty(lamp);















            if (changed)







            {







                var scene = EditorSceneManager.GetActiveScene();







                if (scene.IsValid() && scene.isLoaded)







                {







                    EditorSceneManager.MarkSceneDirty(scene);







                    EditorSceneManager.SaveScene(scene);







                    Debug.Log("[AutoOpenSceneOnLoad] Luxe_Hanger_Lamp fully textured and saved in Scene view!");







                }







            }







        }















        [MenuItem("EscapeRoom/Place Starcourt Mall In Current Scene")]







        public static void PlaceStarcourtMallInCurrentScene()







        {







            if (Application.isPlaying) return;















            GameObject existing = GameObject.Find("Starcourt_Mall_Atrium");







            if (existing == null) existing = GameObject.Find("Starcourt_Mall");















            if (existing != null)







            {







                Selection.activeGameObject = existing;







                EditorGUIUtility.PingObject(existing);







                Debug.Log("[AutoOpenSceneOnLoad] Starcourt Mall already exists in scene. Selected.");







                EnsureStarcourtMallVisible();







                return;







            }















            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Starcourt_Mall/Starcourt_Mall_Atrium.prefab");







            GameObject instance = null;







            if (prefab != null)







            {







                instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);







            }







            else







            {







                instance = new GameObject("Starcourt_Mall_Atrium");







            }















            if (instance != null)







            {







                instance.transform.position = Vector3.zero;







                instance.transform.rotation = Quaternion.identity;







                instance.transform.localScale = Vector3.one;















                EnsureStarcourtMallVisible();















                Selection.activeGameObject = instance;







                EditorGUIUtility.PingObject(instance);















                var scene = EditorSceneManager.GetActiveScene();







                if (scene.IsValid() && scene.isLoaded)







                {







                    EditorSceneManager.MarkSceneDirty(scene);







                    EditorSceneManager.SaveScene(scene);







                    Debug.Log("[AutoOpenSceneOnLoad] Placed and saved Starcourt_Mall_Atrium in " + scene.name);







                }







            }







        }















        [MenuItem("EscapeRoom/Ensure Starcourt Mall Materials Applied")]







        public static void EnsureStarcourtMallVisible()







        {







            if (Application.isPlaying) return;















            Mesh mallMesh = null;







            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath("Assets/Starcourt_Mall/source/Starcourt_Mall.obj");







            foreach (var a in allAssets)







            {







                if (a is Mesh m)







                {







                    mallMesh = m;







                    break;







                }







            }















            Material matSign = AssetDatabase.LoadAssetAtPath<Material>("Assets/Starcourt_Mall/Materials/Starcourt_Sign.mat");







            Material matPillars = AssetDatabase.LoadAssetAtPath<Material>("Assets/Starcourt_Mall/Materials/Starcourt_Pillars.mat");







            Material matStoreUpper = AssetDatabase.LoadAssetAtPath<Material>("Assets/Starcourt_Mall/Materials/Starcourt_Storefronts_Upper.mat");







            Material matStoreLower = AssetDatabase.LoadAssetAtPath<Material>("Assets/Starcourt_Mall/Materials/Starcourt_Storefronts_Lower.mat");







            Material matCarpet = AssetDatabase.LoadAssetAtPath<Material>("Assets/Starcourt_Mall/Materials/Starcourt_Floors_Carpet.mat");







            Material matTiles = AssetDatabase.LoadAssetAtPath<Material>("Assets/Starcourt_Mall/Materials/Starcourt_Floors_Tiles.mat");







            Material matRailings = AssetDatabase.LoadAssetAtPath<Material>("Assets/Starcourt_Mall/Materials/Starcourt_Railings_Escalators.mat");







            Material matTruss = AssetDatabase.LoadAssetAtPath<Material>("Assets/Starcourt_Mall/Materials/Starcourt_Roof_Truss.mat");







            Material matGlass = AssetDatabase.LoadAssetAtPath<Material>("Assets/Starcourt_Mall/Materials/Starcourt_Glass.mat");















            Material[] targetMats = new Material[] {







                matSign, matPillars, matStoreUpper, matStoreLower, matCarpet, matTiles, matRailings, matTruss, matGlass







            };















            GameObject atrium = GameObject.Find("Starcourt_Mall_Atrium");







            if (atrium == null)







            {







                atrium = GameObject.Find("Starcourt_Mall");







            }















            bool changed = false;















            if (atrium != null)







            {







                MeshFilter mf = atrium.GetComponent<MeshFilter>();







                if (mf == null) { mf = atrium.AddComponent<MeshFilter>(); changed = true; }







                if (mallMesh != null && mf.sharedMesh != mallMesh)







                {







                    mf.sharedMesh = mallMesh;







                    EditorUtility.SetDirty(mf);







                    changed = true;







                }















                MeshRenderer mr = atrium.GetComponent<MeshRenderer>();







                if (mr == null) { mr = atrium.AddComponent<MeshRenderer>(); changed = true; }















                bool matsMatch = mr.sharedMaterials != null && mr.sharedMaterials.Length == 9;







                if (matsMatch)







                {







                    for (int i = 0; i < 9; i++)







                    {







                        if (mr.sharedMaterials[i] != targetMats[i]) { matsMatch = false; break; }







                    }







                }















                if (!matsMatch)







                {







                    mr.sharedMaterials = targetMats;







                    EditorUtility.SetDirty(mr);







                    changed = true;







                }







                mr.enabled = true;















                BoxCollider bc = atrium.GetComponent<BoxCollider>();







                if (bc == null) { bc = atrium.AddComponent<BoxCollider>(); changed = true; }







                bc.size = new Vector3(36f, 13f, 32f);







                bc.center = new Vector3(0f, 6.5f, 0.5f);







                bc.enabled = true;







                EditorUtility.SetDirty(bc);















                atrium.SetActive(true);







                EditorUtility.SetDirty(atrium);















                if (changed)







                {







                    var scene = EditorSceneManager.GetActiveScene();







                    if (scene.IsValid() && scene.isLoaded)







                    {







                        EditorSceneManager.MarkSceneDirty(scene);







                        EditorSceneManager.SaveScene(scene);







                        Debug.Log("[AutoOpenSceneOnLoad] Starcourt_Mall_Atrium all 9 materials fully textured and saved in Scene view!");







                    }







                }







            }







        }















        public static void EnsureLightforgedPedestalVisible()







        {







            if (Application.isPlaying) return;















            string meshAssetPath = "Assets/Lightforged_Pedestal/Lightforged_Pedestal_Mesh.asset";







            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshAssetPath);







            if (mesh == null)







            {







                mesh = SceneDoorBootstrap.LoadLightforgedPedestalMeshFromObj();







                if (mesh != null)







                {







                    AssetDatabase.CreateAsset(mesh, meshAssetPath);







                    AssetDatabase.SaveAssets();







                    Debug.Log("[AutoOpenSceneOnLoad] Created and saved " + meshAssetPath);







                }







            }















            Material[] pedestalMats = new Material[]







            {







                AssetDatabase.LoadAssetAtPath<Material>("Assets/Lightforged_Pedestal/Materials/Lightforged_Platform_Mat.mat"),







                AssetDatabase.LoadAssetAtPath<Material>("Assets/Lightforged_Pedestal/Materials/Lightforged_Pedestal_Mat.mat"),







                AssetDatabase.LoadAssetAtPath<Material>("Assets/Lightforged_Pedestal/Materials/Lightforged_Cloth_Mat.mat"),







                AssetDatabase.LoadAssetAtPath<Material>("Assets/Lightforged_Pedestal/Materials/Lightforged_Book_Mat.mat"),







                AssetDatabase.LoadAssetAtPath<Material>("Assets/Lightforged_Pedestal/Materials/Lightforged_Parchment_Mat.mat"),







                AssetDatabase.LoadAssetAtPath<Material>("Assets/Lightforged_Pedestal/Materials/Lightforged_Feather_Mat.mat"),







                AssetDatabase.LoadAssetAtPath<Material>("Assets/Lightforged_Pedestal/Materials/Lightforged_Sigil_Mat.mat")







            };















            GameObject pedestal = GameObject.Find("Lightforged_Pedestal");







            bool changed = false;















            if (pedestal == null)







            {







                GameObject room3 = GameObject.Find("Room_3");







                pedestal = new GameObject("Lightforged_Pedestal");







                if (room3 != null)







                {







                    pedestal.transform.SetParent(room3.transform, false);







                    pedestal.transform.localPosition = new Vector3(0f, 0.02f, 0f);







                }







                else







                {







                    pedestal.transform.position = new Vector3(0f, 0.02f, 20f);







                }







                pedestal.transform.localRotation = Quaternion.identity;







                pedestal.transform.localScale = Vector3.one;







                changed = true;







            }















            MeshFilter mf = pedestal.GetComponent<MeshFilter>();







            if (mf == null) { mf = pedestal.AddComponent<MeshFilter>(); changed = true; }







            if (mesh != null && mf.sharedMesh != mesh)







            {







                mf.sharedMesh = mesh;







                EditorUtility.SetDirty(mf);







                changed = true;







            }















            MeshRenderer mr = pedestal.GetComponent<MeshRenderer>();







            if (mr == null) { mr = pedestal.AddComponent<MeshRenderer>(); changed = true; }















            bool matsMatch = mr.sharedMaterials != null && mr.sharedMaterials.Length == 7;







            if (matsMatch)







            {







                for (int i = 0; i < 7; i++)







                {







                    if (mr.sharedMaterials[i] != pedestalMats[i]) { matsMatch = false; break; }







                }







            }







            if (!matsMatch)







            {







                mr.sharedMaterials = pedestalMats;







                EditorUtility.SetDirty(mr);







                changed = true;







            }







            if (!mr.enabled) { mr.enabled = true; changed = true; }















            BoxCollider col = pedestal.GetComponent<BoxCollider>();







            if (col == null) { col = pedestal.AddComponent<BoxCollider>(); changed = true; }







            col.center = new Vector3(0f, 1.1f, 0f);







            col.size = new Vector3(2.3f, 2.2f, 2.3f);







            col.enabled = true;







            EditorUtility.SetDirty(col);















            Transform lightT = pedestal.transform.Find("HolyLight_Point");







            GameObject lightObj;







            if (lightT == null)







            {







                lightObj = new GameObject("HolyLight_Point");







                lightObj.transform.SetParent(pedestal.transform, false);







                lightObj.transform.localPosition = new Vector3(0f, 1.85f, -0.06f);







                changed = true;







            }







            else







            {







                lightObj = lightT.gameObject;







            }















            Light l = lightObj.GetComponent<Light>();







            if (l == null) { l = lightObj.AddComponent<Light>(); changed = true; }







            l.type = LightType.Point;







            l.color = new Color(1f, 0.92f, 0.7f);







            l.range = 8f;







            l.intensity = 2.5f;







            l.shadows = LightShadows.Soft;







            l.enabled = true;







            EditorUtility.SetDirty(l);















            if (!pedestal.activeSelf) { pedestal.SetActive(true); changed = true; }







            EditorUtility.SetDirty(pedestal);
            // Configure Final Vault Controller on Lightforged Pedestal (same jobs as Room 6 Pedestal)
            FinalVaultController vault = pedestal.GetComponent<FinalVaultController>();
            if (vault == null)
            {
                vault = pedestal.AddComponent<FinalVaultController>();
                changed = true;
            }

            // Check for Door_14_Venge (replaces Door_Final for Room 6 final validation)
            GameObject door14Obj = GameObject.Find("Door_14_Venge");
            if (door14Obj == null) door14Obj = GameObject.Find("Door_14");
            if (door14Obj == null)
            {
                foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    if (root.name.IndexOf("Door_14", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        door14Obj = root;
                        break;
                    }
                }
            }

            GameObject doorFinalObj = GameObject.Find("Door_Final");
            if (doorFinalObj == null)
            {
                GameObject room6 = GameObject.Find("Room_6");
                if (room6 != null)
                {
                    Transform t = room6.transform.Find("Door_Final");
                    if (t != null) doorFinalObj = t.gameObject;
                }
            }

            if (door14Obj == null)
            {
                var prefab14 = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Free Wood Door Pack/Prefab/Wood/Door_14/Door_14_Venge.prefab");
                if (prefab14 != null)
                {
                    door14Obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab14);
                    door14Obj.name = "Door_14_Venge";
                    GameObject room6 = GameObject.Find("Room_6");
                    if (room6 != null)
                    {
                        door14Obj.transform.SetParent(room6.transform, false);
                        if (doorFinalObj != null)
                        {
                            door14Obj.transform.localPosition = new Vector3(doorFinalObj.transform.localPosition.x, 0.05f, doorFinalObj.transform.localPosition.z);
                            door14Obj.transform.localRotation = Quaternion.identity;
                        }
                        else
                        {
                            door14Obj.transform.localPosition = new Vector3(0f, 0.05f, 4.85f);
                            door14Obj.transform.localRotation = Quaternion.identity;
                        }
                    }
                    else if (doorFinalObj != null)
                    {
                        door14Obj.transform.position = new Vector3(doorFinalObj.transform.position.x, 0.05f, doorFinalObj.transform.position.z);
                        door14Obj.transform.rotation = doorFinalObj.transform.rotation;
                    }
                    changed = true;
                }
            }

            if (door14Obj != null)
            {
                door14Obj.SetActive(true);
                Transform doorLeaf = door14Obj.transform.Find("Door");
                if (doorLeaf == null)
                {
                    foreach (Transform child in door14Obj.transform)
                    {
                        if (child.name.Equals("Door", System.StringComparison.OrdinalIgnoreCase))
                        {
                            doorLeaf = child;
                            break;
                        }
                    }
                }

                if (doorLeaf != null)
                {
                    // Disable Free Wood Door Pack direct mouse click script
                    MonoBehaviour[] mbs = doorLeaf.GetComponents<MonoBehaviour>();
                    for (int i = 0; i < mbs.Length; i++)
                    {
                        if (mbs[i] != null && mbs[i].GetType().Name == "Door")
                        {
                            mbs[i].enabled = false;
                        }
                    }

                    BoxCollider leafCol = doorLeaf.GetComponent<BoxCollider>();
                    if (leafCol == null) leafCol = doorLeaf.gameObject.AddComponent<BoxCollider>();
                    leafCol.enabled = true;
                }

                MonoBehaviour[] rootMbs = door14Obj.GetComponents<MonoBehaviour>();
                for (int i = 0; i < rootMbs.Length; i++)
                {
                    if (rootMbs[i] != null && rootMbs[i].GetType().Name == "Door")
                    {
                        rootMbs[i].enabled = false;
                    }
                }

                AREscapeRoom.Doors.DoorController dc14 = door14Obj.GetComponent<AREscapeRoom.Doors.DoorController>();
                if (dc14 == null) dc14 = door14Obj.AddComponent<AREscapeRoom.Doors.DoorController>();
                dc14.ConfigureHinge(doorLeaf, 0f, -90f, true, "Door_14_Venge");

                AREscapeRoom.Doors.ProgressionDoor pd14 = door14Obj.GetComponent<AREscapeRoom.Doors.ProgressionDoor>();
                if (pd14 == null) pd14 = door14Obj.AddComponent<AREscapeRoom.Doors.ProgressionDoor>();
                pd14.Bind(dc14, AREscapeRoom.Core.DoorRequirement.RequiresFinalValidation, false, 6);

                vault.Bind(pd14);
                EditorUtility.SetDirty(vault);

                if (doorFinalObj != null && doorFinalObj != door14Obj)
                {
                    doorFinalObj.SetActive(false);
                }
            }
            else if (doorFinalObj != null)
            {
                AREscapeRoom.Doors.ProgressionDoor pd = doorFinalObj.GetComponent<AREscapeRoom.Doors.ProgressionDoor>();
                if (pd != null)
                {
                    vault.Bind(pd);
                    EditorUtility.SetDirty(vault);
                }
            }

            // Ensure KeySlot and 5 GemSlots exist on Lightforged Pedestal
            if (pedestal.transform.Find("KeySlot") == null)
            {
                GameObject keySlot = GameObject.CreatePrimitive(PrimitiveType.Cube);
                keySlot.name = "KeySlot";
                keySlot.transform.SetParent(pedestal.transform, false);
                keySlot.transform.localPosition = new Vector3(0f, 1.82f, -0.05f);
                keySlot.transform.localScale = new Vector3(0.25f, 0.08f, 0.5f);
                var mrKey = keySlot.GetComponent<MeshRenderer>();
                if (mrKey != null)
                {
                    mrKey.sharedMaterial = new Material(Shader.Find("Standard")) { color = new Color(0.95f, 0.85f, 0.25f) };
                }
                changed = true;
            }

            for (int i = 0; i < 5; i++)
            {
                string slotName = $"GemSlot_{i}";
                if (pedestal.transform.Find(slotName) == null)
                {
                    GameObject gemSlot = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    gemSlot.name = slotName;
                    gemSlot.transform.SetParent(pedestal.transform, false);
                    gemSlot.transform.localPosition = new Vector3(-0.4f + i * 0.2f, 1.82f, 0.25f);
                    gemSlot.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
                    var mrGem = gemSlot.GetComponent<MeshRenderer>();
                    if (mrGem != null)
                    {
                        Material mat = new Material(Shader.Find("Standard")) { color = new Color(0.1f, 0.8f, 1f) };
                        mat.EnableKeyword("_EMISSION");
                        mat.SetColor("_EmissionColor", new Color(0f, 0.5f, 1f));
                        mrGem.sharedMaterial = mat;
                    }
                    changed = true;
                }
            }
















            if (changed)







            {







                var scene = EditorSceneManager.GetActiveScene();







                if (scene.IsValid() && scene.isLoaded)







                {







                    EditorSceneManager.MarkSceneDirty(scene);







                    EditorSceneManager.SaveScene(scene);







                    Debug.Log("[AutoOpenSceneOnLoad] Lightforged_Pedestal fully meshed, textured, lit, and saved in Scene view!");







                }







            }







        }















        public static void EnsureDeskTableWhiteVisible()







        {







            if (Application.isPlaying) return;















            GameObject desk = GameObject.Find("Desk Table White");







            bool changed = false;















            if (desk == null)







            {







                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Desk Table/Desk Table White.prefab");







                if (prefab != null)







                {







                    desk = (GameObject)PrefabUtility.InstantiatePrefab(prefab);







                    desk.name = "Desk Table White";







                    desk.transform.position = new Vector3(-3.2f, 0.02f, -19.6f);







                    desk.transform.rotation = Quaternion.Euler(0f, 90f, 0f);







                    desk.transform.localScale = Vector3.one;







                    changed = true;







                }







            }







            else







            {







                Vector3 targetPos = new Vector3(-3.2f, 0.02f, -19.6f);







                Quaternion targetRot = Quaternion.Euler(0f, 90f, 0f);







                // Respect manual editor layout for existing desk:
                // if (Vector3.Distance(desk.transform.position, targetPos) > 0.05f || desk.transform.position.y < 0f) { desk.transform.position = targetPos; desk.transform.rotation = targetRot; desk.transform.localScale = Vector3.one; EditorUtility.SetDirty(desk.transform); changed = true; }







            }















            if (desk != null)







            {







                if (!desk.activeSelf) { desk.SetActive(true); changed = true; }















                Mesh deskMesh = null;







                Mesh drawerMesh = null;







                Object[] deskAssets = AssetDatabase.LoadAllAssetsAtPath("Assets/Models/Desk Table/Models/desk_table.fbx");







                foreach (var a in deskAssets)







                {







                    if (a is Mesh m)







                    {







                        if (m.name == "desk_body") deskMesh = m;







                        else if (m.name == "desk_drawer") drawerMesh = m;







                    }







                }















                Material deskMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Models/Desk Table/Models/Materials/desk_table1.mat");















                Transform bodyT = desk.transform.Find("desk_body");

                if (bodyT == null)

                {

                    GameObject bodyObj = new GameObject("desk_body");

                    bodyObj.transform.SetParent(desk.transform, false);

                    bodyObj.transform.localPosition = new Vector3(0f, 0.4258374f, 0.017870665f);

                    bodyObj.transform.localRotation = Quaternion.identity;

                    bodyObj.transform.localScale = Vector3.one;

                    bodyT = bodyObj.transform;

                    changed = true;

                }



                if (bodyT != null)

                {

                    bodyT.gameObject.SetActive(true);

                    MeshFilter mf = bodyT.GetComponent<MeshFilter>();

                    if (mf == null) { mf = bodyT.gameObject.AddComponent<MeshFilter>(); changed = true; }

                    if (deskMesh != null && mf.sharedMesh != deskMesh)

                    {

                        mf.sharedMesh = deskMesh;

                        EditorUtility.SetDirty(mf);

                        changed = true;

                    }

                    MeshRenderer mr = bodyT.GetComponent<MeshRenderer>();

                    if (mr == null) { mr = bodyT.gameObject.AddComponent<MeshRenderer>(); changed = true; }

                    mr.enabled = true;

                    if (deskMat != null && mr.sharedMaterial != deskMat)

                    {

                        mr.sharedMaterial = deskMat;

                        EditorUtility.SetDirty(mr);

                        changed = true;

                    }

                }



                Transform drawerT = desk.transform.Find("desk_drawer");

                if (drawerT == null)

                {

                    GameObject drawerObj = new GameObject("desk_drawer");

                    drawerObj.transform.SetParent(desk.transform, false);

                    drawerObj.transform.localPosition = new Vector3(-0.70544314f, 0.69567585f, 0.509279f);

                    drawerObj.transform.localRotation = Quaternion.identity;

                    drawerObj.transform.localScale = Vector3.one;

                    drawerT = drawerObj.transform;

                    changed = true;

                }



                if (drawerT != null)

                {

                    drawerT.gameObject.SetActive(true);

                    MeshFilter mf = drawerT.GetComponent<MeshFilter>();

                    if (mf == null) { mf = drawerT.gameObject.AddComponent<MeshFilter>(); changed = true; }

                    if (drawerMesh != null && mf.sharedMesh != drawerMesh)

                    {

                        mf.sharedMesh = drawerMesh;

                        EditorUtility.SetDirty(mf);

                        changed = true;

                    }

                    MeshRenderer mr = drawerT.GetComponent<MeshRenderer>();

                    if (mr == null) { mr = drawerT.gameObject.AddComponent<MeshRenderer>(); changed = true; }

                    mr.enabled = true;

                    if (deskMat != null && mr.sharedMaterial != deskMat)

                    {

                        mr.sharedMaterial = deskMat;

                        EditorUtility.SetDirty(mr);

                        changed = true;

                    }



                    BoxCollider drawerCol = drawerT.GetComponent<BoxCollider>();

                    if (drawerCol == null)

                    {

                        drawerCol = drawerT.gameObject.AddComponent<BoxCollider>();

                        drawerCol.size = new Vector3(0.40f, 0.22f, 0.06f);

                        drawerCol.center = new Vector3(0.01f, 0.11f, 0.02f);

                        EditorUtility.SetDirty(drawerCol);

                        changed = true;

                    }



                    DeskTableWhiteDrawerController drawerCtrl = drawerT.GetComponent<DeskTableWhiteDrawerController>();

                    if (drawerCtrl == null)

                    {

                        drawerCtrl = drawerT.gameObject.AddComponent<DeskTableWhiteDrawerController>();

                        EditorUtility.SetDirty(drawerCtrl);

                        changed = true;

                    }



                    // Place Stylized_Key inside desk_drawer

                    Transform keyT = drawerT.Find("Stylized_Key");

                    if (keyT == null)

                    {

                        GameObject keyObj = new GameObject("Stylized_Key");

                        keyObj.transform.SetParent(drawerT, false);

                        keyT = keyObj.transform;

                        changed = true;

                    }



                    Vector3 keyTargetPos = new Vector3(0.009f, 0.065f, -0.15f);

                    Quaternion keyTargetRot = Quaternion.Euler(0f, 90f, 0f);

                    Vector3 keyTargetScale = Vector3.one;



                    if (Vector3.Distance(keyT.localPosition, keyTargetPos) > 0.001f ||

                        Quaternion.Angle(keyT.localRotation, keyTargetRot) > 0.1f ||

                        Vector3.Distance(keyT.localScale, keyTargetScale) > 0.0001f)

                    {

                        keyT.localPosition = keyTargetPos;

                        keyT.localRotation = keyTargetRot;

                        keyT.localScale = keyTargetScale;

                        EditorUtility.SetDirty(keyT);

                        changed = true;

                    }



                    MeshFilter keyMf = keyT.GetComponent<MeshFilter>();

                    if (keyMf == null) { keyMf = keyT.gameObject.AddComponent<MeshFilter>(); changed = true; }

                    Mesh keyMesh = null;

                    Object[] keyAssets = AssetDatabase.LoadAllAssetsAtPath("Assets/Stylized_Key/Stylized_Key.fbx");

                    foreach (var ka in keyAssets)

                    {

                        if (ka is Mesh km) { keyMesh = km; break; }

                    }

                    if (keyMesh != null && keyMf.sharedMesh != keyMesh)

                    {

                        keyMf.sharedMesh = keyMesh;

                        EditorUtility.SetDirty(keyMf);

                        changed = true;

                    }



                    MeshRenderer keyMr = keyT.GetComponent<MeshRenderer>();

                    if (keyMr == null) { keyMr = keyT.gameObject.AddComponent<MeshRenderer>(); changed = true; }

                    Material p1 = AssetDatabase.LoadAssetAtPath<Material>("Assets/Stylized_Key/Materials/phong1SG.mat");

                    Material pE1 = AssetDatabase.LoadAssetAtPath<Material>("Assets/Stylized_Key/Materials/phongE1SG.mat");

                    if (p1 != null && pE1 != null)

                    {

                        Material[] mats = new Material[] { p1, pE1 };

                        if (keyMr.sharedMaterials == null || keyMr.sharedMaterials.Length != 2 || keyMr.sharedMaterials[0] != p1 || keyMr.sharedMaterials[1] != pE1)

                        {

                            keyMr.sharedMaterials = mats;

                            EditorUtility.SetDirty(keyMr);

                            changed = true;

                        }

                    }



                    BoxCollider keyCol = keyT.GetComponent<BoxCollider>();

                    if (keyCol == null)

                    {

                        keyCol = keyT.gameObject.AddComponent<BoxCollider>();

                        keyCol.size = new Vector3(0.12f, 0.08f, 0.25f);

                        keyCol.center = Vector3.zero;

                        EditorUtility.SetDirty(keyCol);

                        changed = true;

                    }



                    
                    Light keyLight = keyT.GetComponent<Light>();
                    if (keyLight == null)
                    {
                        keyLight = keyT.gameObject.AddComponent<Light>();
                        keyLight.type = LightType.Point;
                        keyLight.color = new Color(1f, 0.85f, 0.45f);
                        keyLight.range = 0.75f;
                        keyLight.intensity = 1.2f;
                        EditorUtility.SetDirty(keyLight);
                        changed = true;
                    }

                    KeyPickup pickup = keyT.GetComponent<KeyPickup>();

                    if (pickup == null)

                    {

                        pickup = keyT.gameObject.AddComponent<KeyPickup>();

                        EditorUtility.SetDirty(pickup);

                        changed = true;

                    }



                    if (drawerCtrl != null)

                    {

                        drawerCtrl.BindKey(pickup);

                    }

                }



                EditorUtility.SetDirty(desk);







            }















            if (changed)







            {







                var scene = EditorSceneManager.GetActiveScene();







                if (scene.IsValid() && scene.isLoaded)







                {







                    EditorSceneManager.MarkSceneDirty(scene);







                    EditorSceneManager.SaveScene(scene);







                    Debug.Log("[AutoOpenSceneOnLoad] Desk Table White repositioned, verified, and saved in Scene view!");







                }







            }







        }







    



        [MenuItem("EscapeRoom/Attach Caution Frame to Door Leaf")]



        public static void EnsureCautionFrameAttachedToDoor()



        {



            if (Application.isPlaying) return;







            GameObject doorObj = GameObject.Find("Door_5_Brown");



            if (doorObj == null) doorObj = GameObject.Find("Door_5_Room");



            if (doorObj == null)



            {



                var allGos = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);



                for (int i = 0; i < allGos.Length; i++)



                {



                    if (allGos[i] != null && (allGos[i].name.Contains("Door_5") || allGos[i].name.Contains("Brown")))



                    {



                        doorObj = allGos[i];



                        break;



                    }



                }


            }







            if (doorObj == null) return;







            Transform doorLeaf = doorObj.transform.Find("Door");



            if (doorLeaf == null) doorLeaf = doorObj.transform.Find("Door_5_Room");



            if (doorLeaf == null)



            {



                for (int i = 0; i < doorObj.transform.childCount; i++)



                {



                    Transform child = doorObj.transform.GetChild(i);



                    if (child.name.Equals("Door", System.StringComparison.OrdinalIgnoreCase) || child.name.Contains("Door_5_Room"))



                    {



                        doorLeaf = child;



                        break;



                    }



                }



            }







            Transform targetParent = doorLeaf != null ? doorLeaf : doorObj.transform;







            GameObject caution = GameObject.Find("Caution_Frame");



            if (caution == null)



            {



                var allGos = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);



                for (int i = 0; i < allGos.Length; i++)



                {



                    if (allGos[i] != null && allGos[i].name.StartsWith("Caution_Frame", System.StringComparison.OrdinalIgnoreCase))



                    {



                        caution = allGos[i];



                        break;



                    }



                }



            }







            if (caution != null && targetParent != null)



            {



                if (caution.transform.parent != targetParent)



                {



                    Undo.SetTransformParent(caution.transform, targetParent, "Stick Caution_Frame to Door Leaf");



                    EditorUtility.SetDirty(caution);



                    if (!Application.isPlaying && caution.scene.isLoaded)



                    {



                        EditorSceneManager.MarkSceneDirty(caution.scene);



                    }



                    Debug.Log($"[AutoOpenSceneOnLoad] Successfully attached Caution_Frame to moving door leaf '{targetParent.name}' of '{doorObj.name}'!");



                }



            }



        }




        [MenuItem("EscapeRoom/Setup Room 5 Shapes and Sockets")]
        public static void SetupRoom5ShapesAndSockets()
        {
            GameObject room5 = GameObject.Find("Room_5");
            if (room5 == null)
            {
                foreach (var r in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    Transform t = r.transform.Find("Room_5");
                    if (t != null) { room5 = t.gameObject; break; }
                }
            }

            // 1. Deactivate old primitive placeholder shapes inside Room_5 and in scene
            // Deactivate old procedural placeholder shapes if present
            string[] primitiveNames = new string[] { "StarCore", "StarCross", "Shape_Circle", "Shape_Triangle", "Shape_Star", "Shape_Square" };
            foreach (var name in primitiveNames)
            {
                if (room5 != null)
                {
                    Transform prim = room5.transform.Find(name);
                    if (prim != null) prim.gameObject.SetActive(false);
                }
                GameObject obj = GameObject.Find(name);
                if (obj != null) obj.SetActive(false);
            }

            // Deactivate procedural Door_R5_R6 if present
            GameObject doorR5 = GameObject.Find("Door_R5_R6");
            if (doorR5 != null) doorR5.SetActive(false);

            var puzzle = room5 != null ? room5.GetComponent<AREscapeRoom.Puzzles.ShapePlacementPuzzle>() : null;
            if (puzzle == null && room5 != null)
            {
                puzzle = room5.AddComponent<AREscapeRoom.Puzzles.ShapePlacementPuzzle>();
            }

            // 2. Configure the 4 3D geometry objects: Traingle_Shape, Star_Shape, Square_Shape, Circle_Shape
            var shapeConfigs = new (string[] names, AREscapeRoom.Puzzles.ShapeType type, Vector3 defaultPos)[]
            {
                (new string[] { "Traingle_shape", "Traingle_Shape", "Triangle_Shape", "triangle_shape" }, AREscapeRoom.Puzzles.ShapeType.Triangle, new Vector3(-1.5f, 0.22f, 0.0f)),
                (new string[] { "Star_Shape", "star_shape", "StarShape" }, AREscapeRoom.Puzzles.ShapeType.Star, new Vector3(-0.5f, 0.22f, 0.0f)),
                (new string[] { "Square_Shape", "square_shape", "SquareShape" }, AREscapeRoom.Puzzles.ShapeType.Square, new Vector3(0.5f, 0.22f, 0.0f)),
                (new string[] { "Circle_Shape", "circle_shape", "CircleShape" }, AREscapeRoom.Puzzles.ShapeType.Circle, new Vector3(1.5f, 0.22f, 0.0f))
            };

            foreach (var cfg in shapeConfigs)
            {
                GameObject shapeObj = null;
                if (room5 != null)
                {
                    foreach (Transform child in room5.transform)
                    {
                        foreach (var n in cfg.names)
                        {
                            if (string.Equals(child.gameObject.name, n, System.StringComparison.OrdinalIgnoreCase))
                            {
                                shapeObj = child.gameObject;
                                break;
                            }
                        }
                        if (shapeObj != null) break;
                    }
                }

                if (shapeObj == null)
                {
                    foreach (var n in cfg.names)
                    {
                        shapeObj = GameObject.Find(n);
                        if (shapeObj != null) break;
                    }
                }

                if (shapeObj != null)
                {
                    shapeObj.SetActive(true);
                    var shapeComp = shapeObj.GetComponent<AREscapeRoom.Puzzles.ShapeObject>();
                    if (shapeComp == null)
                    {
                        shapeComp = shapeObj.AddComponent<AREscapeRoom.Puzzles.ShapeObject>();
                    }
                    shapeComp.Bind(cfg.type);

                    // Ground the shape on the floor in front of the player if not already placed
                    // Respect manual editor layout for Room 5 shapes:
                    // if (!shapeComp.Placed && (shapeObj.transform.parent == null || (room5 != null && shapeObj.transform.parent == room5.transform))) { shapeObj.transform.localPosition = cfg.defaultPos; shapeObj.transform.localRotation = Quaternion.identity; shapeObj.transform.localScale = Vector3.one; }

                    shapeComp.EnsureCollidersAndPhysics();
                    Debug.Log($"Successfully initialized 3D shape: {shapeObj.name} as {cfg.type}");
                }
                else
                {
                    Debug.LogWarning($"Could not find shape object for {cfg.type}!");
                }
            }

            // 3. Configure 4 Sockets & Labels
            var socketConfigs = new (string[] names, AREscapeRoom.Puzzles.ShapeType type, string labelName)[]
            {
                (new string[] { "Socket_Triangle", "Socket_Traingle" }, AREscapeRoom.Puzzles.ShapeType.Triangle, "SocketLabel_Triangle"),
                (new string[] { "Socket_Star" }, AREscapeRoom.Puzzles.ShapeType.Star, "SocketLabel_Star"),
                (new string[] { "Socket_Square" }, AREscapeRoom.Puzzles.ShapeType.Square, "SocketLabel_Square"),
                (new string[] { "Socket_Circle" }, AREscapeRoom.Puzzles.ShapeType.Circle, "SocketLabel_Circle")
            };

            foreach (var sc in socketConfigs)
            {
                GameObject sockObj = null;
                foreach (var n in sc.names)
                {
                    sockObj = GameObject.Find(n);
                    if (sockObj != null) break;
                }

                if (sockObj != null)
                {
                    sockObj.SetActive(true);
                    var sockComp = sockObj.GetComponent<AREscapeRoom.Puzzles.ShapeSocket>();
                    if (sockComp == null)
                    {
                        sockComp = sockObj.AddComponent<AREscapeRoom.Puzzles.ShapeSocket>();
                    }
                    sockComp.Bind(sc.type, puzzle);

                    Collider sockCol = sockObj.GetComponent<Collider>();
                    if (sockCol == null)
                    {
                        BoxCollider bc = sockObj.AddComponent<BoxCollider>();
                        bc.size = new Vector3(1f, 1f, 0.4f);
                        bc.isTrigger = false;
                    }
                    else
                    {
                        sockCol.enabled = true;
                    }
                }

                GameObject lblObj = GameObject.Find(sc.labelName);
                if (lblObj == null && sc.type == AREscapeRoom.Puzzles.ShapeType.Triangle)
                {
                    lblObj = GameObject.Find("SocketLabel_Traingle");
                }
                if (lblObj != null)
                {
                    lblObj.SetActive(true);
                }
            }

            Debug.Log("[AutoOpenSceneOnLoad] Successfully configured Room 5 shapes, sockets, and labels.");
        }

        [MenuItem("EscapeRoom/Reset Player to Start Position")]
        [MenuItem("EscapeRoom/Recreate Player and Main Camera in Front of AR_Escape_Room_Structure")]
        public static void EnsurePlayerAtStartPosition()
        {
            GameObject oldPlayer = GameObject.Find("Player");
            if (oldPlayer != null)
            {
                Undo.DestroyObjectImmediate(oldPlayer);
            }
            GameObject oldCam = GameObject.Find("Main Camera");
            if (oldCam != null)
            {
                Undo.DestroyObjectImmediate(oldCam);
            }

            GameObject door = GameObject.Find("Door_5_Brown");
            if (door == null) door = GameObject.Find("Door_5_Room");
            GameObject room = GameObject.Find("AR_Escape_Room_Structure");

            Vector3 spawnPos;
            if (door != null)
            {
                spawnPos = new Vector3(door.transform.position.x, 0.05f, door.transform.position.z - 2.8f);
            }
            else if (room != null)
            {
                spawnPos = new Vector3(room.transform.position.x + 0.1f, 0.05f, room.transform.position.z - 8.4f);
            }
            else
            {
                spawnPos = new Vector3(0.109f, 0.05f, -28.459f);
            }

            GameObject player = new GameObject("Player");
            Undo.RegisterCreatedObjectUndo(player, "Create Player");
            player.tag = "Player";
            player.layer = 0;
            player.transform.position = spawnPos;
            player.transform.rotation = Quaternion.identity;
            player.transform.localScale = Vector3.one;

            CharacterController cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.3f;
            cc.center = new Vector3(0f, 0.9f, 0f);
            cc.slopeLimit = 45f;
            cc.stepOffset = 0.3f;
            cc.skinWidth = 0.08f;
            cc.minMoveDistance = 0.001f;

            GameObject camObj = new GameObject("Main Camera");
            Undo.RegisterCreatedObjectUndo(camObj, "Create Main Camera");
            camObj.tag = "MainCamera";
            camObj.layer = 0;
            camObj.transform.SetParent(player.transform, false);
            camObj.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            camObj.transform.localRotation = Quaternion.identity;
            camObj.transform.localScale = Vector3.one;

            Camera cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.cullingMask = ~0;
            cam.fieldOfView = 60f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 120f;
            cam.depth = 100f;

            camObj.AddComponent<AudioListener>();

            PlayerController pc = player.AddComponent<PlayerController>();
            pc.AssignLookCamera(camObj.transform);

            player.AddComponent<PlayerInteraction>();

            EditorUtility.SetDirty(player);
            EditorUtility.SetDirty(camObj);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log($"[AutoOpenSceneOnLoad] Fresh Player and Main Camera recreated in front of AR_Escape_Room_Structure at {spawnPos}!");
        }
    }
}
