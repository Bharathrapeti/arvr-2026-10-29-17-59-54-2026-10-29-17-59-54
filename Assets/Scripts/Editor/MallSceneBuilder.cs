using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using AREscapeRoom.Player;
using AREscapeRoom.Core;
using AREscapeRoom.UI;
using AREscapeRoom.Doors;

public class MallSceneBuilder : EditorWindow
{
    [MenuItem("AR Escape Room/1. Build Mall Scene")]
    public static void BuildScene()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("Stop Play Mode before building the mall scene.");
            return;
        }

        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        string modelPath = "Assets/MallOfMoscow/source/MALL.fbx";

        // Fix Import Settings to ensure Colliders work and textures try to link
        ModelImporter importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
        if (importer != null)
        {
            bool reimport = false;
            if (!importer.isReadable) { importer.isReadable = true; reimport = true; }
            if (importer.materialImportMode == ModelImporterMaterialImportMode.None) { importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard; reimport = true; }
            if (reimport) importer.SaveAndReimport();
        }

        GameObject mallPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        if (mallPrefab == null)
        {
            Debug.LogError("Could not find MALL.fbx. Please check the path.");
            return;
        }

        // 1. Solid Ground Plane
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "SolidGround";
        ground.transform.position = new Vector3(0, -0.01f, 0);
        ground.transform.localScale = new Vector3(100, 1, 100); // 1000x1000 meters
        Material groundMat = new Material(Shader.Find("Standard"));
        groundMat.color = new Color(0.2f, 0.2f, 0.2f);
        ground.GetComponent<MeshRenderer>().sharedMaterial = groundMat;

        // 2. Instantiate Mall
        GameObject mallInstance = (GameObject)PrefabUtility.InstantiatePrefab(mallPrefab);
        mallInstance.name = "Mall_Environment";
        mallInstance.transform.position = Vector3.zero;

        // Auto-fix mall position so it sits on the ground
        Bounds mallBounds = AdjustMallToGround(mallInstance);

        // Automatically assign textures if they exist
        AssignTexturesToMall(mallInstance);

        // Add colliders to Mall
        MeshFilter[] meshFilters = mallInstance.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter mf in meshFilters)
        {
            if (mf.gameObject.GetComponent<MeshCollider>() == null)
            {
                mf.gameObject.AddComponent<MeshCollider>();
            }
        }

        // 3. Setup Player
        SetupPlayer(new Vector3(mallBounds.center.x, 0.1f, mallBounds.min.z - 8f));

        // 4. Setup Escape Room Structure
        DoorController entranceDC;
        DoorController exitDC;
        GameObject roomGroup = SetupEscapeRoomStructure(mallBounds, out entranceDC, out exitDC);

        // 5. Cut mall walls inside Escape Room footprint to ensure no blocking mesh or colliders
        Bounds roomInteriorBounds = new Bounds(roomGroup.transform.position + new Vector3(0, 2.5f, 0), new Vector3(8.5f, 4.5f, 8.5f));
        CutoutMallForEscapeRoom(mallInstance, roomInteriorBounds);

        // 6. Setup Game Manager & UI
        SetupGameLogic(entranceDC, exitDC);

        string scenePath = "Assets/Scenes/RealisticMallScene.unity";
        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        EditorSceneManager.MarkSceneDirty(newScene);
        EditorSceneManager.SaveScene(newScene, scenePath);
        
        Debug.Log("Scene built successfully!");
    }

    private static void AssignTexturesToMall(GameObject mall)
    {
        string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/MallOfMoscow/textures" });
        if (textureGuids.Length == 0) return;

        Texture2D defaultTex = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(textureGuids[0]));
        
        Renderer[] renderers = mall.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            if (r.sharedMaterials == null) continue;
            Material[] mats = new Material[r.sharedMaterials.Length];
            for (int i = 0; i < r.sharedMaterials.Length; i++)
            {
                Material original = r.sharedMaterials[i];
                if (original == null) continue;

                // Create a unique material instance so we can modify it
                Material newMat = new Material(Shader.Find("Standard"));
                newMat.name = original.name;
                
                // Try to find a matching texture by name
                Texture2D matchedTex = defaultTex;
                foreach (string guid in textureGuids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path.ToLower().Contains(original.name.ToLower()))
                    {
                        matchedTex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                        break;
                    }
                }
                
                newMat.mainTexture = matchedTex;
                mats[i] = newMat;
            }
            r.sharedMaterials = mats;
        }
    }

    private static Bounds AdjustMallToGround(GameObject mall)
    {
        Renderer[] renderers = mall.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(Vector3.zero, Vector3.one);

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        float yOffset = -bounds.min.y;
        mall.transform.position += new Vector3(0, yOffset, 0);
        
        // Recalculate bounds after moving
        bounds.center += new Vector3(0, yOffset, 0);
        
        Debug.Log($"Adjusted Mall Y Position by {yOffset} to sit exactly on the ground.");
        return bounds;
    }

    private static void CutoutMallForEscapeRoom(GameObject mallInstance, Bounds roomInteriorBounds)
    {
        MeshFilter[] meshFilters = mallInstance.GetComponentsInChildren<MeshFilter>();
        int cutCount = 0;

        foreach (MeshFilter mf in meshFilters)
        {
            Mesh originalMesh = mf.sharedMesh;
            if (originalMesh == null) continue;

            Renderer r = mf.GetComponent<Renderer>();
            if (r == null || !r.bounds.Intersects(roomInteriorBounds)) continue;

            Transform tr = mf.transform;
            Mesh cutMesh = Object.Instantiate(originalMesh);
            cutMesh.name = originalMesh.name + "_Cutout";

            Vector3[] vertices = cutMesh.vertices;
            Vector3[] worldVerts = new Vector3[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                worldVerts[i] = tr.TransformPoint(vertices[i]);
            }

            bool meshModified = false;
            int subMeshCount = cutMesh.subMeshCount;

            for (int sub = 0; sub < subMeshCount; sub++)
            {
                int[] triangles = cutMesh.GetTriangles(sub);
                System.Collections.Generic.List<int> newTriangles = new System.Collections.Generic.List<int>();

                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int i0 = triangles[i];
                    int i1 = triangles[i + 1];
                    int i2 = triangles[i + 2];

                    Vector3 w0 = worldVerts[i0];
                    Vector3 w1 = worldVerts[i1];
                    Vector3 w2 = worldVerts[i2];

                    // Drop triangle if any vertex is inside the interior room bounds
                    bool inside0 = roomInteriorBounds.Contains(w0);
                    bool inside1 = roomInteriorBounds.Contains(w1);
                    bool inside2 = roomInteriorBounds.Contains(w2);

                    if (inside0 || inside1 || inside2)
                    {
                        meshModified = true;
                        continue;
                    }

                    newTriangles.Add(i0);
                    newTriangles.Add(i1);
                    newTriangles.Add(i2);
                }

                cutMesh.SetTriangles(newTriangles.ToArray(), sub);
            }

            if (meshModified)
            {
                cutMesh.RecalculateBounds();
                cutMesh.RecalculateNormals();
                mf.sharedMesh = cutMesh;

                MeshCollider mc = mf.GetComponent<MeshCollider>();
                if (mc != null)
                {
                    Object.DestroyImmediate(mc);
                }
                mc = mf.gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = cutMesh;

                cutCount++;
                Debug.Log($"[MallSceneBuilder] Cut mall mesh wall '{mf.gameObject.name}' to clear Escape Room interior space.");
            }
        }
    }

    private static GameObject SetupEscapeRoomStructure(Bounds mallBounds, out DoorController entranceDC, out DoorController exitDC)
    {
        // Creates a physical room inside the front of the mall
        GameObject roomGroup = new GameObject("AR_Escape_Room_Structure");
        
        // Place room so the front entrance wall (local z = -5) aligns flush with the front facade (min.z)
        // and the body of the room (0 to +5) extends INSIDE the mall building.
        roomGroup.transform.position = new Vector3(mallBounds.center.x, 0, mallBounds.min.z + 5f); 

        Material wallMat = new Material(Shader.Find("Standard"));
        wallMat.color = new Color(0.1f, 0.1f, 0.2f); // Dark blue theme

        GameObject backWallL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backWallL.name = "Wall_Back_Left";
        backWallL.transform.SetParent(roomGroup.transform);
        backWallL.transform.localPosition = new Vector3(-3.05f, 2.5f, 5f);
        backWallL.transform.localScale = new Vector3(3.9f, 5f, 1f);
        backWallL.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

        GameObject backWallR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backWallR.name = "Wall_Back_Right";
        backWallR.transform.SetParent(roomGroup.transform);
        backWallR.transform.localPosition = new Vector3(3.05f, 2.5f, 5f);
        backWallR.transform.localScale = new Vector3(3.9f, 5f, 1f);
        backWallR.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

        GameObject backWallTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backWallTop.name = "Wall_Back_Top";
        backWallTop.transform.SetParent(roomGroup.transform);
        backWallTop.transform.localPosition = new Vector3(0f, 4f, 5f);
        backWallTop.transform.localScale = new Vector3(2.2f, 2f, 1f);
        backWallTop.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

        GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftWall.name = "Wall_Left";
        leftWall.transform.SetParent(roomGroup.transform);
        leftWall.transform.localPosition = new Vector3(-5f, 2.5f, 0);
        leftWall.transform.localScale = new Vector3(1, 5, 10);
        leftWall.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

        GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightWall.name = "Wall_Right";
        rightWall.transform.SetParent(roomGroup.transform);
        rightWall.transform.localPosition = new Vector3(5f, 2.5f, 0);
        rightWall.transform.localScale = new Vector3(1, 5, 10);
        rightWall.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

        GameObject frontWallL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frontWallL.name = "Wall_Front_Left";
        frontWallL.transform.SetParent(roomGroup.transform);
        frontWallL.transform.localPosition = new Vector3(-3.5f, 2.5f, -5f);
        frontWallL.transform.localScale = new Vector3(3, 5, 1);
        frontWallL.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

        GameObject frontWallR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frontWallR.name = "Wall_Front_Right";
        frontWallR.transform.SetParent(roomGroup.transform);
        frontWallR.transform.localPosition = new Vector3(3.5f, 2.5f, -5f);
        frontWallR.transform.localScale = new Vector3(3, 5, 1);
        frontWallR.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

        GameObject frontWallTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frontWallTop.name = "Wall_Front_Top";
        frontWallTop.transform.SetParent(roomGroup.transform);
        frontWallTop.transform.localPosition = new Vector3(0, 4f, -5f);
        frontWallTop.transform.localScale = new Vector3(4, 2, 1);
        frontWallTop.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.name = "Roof";
        roof.transform.SetParent(roomGroup.transform);
        roof.transform.localPosition = new Vector3(0, 5.5f, 0);
        roof.transform.localScale = new Vector3(10, 1, 10);
        roof.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

        // Doors
        Material glassMat = new Material(Shader.Find("Standard"));
        glassMat.color = new Color(0.2f, 0.5f, 0.8f, 0.6f);
        glassMat.SetFloat("_Mode", 3); // Transparent
        glassMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        glassMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        glassMat.SetInt("_ZWrite", 0);
        glassMat.DisableKeyword("_ALPHATEST_ON");
        glassMat.EnableKeyword("_ALPHABLEND_ON");
        glassMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        glassMat.renderQueue = 3000;

        GameObject entranceDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        entranceDoor.name = "EntranceDoor_Sliding";
        entranceDoor.transform.SetParent(roomGroup.transform);
        entranceDoor.transform.localPosition = new Vector3(0, 1.5f, -5f);
        entranceDoor.transform.localScale = new Vector3(3.9f, 3, 0.2f);
        entranceDoor.GetComponent<MeshRenderer>().sharedMaterial = glassMat;

        // Entrance Frame (Red)
        Material redMat = new Material(Shader.Find("Standard"));
        redMat.color = Color.red;
        GameObject inFrameLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        inFrameLeft.transform.SetParent(roomGroup.transform);
        inFrameLeft.transform.localPosition = new Vector3(-2.1f, 1.5f, -5f);
        inFrameLeft.transform.localScale = new Vector3(0.3f, 3.2f, 0.4f);
        inFrameLeft.GetComponent<MeshRenderer>().sharedMaterial = redMat;
        
        GameObject inFrameRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
        inFrameRight.transform.SetParent(roomGroup.transform);
        inFrameRight.transform.localPosition = new Vector3(2.1f, 1.5f, -5f);
        inFrameRight.transform.localScale = new Vector3(0.3f, 3.2f, 0.4f);
        inFrameRight.GetComponent<MeshRenderer>().sharedMaterial = redMat;
        
        GameObject inFrameTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        inFrameTop.transform.SetParent(roomGroup.transform);
        inFrameTop.transform.localPosition = new Vector3(0, 3.15f, -5f);
        inFrameTop.transform.localScale = new Vector3(4.5f, 0.3f, 0.4f);
        inFrameTop.GetComponent<MeshRenderer>().sharedMaterial = redMat;

        entranceDC = entranceDoor.AddComponent<DoorController>();
        SerializedObject edSo = new SerializedObject(entranceDC);
        edSo.FindProperty("doorType").enumValueIndex = (int)DoorController.DoorType.SlidingHorizontal;
        edSo.FindProperty("doorName").stringValue = "Entrance";
        edSo.FindProperty("closedLocalPosition").vector3Value = new Vector3(0, 1.5f, -5f);
        edSo.FindProperty("openLocalPosition").vector3Value = new Vector3(-4.2f, 1.5f, -5f);
        edSo.FindProperty("moveSpeed").floatValue = 6.0f;
        edSo.FindProperty("startsLocked").boolValue = false;
        edSo.ApplyModifiedProperties();
        EditorUtility.SetDirty(entranceDC);

        MallEntranceProximity proximity = entranceDoor.AddComponent<MallEntranceProximity>();
        SerializedObject proxSo = new SerializedObject(proximity);
        proxSo.FindProperty("mallSlidingDoor").objectReferenceValue = entranceDC;
        proxSo.ApplyModifiedProperties();

        // Exit Door inside the room (Door_5_Brown)
        GameObject brownDoorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Free Wood Door Pack/Prefab/Wood/Door_5/Door_5_Brown.prefab");
        GameObject exitDoor;
        Transform exitDoorLeaf = null;
        if (brownDoorPrefab != null)
        {
            exitDoor = (GameObject)PrefabUtility.InstantiatePrefab(brownDoorPrefab);
            exitDoor.name = "ExitDoor_Keypad";
            exitDoor.transform.SetParent(roomGroup.transform, false);
            exitDoor.transform.localPosition = new Vector3(0, 0, 4.9f);
            exitDoor.transform.localRotation = Quaternion.identity;
            exitDoor.transform.localScale = new Vector3(1.85f, 1.45f, 1f);
            exitDoorLeaf = exitDoor.transform.Find("Door");
            if (exitDoorLeaf != null)
            {
                var legacyDoor = exitDoorLeaf.GetComponent("Door") as MonoBehaviour;
                if (legacyDoor != null) legacyDoor.enabled = false;
            }
        }
        else
        {
            exitDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            exitDoor.name = "ExitDoor_Keypad";
            exitDoor.transform.SetParent(roomGroup.transform);
            exitDoor.transform.localPosition = new Vector3(0, 1.5f, 4.9f);
            exitDoor.transform.localScale = new Vector3(2f, 3f, 0.2f);
            exitDoor.GetComponent<MeshRenderer>().sharedMaterial = glassMat;
        }
        
        // Exit Frame (Green)
        Material greenMat = new Material(Shader.Find("Standard"));
        greenMat.color = Color.green;
        GameObject outFrameLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        outFrameLeft.transform.SetParent(roomGroup.transform);
        outFrameLeft.transform.localPosition = new Vector3(-1.15f, 1.5f, 4.9f);
        outFrameLeft.transform.localScale = new Vector3(0.3f, 3.2f, 0.4f);
        outFrameLeft.GetComponent<MeshRenderer>().sharedMaterial = greenMat;
        
        GameObject outFrameRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
        outFrameRight.transform.SetParent(roomGroup.transform);
        outFrameRight.transform.localPosition = new Vector3(1.15f, 1.5f, 4.9f);
        outFrameRight.transform.localScale = new Vector3(0.3f, 3.2f, 0.4f);
        outFrameRight.GetComponent<MeshRenderer>().sharedMaterial = greenMat;
        
        GameObject outFrameTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        outFrameTop.transform.SetParent(roomGroup.transform);
        outFrameTop.transform.localPosition = new Vector3(0, 3.15f, 4.9f);
        outFrameTop.transform.localScale = new Vector3(2.6f, 0.3f, 0.4f);
        outFrameTop.GetComponent<MeshRenderer>().sharedMaterial = greenMat;

        exitDC = exitDoor.AddComponent<DoorController>();
        SerializedObject xdSo = new SerializedObject(exitDC);
        xdSo.FindProperty("doorType").enumValueIndex = (int)DoorController.DoorType.HingeRotation;
        xdSo.FindProperty("doorName").stringValue = "Room 1 Exit";
        xdSo.FindProperty("doorTransform").objectReferenceValue = exitDoorLeaf != null ? exitDoorLeaf : exitDoor.transform;
        xdSo.FindProperty("closedYRotation").floatValue = 0f;
        xdSo.FindProperty("openYRotation").floatValue = 90f;
        xdSo.FindProperty("moveSpeed").floatValue = 3.0f;
        xdSo.FindProperty("startsLocked").boolValue = true;
        xdSo.ApplyModifiedProperties();
        EditorUtility.SetDirty(exitDC);

        return roomGroup;
    }

    private static void SetupPlayer(Vector3 spawnPosition)
    {
        GameObject player = new GameObject("Player");
        player.transform.position = spawnPosition;
        player.tag = "Player";

        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.3f;
        cc.center = new Vector3(0, 0.9f, 0);

        player.AddComponent<PlayerController>();

        GameObject cam = new GameObject("Main Camera");
        cam.transform.SetParent(player.transform);
        cam.transform.localPosition = new Vector3(0, 1.6f, 0);
        cam.tag = "MainCamera";
        
        cam.AddComponent<Camera>();
        cam.AddComponent<AudioListener>();

        SerializedObject playerSo = new SerializedObject(player.GetComponent<PlayerController>());
        playerSo.FindProperty("cameraTransform").objectReferenceValue = cam.transform;
        playerSo.ApplyModifiedProperties();

        GameObject defaultCam = GameObject.Find("Main Camera");
        if (defaultCam != null && defaultCam != cam)
        {
            DestroyImmediate(defaultCam);
        }
    }
    
    private static void SetupGameLogic(DoorController entranceDC, DoorController exitDC)
    {
        // Manager
        GameObject manager = new GameObject("EscapeRoomManager");
        manager.AddComponent<EscapeRoomManager>();
        
        // Keypad Canvas
        GameObject canvasObj = new GameObject("KeypadCanvas");
        Canvas c = canvasObj.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        KeypadUI keypadUI = canvasObj.AddComponent<KeypadUI>();

        // Background
        GameObject bgObj = new GameObject("DisplayBackground");
        bgObj.transform.SetParent(canvasObj.transform, false);
        var bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.3f, 0.2f);
        bgRect.anchorMax = new Vector2(0.7f, 0.8f);
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        var bgImage = bgObj.AddComponent<UnityEngine.UI.Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);

        // Status Text
        GameObject statusObj = new GameObject("StatusText");
        statusObj.transform.SetParent(bgObj.transform, false);
        var statusRect = statusObj.AddComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0f, 0.85f);
        statusRect.anchorMax = new Vector2(1f, 0.95f);
        statusRect.offsetMin = Vector2.zero;
        statusRect.offsetMax = Vector2.zero;
        var statusText = statusObj.AddComponent<UnityEngine.UI.Text>();
        statusText.text = "ENTER PASSWORD";
        statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        statusText.alignment = TextAnchor.MiddleCenter;
        statusText.color = Color.white;
        statusText.fontSize = 24;

        // Display Text
        GameObject displayObj = new GameObject("DisplayText");
        displayObj.transform.SetParent(bgObj.transform, false);
        var displayRect = displayObj.AddComponent<RectTransform>();
        displayRect.anchorMin = new Vector2(0.1f, 0.65f);
        displayRect.anchorMax = new Vector2(0.9f, 0.8f);
        displayRect.offsetMin = Vector2.zero;
        displayRect.offsetMax = Vector2.zero;
        var displayImage = displayObj.AddComponent<UnityEngine.UI.Image>();
        displayImage.color = Color.black;
        
        GameObject textValObj = new GameObject("TextValue");
        textValObj.transform.SetParent(displayObj.transform, false);
        var tvRect = textValObj.AddComponent<RectTransform>();
        tvRect.anchorMin = Vector2.zero;
        tvRect.anchorMax = Vector2.one;
        tvRect.offsetMin = Vector2.zero;
        tvRect.offsetMax = Vector2.zero;
        var displayText = textValObj.AddComponent<UnityEngine.UI.Text>();
        displayText.text = "";
        displayText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        displayText.alignment = TextAnchor.MiddleCenter;
        displayText.color = Color.green;
        displayText.fontSize = 40;

        // Number Buttons (1-9)
        float startX = 0.1f;
        float startY = 0.45f;
        float spacingX = 0.28f;
        float spacingY = 0.15f;
        float w = 0.2f;
        float h = 0.12f;

        int num = 1;
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                CreateButton(bgObj.transform, $"Btn_{num}", num.ToString(), 
                    new Vector2(startX + col*spacingX, startY - row*spacingY), 
                    new Vector2(w, h));
                num++;
            }
        }

        // Bottom Row: CLR, 0, ENT
        CreateButton(bgObj.transform, "Btn_CLR", "CLR", new Vector2(startX, startY - 3*spacingY), new Vector2(w, h), Color.red);
        CreateButton(bgObj.transform, "Btn_0", "0", new Vector2(startX + spacingX, startY - 3*spacingY), new Vector2(w, h));
        CreateButton(bgObj.transform, "Btn_ENT", "ENT", new Vector2(startX + 2*spacingX, startY - 3*spacingY), new Vector2(w, h), Color.green);
        
        // Close Button (top-right X)
        CreateButton(bgObj.transform, "Btn_Close", "X", new Vector2(0.88f, 0.88f), new Vector2(0.1f, 0.1f), Color.red);

        // Bind References in Script using Reflection or SerializedObject since fields are private/serialized
        SerializedObject so = new SerializedObject(keypadUI);
        so.FindProperty("keypadCanvas").objectReferenceValue = bgObj;
        so.FindProperty("displayText").objectReferenceValue = displayText;
        so.FindProperty("statusText").objectReferenceValue = statusText;
        so.FindProperty("displayBackground").objectReferenceValue = bgImage;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(keypadUI);

        bgObj.SetActive(false);

        // Event System for UI
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // Logic Triggers Group
        GameObject logicGroup = new GameObject("--- Escape Room Logic ---");
        
        GameObject exitLockObj = new GameObject("ExitDoorLock");
        exitLockObj.transform.SetParent(logicGroup.transform);
        // Position trigger right in front of the exit door
        exitLockObj.transform.position = exitDC.transform.position + new Vector3(0, 0, -2f);
        ExitDoorLock exitLock = exitLockObj.AddComponent<ExitDoorLock>();
        SerializedObject lockSo = new SerializedObject(exitLock);
        lockSo.FindProperty("keypadUI").objectReferenceValue = keypadUI;
        lockSo.FindProperty("exitDoor").objectReferenceValue = exitDC;
        lockSo.ApplyModifiedProperties();
        EditorUtility.SetDirty(exitLock);
        
        GameObject entranceTrapObj = new GameObject("EntranceTrapTrigger");
        entranceTrapObj.transform.SetParent(logicGroup.transform);
        entranceTrapObj.transform.position = entranceDC.transform.position + new Vector3(0, 0, 3.5f);

        BoxCollider bc = entranceTrapObj.AddComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.size = new Vector3(6f, 4f, 3f);

        EntranceTrapTrigger trap = entranceTrapObj.AddComponent<EntranceTrapTrigger>();
        SerializedObject trapSo = new SerializedObject(trap);
        trapSo.FindProperty("entranceDoor").objectReferenceValue = entranceDC;
        trapSo.ApplyModifiedProperties();
        EditorUtility.SetDirty(trap);
    }

    private static void CreateButton(Transform parent, string name, string label, Vector2 position, Vector2 size, Color? color = null)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        var rect = btnObj.AddComponent<RectTransform>();
        rect.anchorMin = position;
        rect.anchorMax = position + size;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        var img = btnObj.AddComponent<UnityEngine.UI.Image>();
        img.color = color ?? Color.gray;
        img.raycastTarget = true;
        
        btnObj.AddComponent<UnityEngine.UI.Button>();
        
        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);
        var txtRect = txtObj.AddComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;
        var txt = txtObj.AddComponent<UnityEngine.UI.Text>();
        txt.text = label;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.black;
        txt.fontSize = 24;
        txt.resizeTextForBestFit = true;
        txt.raycastTarget = false; // Prevent text from swallowing button click raycasts
    }
}
