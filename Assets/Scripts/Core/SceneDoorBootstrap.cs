using System.Collections.Generic;



using UnityEngine;



using UnityEngine.UI;



using AREscapeRoom.Doors;



using AREscapeRoom.UI;



using AREscapeRoom.Player;



using AREscapeRoom.Puzzles;







namespace AREscapeRoom.Core



{



    public static class SceneDoorBootstrap



    {



        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]



        private static void AfterSceneLoad()



        {



            FixBuiltinFonts();



            WireDoorsAndTriggers();



            CutBackWallDoorway();



            EscapeWorldBuilder.Install();



            SetupOldSimpleDrawer();



            SetupBlueDiamond();



            SetupLightforgedPedestal();



            SetupModernEntertainmentCenter();



            SetupLuxeHangerLamp();



            SetupStarcourtMall();



            SetupIceScreamMall();



            SetupDeskTableWhite();

            SetupReplacementDoors();

            SetupRoom5ShapesAndSockets();
            EnsurePlayerStartingPosition();



        }







        private static void FixBuiltinFonts()



        {



            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");



            if (font == null) return;







            Text[] texts = Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);



            for (int i = 0; i < texts.Length; i++)



            {



                if (texts[i] == null) continue;



                if (texts[i].font == null || texts[i].font.name == "Arial")



                {



                    texts[i].font = font;



                }



            }



        }







        private static void WireDoorsAndTriggers()



        {



            GameObject brownDoor = GameObject.Find("Door_5_Brown");



            if (brownDoor == null) brownDoor = GameObject.Find("Door_5_Room");



            GameObject slidingDoor = GameObject.Find("EntranceDoor_Sliding");







            // If Door_5_Brown / Door_5_Room is present, remove/hide the old sliding door



            if (brownDoor != null && slidingDoor != null)



            {



                slidingDoor.SetActive(false);



            }







            GameObject entranceObj = brownDoor != null ? brownDoor : slidingDoor;



            GameObject exitObj = GameObject.Find("ExitDoor_Keypad");



            if (entranceObj == null && exitObj == null) return;







            if (Object.FindFirstObjectByType<EscapeRoomManager>() == null)



            {



                new GameObject("EscapeRoomManager").AddComponent<EscapeRoomManager>();



            }







            DoorController entrance = null;



            DoorController exit = null;



            if (entranceObj != null)



            {



                entrance = entranceObj.GetComponent<DoorController>();



                if (entrance == null) entrance = entranceObj.AddComponent<DoorController>();







                // Check if this is the hinged wooden door (Door_5_Brown / Door_5_Room)



                Transform doorLeaf = entranceObj.transform.Find("Door");



                if (doorLeaf == null) doorLeaf = entranceObj.transform.Find("Door_5_Room");



                bool isHinged = doorLeaf != null || entranceObj.name.Contains("Door_5") || entranceObj.name.Contains("Brown");







                if (isHinged)



                {



                    if (doorLeaf == null)



                    {



                        for (int i = 0; i < entranceObj.transform.childCount; i++)



                        {



                            Transform child = entranceObj.transform.GetChild(i);



                            if (child.name.Equals("Door", System.StringComparison.OrdinalIgnoreCase) || child.name.Contains("Door_5_Room"))



                            {



                                doorLeaf = child;



                                break;



                            }



                        }



                    }







                    // Disable conflicting third-party DoorScript.Door component on child Door



                    if (doorLeaf != null)



                    {



                        MonoBehaviour[] mbs = doorLeaf.GetComponents<MonoBehaviour>();



                        for (int i = 0; i < mbs.Length; i++)



                        {



                            if (mbs[i] != null && mbs[i].GetType().Name == "Door")



                            {



                                mbs[i].enabled = false;



                            }



                        }



                    }







                    Transform rotTransform = doorLeaf != null ? doorLeaf : entranceObj.transform;



                    // Hinge rotation: 0 degrees closed, 90 degrees open to inside (+Z)



                    entrance.ConfigureHinge(rotTransform, 0f, 90f, false, "Entrance");







                    // Stick Caution_Frame to the moving door leaf so it follows the door when it opens



                    AttachCautionFrameToDoorLeaf(rotTransform);



                }



                else



                {



                    entrance.ConfigureSliding(entranceObj.transform.localPosition, entranceObj.transform.localPosition + new Vector3(-4.2f, 0f, 0f), false, "Entrance");



                }







                MallEntranceProximity proximity = entranceObj.GetComponent<MallEntranceProximity>();



                if (proximity == null) proximity = entranceObj.AddComponent<MallEntranceProximity>();



                proximity.Bind(entrance);



            }



        if (exitObj != null)



            {



                exit = exitObj.GetComponent<DoorController>();



                if (exit == null) exit = exitObj.AddComponent<DoorController>();







                Transform exitLeaf = exitObj.transform.Find("Door");



                bool isHinged = exitLeaf != null || exitObj.name.Contains("Door_5") || exitObj.name.Contains("Brown");







                if (isHinged)



                {



                    if (exitLeaf != null)



                    {



                        exitLeaf.localPosition = new Vector3(0.5f, 0f, 0.004845f);



                        exitLeaf.localRotation = Quaternion.identity;



                        exitLeaf.localScale = Vector3.one;







                        MonoBehaviour[] mbs = exitLeaf.GetComponents<MonoBehaviour>();



                        for (int i = 0; i < mbs.Length; i++)



                        {



                            if (mbs[i] != null && mbs[i].GetType().Name == "Door")



                            {



                                mbs[i].enabled = false;



                            }



                        }



                    }







                    // Respect manual editor layout for ExitDoor_Keypad:
                    // exitObj.transform.localScale = new Vector3(1.85f, 1.45f, 1f);







                    BoxCollider box = exitObj.GetComponent<BoxCollider>();



                    if (box == null) box = exitObj.AddComponent<BoxCollider>();



                    box.size = new Vector3(1.1f, 2.15f, 0.4f);



                    box.center = new Vector3(0f, 1.075f, 0f);



                    box.isTrigger = false;



                    box.enabled = true;







                    Transform rotTransform = exitLeaf != null ? exitLeaf : exitObj.transform;



                    exit.ConfigureHinge(rotTransform, 0f, 90f, true, "Room 1 Exit");







                    ProgressDoor pd = exitObj.GetComponent<ProgressDoor>();



                    if (pd == null) pd = exitObj.AddComponent<ProgressDoor>();



                    pd.Bind(exit, DoorRequirement.RequiresKey, false, 2);



                }



                else



                {



                    exit.ConfigureSliding(exitObj.transform.localPosition, exitObj.transform.localPosition + new Vector3(-3.2f, 0f, 0f), true, "Room 1 Exit");



                }



            }







            ExitDoorLock exitLock = Object.FindFirstObjectByType<ExitDoorLock>();



            if (exitLock != null)



            {



                exitLock.enabled = false;



            }







            GameObject trapObj = GameObject.Find("EntranceTrapTrigger");



            if (trapObj == null && entranceObj != null)



            {



                trapObj = new GameObject("EntranceTrapTrigger");



                trapObj.transform.position = entranceObj.transform.position + entranceObj.transform.forward * 3.5f + Vector3.up;



            }







            if (trapObj != null && entrance != null)



            {



                BoxCollider box = trapObj.GetComponent<BoxCollider>();



                if (box == null) box = trapObj.AddComponent<BoxCollider>();



                box.isTrigger = true;



                box.size = new Vector3(6f, 4f, 3f);



                box.center = Vector3.zero;







                EntranceTrapTrigger trap = trapObj.GetComponent<EntranceTrapTrigger>();



                if (trap == null) trap = trapObj.AddComponent<EntranceTrapTrigger>();



                trap.Bind(entrance);







                if (entranceObj != null)



                {



                    Vector3 inside = entranceObj.transform.position;



                    inside += Vector3.forward * 3.5f;



                    inside.y = entranceObj.transform.position.y;



                    trapObj.transform.position = inside;



                }



            }







            GameObject exitTrapObj = GameObject.Find("Room2TrapTrigger");



            if (exitTrapObj == null && exitObj != null)



            {



                exitTrapObj = new GameObject("Room2TrapTrigger");



                Vector3 inside = exitObj.transform.position + Vector3.forward * 2.7f;



                inside.y = exitObj.transform.position.y + 1.5f;



                exitTrapObj.transform.position = inside;



            }







            if (exitTrapObj != null && exit != null)



            {



                BoxCollider box = exitTrapObj.GetComponent<BoxCollider>();



                if (box == null) box = exitTrapObj.AddComponent<BoxCollider>();



                box.isTrigger = true;



                box.size = new Vector3(6f, 3.5f, 2.5f);



                box.center = Vector3.zero;







                RoomTrapTrigger trap = exitTrapObj.GetComponent<RoomTrapTrigger>();



                if (trap == null) trap = exitTrapObj.AddComponent<RoomTrapTrigger>();



                trap.Bind(exit, RoomId.Room2, true);







                if (exitObj != null)



                {



                    Vector3 inside = exitObj.transform.position + Vector3.forward * 2.7f;



                    inside.y = exitObj.transform.position.y + 1.5f;



                    exitTrapObj.transform.position = inside;



                }



            }







            PlayerController player = Object.FindFirstObjectByType<PlayerController>();



            if (player != null)



            {



                Camera cam = player.GetComponentInChildren<Camera>();



                if (cam != null)



                {



                    player.AssignLookCamera(cam.transform);



                }







                if (player.transform.Find("PlayerTriggerProbe") == null)



                {



                    GameObject probe = new GameObject("PlayerTriggerProbe");



                    probe.tag = "Player";



                    probe.transform.SetParent(player.transform, false);



                    probe.transform.localPosition = new Vector3(0f, 0.9f, 0f);



                    SphereCollider sphere = probe.AddComponent<SphereCollider>();



                    sphere.isTrigger = true;



                    sphere.radius = 0.45f;



                    Rigidbody rb = probe.AddComponent<Rigidbody>();



                    rb.isKinematic = true;



                    rb.useGravity = false;



                    rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;



                }



            }



        }







                public static void AttachCautionFrameToDoorLeaf(Transform doorLeaf)

        {

            if (doorLeaf == null) return;



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



            if (caution != null && doorLeaf != null)

            {

                if (caution.transform.parent != doorLeaf)

                {

                    caution.transform.SetParent(doorLeaf, true);

                    Debug.Log($"[SceneDoorBootstrap] Successfully attached Caution_Frame to moving door leaf '{doorLeaf.name}' of '{doorLeaf.parent?.name}'!");

                }

            }

        }



private static void CutBackWallDoorway()



        {



            GameObject backWall = GameObject.Find("Wall_Back");



            if (backWall == null) return;



            if (GameObject.Find("Wall_Back_Left") != null) return;







            Transform parent = backWall.transform.parent;



            Vector3 localPos = backWall.transform.localPosition;



            Quaternion localRot = backWall.transform.localRotation;



            Material mat = null;



            MeshRenderer srcRenderer = backWall.GetComponent<MeshRenderer>();



            if (srcRenderer != null) mat = srcRenderer.sharedMaterial;







            Object.DestroyImmediate(backWall);







            CreateWallPiece("Wall_Back_Left", parent, localPos + new Vector3(-3.05f, 0f, 0f), new Vector3(3.9f, 5f, 1f), localRot, mat);



            CreateWallPiece("Wall_Back_Right", parent, localPos + new Vector3(3.05f, 0f, 0f), new Vector3(3.9f, 5f, 1f), localRot, mat);



            CreateWallPiece("Wall_Back_Top", parent, localPos + new Vector3(0f, 1.5f, 0f), new Vector3(2.2f, 2f, 1f), localRot, mat);



        }







        private static void CreateWallPiece(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Quaternion localRotation, Material mat)



        {



            GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cube);



            piece.name = name;



            piece.transform.SetParent(parent, false);



            piece.transform.localPosition = localPosition;



            piece.transform.localRotation = localRotation;



            piece.transform.localScale = localScale;



            if (mat != null)



            {



                piece.GetComponent<MeshRenderer>().sharedMaterial = mat;



            }



        }







        private static void SetupOldSimpleDrawer()



        {



            GameObject drawerObj = GameObject.Find("Old_Simple_Drawer");



            if (drawerObj == null) return;







            // Respect manual editor layout for Old_Simple_Drawer:
            // drawerObj.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);







            var ctrl = drawerObj.GetComponent<OldSimpleDrawerController>();



            if (ctrl == null) ctrl = drawerObj.AddComponent<OldSimpleDrawerController>();







            Transform dLeft = drawerObj.transform.Find("Drawer_Left");



            Transform dRight = drawerObj.transform.Find("Drawer_Right");







            if (dLeft == null || dRight == null)



            {



                MeshFilter mf = drawerObj.GetComponent<MeshFilter>();



                MeshRenderer mr = drawerObj.GetComponent<MeshRenderer>();



                Material mat = mr != null ? mr.sharedMaterial : null;







                if (mf != null && mf.sharedMesh != null)



                {



                    SplitDrawerMesh(drawerObj, mf.sharedMesh, mat);



                }



            }







            ctrl.InitTransforms();



            ctrl.FindAndMountKey();



        }







        private static void SplitDrawerMesh(GameObject root, Mesh fullMesh, Material mat)



        {



            Vector3[] allVerts = fullMesh.vertices;



            Vector3[] allNormals = fullMesh.normals;



            Vector2[] allUVs = fullMesh.uv;



            int[] allTris = fullMesh.triangles;







            if (allVerts == null || allVerts.Length == 0) return;







            List<int> frameTris = new List<int>();



            List<int> leftTris = new List<int>();



            List<int> rightTris = new List<int>();







            for (int i = 0; i < allTris.Length; i += 3)



            {



                int i0 = allTris[i];



                int i1 = allTris[i + 1];



                int i2 = allTris[i + 2];







                Vector3 v0 = allVerts[i0];



                Vector3 v1 = allVerts[i1];



                Vector3 v2 = allVerts[i2];







                float minX = Mathf.Min(v0.x, Mathf.Min(v1.x, v2.x));



                float maxX = Mathf.Max(v0.x, Mathf.Max(v1.x, v2.x));



                float minY = Mathf.Min(v0.y, Mathf.Min(v1.y, v2.y));







                if (minY >= 1.4f && maxX <= 0.05f && minX >= -1.95f)



                {



                    leftTris.Add(i0); leftTris.Add(i1); leftTris.Add(i2);



                }



                else if (minY >= 1.4f && minX >= -0.05f && maxX <= 1.95f)



                {



                    rightTris.Add(i0); rightTris.Add(i1); rightTris.Add(i2);



                }



                else



                {



                    frameTris.Add(i0); frameTris.Add(i1); frameTris.Add(i2);



                }



            }







            CreateSubPart(root, "Table_Frame", allVerts, allNormals, allUVs, frameTris, mat, false);



            CreateSubPart(root, "Drawer_Left", allVerts, allNormals, allUVs, leftTris, mat, true);



            CreateSubPart(root, "Drawer_Right", allVerts, allNormals, allUVs, rightTris, mat, true);







            MeshFilter rootMf = root.GetComponent<MeshFilter>();



            if (rootMf != null) rootMf.sharedMesh = null;



        }







        private static void CreateSubPart(GameObject root, string name, Vector3[] srcVerts, Vector3[] srcNormals, Vector2[] srcUVs, List<int> tris, Material mat, bool addCollider)



        {



            if (tris.Count == 0) return;







            GameObject part = new GameObject(name);



            part.transform.SetParent(root.transform, false);



            part.transform.localPosition = Vector3.zero;



            part.transform.localRotation = Quaternion.identity;



            part.transform.localScale = Vector3.one;







            Dictionary<int, int> remap = new Dictionary<int, int>();



            List<Vector3> verts = new List<Vector3>();



            List<Vector3> normals = new List<Vector3>();



            List<Vector2> uvs = new List<Vector2>();



            List<int> newTris = new List<int>();







            for (int i = 0; i < tris.Count; i++)



            {



                int oldIdx = tris[i];



                if (!remap.TryGetValue(oldIdx, out int newIdx))



                {



                    newIdx = verts.Count;



                    remap[oldIdx] = newIdx;



                    verts.Add(srcVerts[oldIdx]);



                    if (srcNormals != null && srcNormals.Length > oldIdx) normals.Add(srcNormals[oldIdx]);



                    if (srcUVs != null && srcUVs.Length > oldIdx) uvs.Add(srcUVs[oldIdx]);



                }



                newTris.Add(newIdx);



            }







            Mesh mesh = new Mesh();



            mesh.name = name + "_Mesh";



            mesh.SetVertices(verts);



            if (normals.Count == verts.Count) mesh.SetNormals(normals);



            if (uvs.Count == verts.Count) mesh.SetUVs(0, uvs);



            mesh.SetTriangles(newTris, 0);



            mesh.RecalculateBounds();







            MeshFilter mf = part.AddComponent<MeshFilter>();



            mf.sharedMesh = mesh;







            MeshRenderer mr = part.AddComponent<MeshRenderer>();



            if (mat != null) mr.sharedMaterial = mat;







            if (addCollider)



            {



                BoxCollider box = part.AddComponent<BoxCollider>();



                box.center = mesh.bounds.center;



                box.size = mesh.bounds.size;



            }



        }







        public static void SetupBlueDiamond()



        {



            GameObject room3 = GameObject.Find("Room_3");



            Transform room3Transform = room3 != null ? room3.transform : null;







            Mesh diamondMesh = null;



            Material diamondMat = null;







            // 1. Try to obtain mesh and material from any existing diamond object



            GameObject refDiamond = GameObject.Find("Blue_Diamond");



            if (refDiamond != null)



            {



                MeshFilter refMf = refDiamond.GetComponentInChildren<MeshFilter>();



                if (refMf != null && refMf.sharedMesh != null && refMf.sharedMesh.vertexCount > 0)



                {



                    diamondMesh = refMf.sharedMesh;



                }



                MeshRenderer refMr = refDiamond.GetComponentInChildren<MeshRenderer>();



                if (refMr != null && refMr.sharedMaterial != null)



                {



                    diamondMat = refMr.sharedMaterial;



                }



            }







            // Check Blue_Diamond_1..5 for valid material



            for (int i = 1; i <= 5; i++)



            {



                GameObject obj = GameObject.Find("Blue_Diamond_" + i);



                if (obj == null) continue;







                if (diamondMesh == null)



                {



                    MeshFilter mf = obj.GetComponentInChildren<MeshFilter>();



                    if (mf != null && mf.sharedMesh != null && mf.sharedMesh.vertexCount > 0)



                    {



                        diamondMesh = mf.sharedMesh;



                    }



                }







                if (diamondMat == null)



                {



                    MeshRenderer mr = obj.GetComponentInChildren<MeshRenderer>();



                    if (mr != null && mr.sharedMaterial != null && mr.sharedMaterial.name.IndexOf("Default", System.StringComparison.OrdinalIgnoreCase) < 0)



                    {



                        diamondMat = mr.sharedMaterial;



                    }



                }



            }







            // 2. Fallback: Parse OBJ mesh directly from Assets/Blue_Diamond/Blue_Diamond.obj



            if (diamondMesh == null)



            {



                diamondMesh = LoadDiamondMeshFromObj();



            }







            // 3. Fallback: Find material in Resources or create standard cyan diamond material



            if (diamondMat == null)



            {



                Material[] mats = Resources.FindObjectsOfTypeAll<Material>();



                for (int m = 0; m < mats.Length; m++)



                {



                    if (mats[m] != null && mats[m].name.IndexOf("Blue_Diamond", System.StringComparison.OrdinalIgnoreCase) >= 0)



                    {



                        diamondMat = mats[m];



                        break;



                    }



                }



            }



            if (diamondMat == null)



            {



                Shader shader = Shader.Find("Standard");



                if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");



                if (shader != null)



                {



                    diamondMat = new Material(shader);



                    diamondMat.name = "Runtime_Blue_Diamond_Material";



                    diamondMat.color = new Color(0.2f, 0.7f, 1f, 1f);



                    diamondMat.SetFloat("_Metallic", 0.9f);



                    diamondMat.SetFloat("_Glossiness", 0.95f);



                }



            }







            Vector3[] diamondPositions = new Vector3[]



            {



                new Vector3(1.72f, 1.0f, 2.81f),    // Diamond 1: Hovering at the pedestal spot the user saw



                new Vector3(-4.5f, 1.34f, -1.5f),   // Diamond 2: Room 3 left side



                new Vector3(4.5f, 1.6f, 1.2f),      // Diamond 3: Room 3 right side



                new Vector3(-3.2f, 1.6f, 4.4f),     // Diamond 4: Room 3 back-left corner



                new Vector3(3.4f, 2.0f, -4.4f)      // Diamond 5: Room 3 front-right corner



            };







            List<GameObject> diamondObjects = new List<GameObject>();







            for (int k = 1; k <= 5; k++)



            {



                string targetName = "Blue_Diamond_" + k;



                GameObject dObj = GameObject.Find(targetName);







                if (dObj == null)



                {



                    if (k == 1 && refDiamond != null)



                    {



                        dObj = refDiamond;



                        dObj.name = targetName;



                    }



                    else



                    {



                        dObj = new GameObject(targetName);



                        if (room3Transform != null) dObj.transform.SetParent(room3Transform, false);



                    }



                }







                if (room3Transform != null && dObj.transform.parent != room3Transform)



                {



                    dObj.transform.SetParent(room3Transform, true);



                }







                
                // Respect visual editor layout coordinates for Blue Diamonds in scene:
                // dObj.transform.localPosition = diamondPositions[k - 1];



                // dObj.transform.localRotation = Quaternion.Euler(0f, (k - 1) * 72f, 0f);



                // dObj.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);







                // Setup MeshFilter



                MeshFilter mf = dObj.GetComponent<MeshFilter>();



                if (mf == null) mf = dObj.AddComponent<MeshFilter>();



                if (diamondMesh != null)



                {



                    mf.sharedMesh = diamondMesh;



                }







                // Setup MeshRenderer



                MeshRenderer mr = dObj.GetComponent<MeshRenderer>();



                if (mr == null) mr = dObj.AddComponent<MeshRenderer>();



                if (diamondMat != null)



                {



                    mr.sharedMaterial = diamondMat;



                }



                mr.enabled = true;







                // Setup SphereCollider



                SphereCollider sc = dObj.GetComponent<SphereCollider>();



                if (sc == null) sc = dObj.AddComponent<SphereCollider>();



                sc.center = Vector3.zero;



                sc.radius = 1.0f;



                sc.isTrigger = false;



                sc.enabled = true;







                // Remove conflicting child GemCollectible components



                GemCollectible[] childGems = dObj.GetComponentsInChildren<GemCollectible>(true);



                for (int c = 0; c < childGems.Length; c++)



                {



                    if (childGems[c] != null && childGems[c].gameObject != dObj)



                    {



                        Object.Destroy(childGems[c]);



                    }



                }







                // Setup GemCollectible on root



                GemCollectible gem = dObj.GetComponent<GemCollectible>();



                if (gem == null) gem = dObj.AddComponent<GemCollectible>();



                gem.enabled = true;







                dObj.SetActive(true);



                diamondObjects.Add(dObj);



            }







            // If standalone "Blue_Diamond" is still around and wasn't reused as Blue_Diamond_1, deactivate it



            if (refDiamond != null && refDiamond.name == "Blue_Diamond" && !diamondObjects.Contains(refDiamond))



            {



                refDiamond.SetActive(false);



            }







            Debug.Log($"[SceneDoorBootstrap] Successfully initialized {diamondObjects.Count} Blue Diamonds in Room 3.");



        }







        private static Mesh LoadDiamondMeshFromObj()



        {



            try



            {



                string path = System.IO.Path.Combine(Application.dataPath, "Blue_Diamond", "Blue_Diamond.obj");



                if (!System.IO.File.Exists(path))



                {



                    string[] found = System.IO.Directory.GetFiles(Application.dataPath, "Blue_Diamond.obj", System.IO.SearchOption.AllDirectories);



                    if (found.Length > 0) path = found[0];



                    else return null;



                }







                string[] lines = System.IO.File.ReadAllLines(path);



                List<Vector3> rawV = new List<Vector3>();



                List<Vector3> rawN = new List<Vector3>();



                List<Vector2> rawUV = new List<Vector2>();







                List<Vector3> verts = new List<Vector3>();



                List<Vector3> normals = new List<Vector3>();



                List<Vector2> uvs = new List<Vector2>();



                List<int> tris = new List<int>();







                var culture = System.Globalization.CultureInfo.InvariantCulture;







                for (int i = 0; i < lines.Length; i++)



                {



                    string line = lines[i].Trim();



                    if (line.StartsWith("v "))



                    {



                        string[] p = line.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);



                        if (p.Length >= 4)



                        {



                            rawV.Add(new Vector3(float.Parse(p[1], culture), float.Parse(p[2], culture), float.Parse(p[3], culture)));



                        }



                    }



                    else if (line.StartsWith("vn "))



                    {



                        string[] p = line.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);



                        if (p.Length >= 4)



                        {



                            rawN.Add(new Vector3(float.Parse(p[1], culture), float.Parse(p[2], culture), float.Parse(p[3], culture)));



                        }



                    }



                    else if (line.StartsWith("vt "))



                    {



                        string[] p = line.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);



                        if (p.Length >= 3)



                        {



                            rawUV.Add(new Vector2(float.Parse(p[1], culture), float.Parse(p[2], culture)));



                        }



                    }



                    else if (line.StartsWith("f "))



                    {



                        string[] p = line.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);



                        if (p.Length >= 4)



                        {



                            for (int j = 1; j <= 3; j++)



                            {



                                string[] idx = p[j].Split('/');



                                int vi = int.Parse(idx[0]) - 1;



                                if (vi >= 0 && vi < rawV.Count) verts.Add(rawV[vi]);



                                else verts.Add(Vector3.zero);







                                if (idx.Length > 1 && !string.IsNullOrEmpty(idx[1]) && rawUV.Count > 0)



                                {



                                    int uvi = int.Parse(idx[1]) - 1;



                                    uvs.Add(uvi >= 0 && uvi < rawUV.Count ? rawUV[uvi] : Vector2.zero);



                                }



                                else



                                {



                                    uvs.Add(Vector2.zero);



                                }







                                if (idx.Length > 2 && !string.IsNullOrEmpty(idx[2]) && rawN.Count > 0)



                                {



                                    int ni = int.Parse(idx[2]) - 1;



                                    normals.Add(ni >= 0 && ni < rawN.Count ? rawN[ni] : Vector3.up);



                                }



                                else



                                {



                                    normals.Add(Vector3.up);



                                }







                                tris.Add(verts.Count - 1);



                            }



                        }



                    }



                }







                if (verts.Count == 0) return null;







                Mesh mesh = new Mesh();



                mesh.name = "Blue_Diamond_Runtime_Mesh";



                mesh.SetVertices(verts);



                mesh.SetTriangles(tris, 0);



                if (normals.Count == verts.Count) mesh.SetNormals(normals);



                else mesh.RecalculateNormals();



                if (uvs.Count == verts.Count) mesh.SetUVs(0, uvs);



                mesh.RecalculateBounds();



                return mesh;



            }



            catch (System.Exception ex)



            {



                Debug.LogWarning("[SceneDoorBootstrap] Could not parse OBJ mesh: " + ex.Message);



                return null;



            }



        }



    



        private static void SetupLightforgedPedestal()



        {



            GameObject pedestal = GameObject.Find("Lightforged_Pedestal");



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







                var col = pedestal.AddComponent<BoxCollider>();



                col.center = new Vector3(0f, 1.1f, 0f);



                col.size = new Vector3(2.3f, 2.2f, 2.3f);



            }







            MeshFilter mf = pedestal.GetComponent<MeshFilter>();



            if (mf == null) mf = pedestal.AddComponent<MeshFilter>();







            MeshRenderer mr = pedestal.GetComponent<MeshRenderer>();



            if (mr == null) mr = pedestal.AddComponent<MeshRenderer>();







            if (mf.sharedMesh == null || mf.sharedMesh.vertexCount == 0)



            {



                Mesh loadedMesh = LoadLightforgedPedestalMeshFromObj();



                if (loadedMesh != null)



                {



                    mf.sharedMesh = loadedMesh;



                }



            }







            Material[] mats = mr.sharedMaterials;



            bool needMats = mats == null || mats.Length < 7 || mats[0] == null;



            if (needMats)



            {



                mats = new Material[7];



                string[] matNames = {



                    "Lightforged_Platform_Mat",



                    "Lightforged_Pedestal_Mat",



                    "Lightforged_Cloth_Mat",



                    "Lightforged_Book_Mat",



                    "Lightforged_Parchment_Mat",



                    "Lightforged_Feather_Mat",



                    "Lightforged_Sigil_Mat"



                };



                string[] texNames = {



                    "Lightforged_Platform_BaseColor.png",



                    "Lightforged_Pedestal_BaseColor.png",



                    "Lightforged_Cloth_BaseColor.png",



                    "Lightforged_Book_BaseColor.png",



                    "Lightforged_Parchment_BaseColor.png",



                    "Lightforged_Feather_BaseColor.png",



                    "Lightforged_Sigil_BaseColor.png"



                };



                string[] normNames = {



                    "Lightforged_Platform_Normal.png",



                    "Lightforged_Pedestal_Normal.png",



                    "Lightforged_Cloth_Normal.png",



                    "Lightforged_Book_Normal.png",



                    "Lightforged_Parchment_Normal.png",



                    "Lightforged_Feather_Normal.png",



                    "Lightforged_Sigil_Normal.png"



                };



                string[] metNames = {



                    "Lightforged_Platform_MetallicSmoothness.png",



                    "Lightforged_Pedestal_MetallicSmoothness.png",



                    "Lightforged_Cloth_MetallicSmoothness.png",



                    "Lightforged_Book_MetallicSmoothness.png",



                    "Lightforged_Parchment_MetallicSmoothness.png",



                    "Lightforged_Feather_MetallicSmoothness.png",



                    "Lightforged_Sigil_MetallicSmoothness.png"



                };







                Shader stdShader = Shader.Find("Standard");



                if (stdShader == null) stdShader = Shader.Find("Universal Render Pipeline/Lit");



                if (stdShader == null) stdShader = Shader.Find("Mobile/Diffuse");







                string basePath = System.IO.Path.Combine(Application.dataPath, "Lightforged_Pedestal");



                string texPath = System.IO.Path.Combine(basePath, "Textures");







                for (int i = 0; i < 7; i++)



                {



                    Material m = new Material(stdShader);



                    m.name = matNames[i];



                    m.EnableKeyword("_METALLICGLOSSMAP");



                    m.EnableKeyword("_NORMALMAP");



                    m.SetFloat("_Glossiness", 0.95f);



                    m.SetFloat("_Metallic", 0.85f);







                    string fullTexPath = System.IO.Path.Combine(texPath, texNames[i]);



                    if (System.IO.File.Exists(fullTexPath))



                    {



                        byte[] bytes = System.IO.File.ReadAllBytes(fullTexPath);



                        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, true);



                        if (tex.LoadImage(bytes))



                        {



                            tex.Apply(true, false);



                            m.mainTexture = tex;



                        }



                    }







                    string fullNormPath = System.IO.Path.Combine(texPath, normNames[i]);



                    if (System.IO.File.Exists(fullNormPath))



                    {



                        byte[] bytes = System.IO.File.ReadAllBytes(fullNormPath);



                        Texture2D ntex = new Texture2D(2, 2, TextureFormat.RGBA32, true, true);



                        if (ntex.LoadImage(bytes))



                        {



                            ntex.Apply(true, false);



                            m.SetTexture("_BumpMap", ntex);



                        }



                    }







                    string fullMetPath = System.IO.Path.Combine(texPath, metNames[i]);



                    if (System.IO.File.Exists(fullMetPath))



                    {



                        byte[] bytes = System.IO.File.ReadAllBytes(fullMetPath);



                        Texture2D mtex = new Texture2D(2, 2, TextureFormat.RGBA32, true);



                        if (mtex.LoadImage(bytes))



                        {



                            mtex.Apply(true, false);



                            m.SetTexture("_MetallicGlossMap", mtex);



                        }



                    }







                    if (i == 6) // Sigil Emissive



                    {



                        m.EnableKeyword("_EMISSION");



                        m.SetColor("_EmissionColor", new Color(2.5f, 2.1f, 0.8f, 1f));



                        string emPath = System.IO.Path.Combine(texPath, "Lightforged_Sigil_Emission.png");



                        if (System.IO.File.Exists(emPath))



                        {



                            byte[] bytes = System.IO.File.ReadAllBytes(emPath);



                            Texture2D etex = new Texture2D(2, 2, TextureFormat.RGBA32, true);



                            if (etex.LoadImage(bytes))



                            {



                                etex.Apply(true, false);



                                m.SetTexture("_EmissionMap", etex);



                            }



                        }



                        else if (m.mainTexture != null)



                        {



                            m.SetTexture("_EmissionMap", m.mainTexture);



                        }



                    }



                    mats[i] = m;



                }



                mr.sharedMaterials = mats;



            }







            // Add radiant Holy Light illumination



            if (pedestal.transform.Find("HolyLight_Point") == null)



            {



                GameObject lightObj = new GameObject("HolyLight_Point");



                lightObj.transform.SetParent(pedestal.transform, false);



                lightObj.transform.localPosition = new Vector3(0f, 1.85f, -0.06f);



                Light l = lightObj.AddComponent<Light>();



                l.type = LightType.Point;



                l.range = 7f;



                l.intensity = 1.4f;



                l.color = new Color(1f, 0.88f, 0.55f);



            }
            // Configure Final Vault Controller on Lightforged Pedestal (same jobs as Room 6 Pedestal)
            AREscapeRoom.Puzzles.FinalVaultController vault = pedestal.GetComponent<AREscapeRoom.Puzzles.FinalVaultController>();
            if (vault == null)
            {
                vault = pedestal.AddComponent<AREscapeRoom.Puzzles.FinalVaultController>();
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

#if UNITY_EDITOR
            if (door14Obj == null)
            {
                var prefab14 = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Free Wood Door Pack/Prefab/Wood/Door_14/Door_14_Venge.prefab");
                if (prefab14 != null)
                {
                    door14Obj = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab14);
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
                }
            }
#endif

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

                DoorController dc14 = door14Obj.GetComponent<DoorController>();
                if (dc14 == null) dc14 = door14Obj.AddComponent<DoorController>();
                dc14.ConfigureHinge(doorLeaf, 0f, -90f, true, "Door_14_Venge");

                AREscapeRoom.Doors.ProgressionDoor pd14 = door14Obj.GetComponent<AREscapeRoom.Doors.ProgressionDoor>();
                if (pd14 == null) pd14 = door14Obj.AddComponent<AREscapeRoom.Doors.ProgressionDoor>();
                pd14.Bind(dc14, AREscapeRoom.Core.DoorRequirement.RequiresFinalValidation, false, 6);

                vault.Bind(pd14);

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
                }
            }
}







        public static Mesh LoadLightforgedPedestalMeshFromObj()



        {



            string objPath = System.IO.Path.Combine(Application.dataPath, "Lightforged_Pedestal", "Lightforged_Pedestal.obj");



            if (!System.IO.File.Exists(objPath)) return null;







            try



            {



                var culture = System.Globalization.CultureInfo.InvariantCulture;



                List<Vector3> rawV = new List<Vector3>();



                List<Vector3> rawN = new List<Vector3>();



                List<Vector2> rawUV = new List<Vector2>();







                List<Vector3> outV = new List<Vector3>();



                List<Vector3> outN = new List<Vector3>();



                List<Vector2> outUV = new List<Vector2>();







                List<int>[] subTris = new List<int>[7];



                for (int s = 0; s < 7; s++) subTris[s] = new List<int>();



                int activeSub = 0;







                string[] lines = System.IO.File.ReadAllLines(objPath);



                for (int i = 0; i < lines.Length; i++)



                {



                    string line = lines[i].Trim();



                    if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;







                    if (line.StartsWith("usemtl "))



                    {



                        string matName = line.Substring(7).Trim();



                        if (matName.Contains("Platform")) activeSub = 0;



                        else if (matName.Contains("Pedestal")) activeSub = 1;



                        else if (matName.Contains("Cloth")) activeSub = 2;



                        else if (matName.Contains("Book")) activeSub = 3;



                        else if (matName.Contains("Parchment")) activeSub = 4;



                        else if (matName.Contains("Feather")) activeSub = 5;



                        else if (matName.Contains("Sigil")) activeSub = 6;



                    }



                    else if (line.StartsWith("v "))



                    {



                        string[] p = line.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);



                        if (p.Length >= 4)



                        {



                            rawV.Add(new Vector3(float.Parse(p[1], culture), float.Parse(p[2], culture), float.Parse(p[3], culture)));



                        }



                    }



                    else if (line.StartsWith("vn "))



                    {



                        string[] p = line.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);



                        if (p.Length >= 4)



                        {



                            rawN.Add(new Vector3(float.Parse(p[1], culture), float.Parse(p[2], culture), float.Parse(p[3], culture)));



                        }



                    }



                    else if (line.StartsWith("vt "))



                    {



                        string[] p = line.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);



                        if (p.Length >= 3)



                        {



                            rawUV.Add(new Vector2(float.Parse(p[1], culture), float.Parse(p[2], culture)));



                        }



                    }



                    else if (line.StartsWith("f "))



                    {



                        string[] p = line.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);



                        if (p.Length >= 4)



                        {



                            for (int j = 1; j <= 3; j++)



                            {



                                string[] idx = p[j].Split('/');



                                int vi = int.Parse(idx[0]) - 1;



                                if (vi >= 0 && vi < rawV.Count) outV.Add(rawV[vi]);



                                else outV.Add(Vector3.zero);







                                if (idx.Length > 1 && !string.IsNullOrEmpty(idx[1]) && rawUV.Count > 0)



                                {



                                    int uvi = int.Parse(idx[1]) - 1;



                                    outUV.Add(uvi >= 0 && uvi < rawUV.Count ? rawUV[uvi] : Vector2.zero);



                                }



                                else outUV.Add(Vector2.zero);







                                if (idx.Length > 2 && !string.IsNullOrEmpty(idx[2]) && rawN.Count > 0)



                                {



                                    int ni = int.Parse(idx[2]) - 1;



                                    outN.Add(ni >= 0 && ni < rawN.Count ? rawN[ni] : Vector3.up);



                                }



                                else outN.Add(Vector3.up);







                                subTris[activeSub].Add(outV.Count - 1);



                            }



                        }



                    }



                }







                if (outV.Count == 0) return null;







                Mesh mesh = new Mesh();



                mesh.name = "Lightforged_Pedestal_Mesh";



                mesh.SetVertices(outV);



                mesh.subMeshCount = 7;



                for (int s = 0; s < 7; s++)



                {



                    mesh.SetTriangles(subTris[s], s);



                }



                if (outN.Count == outV.Count) mesh.SetNormals(outN);



                else mesh.RecalculateNormals();



                if (outUV.Count == outV.Count) mesh.SetUVs(0, outUV);



                mesh.RecalculateBounds();



                return mesh;



            }



            catch (System.Exception ex)



            {



                Debug.LogWarning("[SceneDoorBootstrap] Could not parse Pedestal OBJ mesh: " + ex.Message);



                return null;



            }



        }











        private static void SetupModernEntertainmentCenter()



        {



            GameObject mec = GameObject.Find("Modern_Entertainment_Center");



            if (mec == null)



            {



                mec = new GameObject("Modern_Entertainment_Center");



                mec.transform.position = new Vector3(-2.2f, 0f, -23.0f);



                mec.transform.rotation = Quaternion.Euler(0f, 90f, 0f);



            }







            MeshFilter mf = mec.GetComponent<MeshFilter>();



            if (mf == null) mf = mec.AddComponent<MeshFilter>();



            MeshRenderer mr = mec.GetComponent<MeshRenderer>();



            if (mr == null) mr = mec.AddComponent<MeshRenderer>();







            if (mf.sharedMesh == null || mf.sharedMesh.vertexCount == 0)



            {



                Mesh loadedMesh = LoadModernEntertainmentCenterMesh();



                if (loadedMesh != null)



                {



                    mf.sharedMesh = loadedMesh;



                }



            }







            Material[] mats = mr.sharedMaterials;



            bool needMats = mats == null || mats.Length < 4 || mats[0] == null;



            if (needMats)



            {



                mats = new Material[4];



                string[] matNames = { "Screen", "TV_&_Cables", "Stand", "Player_&_Decor" };



                string[] bcNames = { "Screen_Base_Color.png", "TV_&_Cables_Base_Color.png", "Stand_Base_Color.png", "Player_&_Decor_Base_Color.png" };



                string[] normNames = { "Screen_Normal.png", "TV_&_Cables_Normal.png", "Stand_Normal.png", "Player_&_Decor_Normal.png" };



                string[] metNames = { "Screen_MetallicGloss.png", "TV_&_Cables_MetallicGloss.png", "Stand_MetallicGloss.png", "Player_&_Decor_MetallicGloss.png" };



                string[] aoNames = { "Screen_Mixed_AO.png", "TV_&_Cables_Mixed_AO.png", "Stand_Mixed_AO.png", "Player_&_Decor_Mixed_AO.png" };







                Shader stdShader = Shader.Find("Standard");



                if (stdShader == null) stdShader = Shader.Find("Universal Render Pipeline/Lit");



                if (stdShader == null) stdShader = Shader.Find("Mobile/Diffuse");







                string basePath = System.IO.Path.Combine(Application.dataPath, "modern-entertainment-center-free");



                string texPath = System.IO.Path.Combine(basePath, "textures");







                for (int i = 0; i < 4; i++)



                {



                    Material m = new Material(stdShader);



                    m.name = matNames[i];



                    m.EnableKeyword("_METALLICGLOSSMAP");



                    m.EnableKeyword("_NORMALMAP");



                    m.SetFloat("_Glossiness", 0.95f);



                    m.SetFloat("_Metallic", 0.85f);







                    string fullBc = System.IO.Path.Combine(texPath, bcNames[i]);



                    if (System.IO.File.Exists(fullBc))



                    {



                        byte[] b = System.IO.File.ReadAllBytes(fullBc);



                        Texture2D t = new Texture2D(2, 2, TextureFormat.RGBA32, true);



                        if (t.LoadImage(b)) { t.Apply(true, false); m.mainTexture = t; }



                    }







                    string fullNorm = System.IO.Path.Combine(texPath, normNames[i]);



                    if (System.IO.File.Exists(fullNorm))



                    {



                        byte[] b = System.IO.File.ReadAllBytes(fullNorm);



                        Texture2D t = new Texture2D(2, 2, TextureFormat.RGBA32, true, true);



                        if (t.LoadImage(b)) { t.Apply(true, false); m.SetTexture("_BumpMap", t); }



                    }







                    string fullMet = System.IO.Path.Combine(texPath, metNames[i]);



                    if (System.IO.File.Exists(fullMet))



                    {



                        byte[] b = System.IO.File.ReadAllBytes(fullMet);



                        Texture2D t = new Texture2D(2, 2, TextureFormat.RGBA32, true);



                        if (t.LoadImage(b)) { t.Apply(true, false); m.SetTexture("_MetallicGlossMap", t); }



                    }







                    string fullAo = System.IO.Path.Combine(texPath, aoNames[i]);



                    if (System.IO.File.Exists(fullAo))



                    {



                        byte[] b = System.IO.File.ReadAllBytes(fullAo);



                        Texture2D t = new Texture2D(2, 2, TextureFormat.RGBA32, true);



                        if (t.LoadImage(b)) { t.Apply(true, false); m.SetTexture("_OcclusionMap", t); }



                    }







                    if (i == 1) // TV & Cables Emissive LED



                    {



                        m.EnableKeyword("_EMISSION");



                        m.SetColor("_EmissionColor", new Color(2.5f, 0.1f, 0.1f, 1f));



                        string fullEm = System.IO.Path.Combine(texPath, "TV_&_Cables_Emissive.png");



                        if (System.IO.File.Exists(fullEm))



                        {



                            byte[] b = System.IO.File.ReadAllBytes(fullEm);



                            Texture2D t = new Texture2D(2, 2, TextureFormat.RGBA32, true);



                            if (t.LoadImage(b)) { t.Apply(true, false); m.SetTexture("_EmissionMap", t); }



                        }



                    }







                    mats[i] = m;



                }



                mr.sharedMaterials = mats;



            }



        }







        private static Mesh LoadModernEntertainmentCenterMesh()



        {



            string objPath = System.IO.Path.Combine(Application.dataPath, "modern-entertainment-center-free", "source", "M-Entertainment.obj");



            if (!System.IO.File.Exists(objPath)) return null;







            try



            {



                var culture = System.Globalization.CultureInfo.InvariantCulture;



                List<Vector3> rawV = new List<Vector3>();



                List<Vector3> rawN = new List<Vector3>();



                List<Vector2> rawUV = new List<Vector2>();







                List<Vector3> outV = new List<Vector3>();



                List<Vector3> outN = new List<Vector3>();



                List<Vector2> outUV = new List<Vector2>();







                List<int>[] subTris = new List<int>[4];



                for (int s = 0; s < 4; s++) subTris[s] = new List<int>();



                int activeSub = 0;







                string[] lines = System.IO.File.ReadAllLines(objPath);



                for (int i = 0; i < lines.Length; i++)



                {



                    string line = lines[i].Trim();



                    if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;







                    if (line.StartsWith("usemtl "))



                    {



                        string matName = line.Substring(7).Trim();



                        if (matName.Contains("Screen")) activeSub = 0;



                        else if (matName.Contains("TV")) activeSub = 1;



                        else if (matName.Contains("Stand")) activeSub = 2;



                        else if (matName.Contains("Player")) activeSub = 3;



                    }



                    else if (line.StartsWith("v "))



                    {



                        string[] p = line.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);



                        if (p.Length >= 4)



                        {



                            rawV.Add(new Vector3(float.Parse(p[1], culture), float.Parse(p[2], culture), float.Parse(p[3], culture)));



                        }



                    }



                    else if (line.StartsWith("vn "))



                    {



                        string[] p = line.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);



                        if (p.Length >= 4)



                        {



                            rawN.Add(new Vector3(float.Parse(p[1], culture), float.Parse(p[2], culture), float.Parse(p[3], culture)));



                        }



                    }



                    else if (line.StartsWith("vt "))



                    {



                        string[] p = line.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);



                        if (p.Length >= 3)



                        {



                            rawUV.Add(new Vector2(float.Parse(p[1], culture), float.Parse(p[2], culture)));



                        }



                    }



                    else if (line.StartsWith("f "))



                    {



                        string[] p = line.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);



                        if (p.Length >= 4)



                        {



                            for (int j = 1; j <= 3; j++)



                            {



                                string[] idx = p[j].Split('/');



                                int vi = int.Parse(idx[0]) - 1;



                                if (vi >= 0 && vi < rawV.Count) outV.Add(rawV[vi]);



                                else outV.Add(Vector3.zero);







                                if (idx.Length > 1 && !string.IsNullOrEmpty(idx[1]) && rawUV.Count > 0)



                                {



                                    int uvi = int.Parse(idx[1]) - 1;



                                    outUV.Add(uvi >= 0 && uvi < rawUV.Count ? rawUV[uvi] : Vector2.zero);



                                }



                                else outUV.Add(Vector2.zero);







                                if (idx.Length > 2 && !string.IsNullOrEmpty(idx[2]) && rawN.Count > 0)



                                {



                                    int ni = int.Parse(idx[2]) - 1;



                                    outN.Add(ni >= 0 && ni < rawN.Count ? rawN[ni] : Vector3.up);



                                }



                                else outN.Add(Vector3.up);







                                subTris[activeSub].Add(outV.Count - 1);



                            }



                        }



                    }



                }







                if (outV.Count == 0) return null;







                Mesh mesh = new Mesh();



                mesh.name = "Modern_Entertainment_Center_Mesh";



                mesh.SetVertices(outV);



                mesh.subMeshCount = 4;



                for (int s = 0; s < 4; s++)



                {



                    mesh.SetTriangles(subTris[s], s);



                }



                if (outN.Count == outV.Count) mesh.SetNormals(outN);



                else mesh.RecalculateNormals();



                if (outUV.Count == outV.Count) mesh.SetUVs(0, outUV);



                mesh.RecalculateBounds();



                return mesh;



            }



            catch (System.Exception ex)



            {



                Debug.LogWarning("[SceneDoorBootstrap] Could not parse MEC OBJ mesh: " + ex.Message);



                return null;



            }



        }



    



        private static void SetupLuxeHangerLamp()



        {



            GameObject lamp = GameObject.Find("Luxe_Hanger_Lamp");



            if (lamp == null)



            {



                lamp = new GameObject("Luxe_Hanger_Lamp");



                lamp.transform.position = new Vector3(0f, 3.48f, 0f);



                lamp.transform.rotation = Quaternion.identity;



            }







            MeshFilter mf = lamp.GetComponent<MeshFilter>();



            if (mf == null) mf = lamp.AddComponent<MeshFilter>();



            MeshRenderer mr = lamp.GetComponent<MeshRenderer>();



            if (mr == null) mr = lamp.AddComponent<MeshRenderer>();







            if (mf.sharedMesh == null || mf.sharedMesh.vertexCount == 0)



            {



                Mesh loadedMesh = LoadLuxeHangerLampMesh();



                if (loadedMesh != null)



                {



                    mf.sharedMesh = loadedMesh;



                }



            }







            Material[] mats = mr.sharedMaterials;



            bool needMats = mats == null || mats.Length < 3 || mats[0] == null;



            if (needMats)



            {



                mats = new Material[3];



                string[] matNames = { "Hanger_BlackMetal", "Hanger_CrystalGlass", "Hanger_BulbGlow" };



                string[] bcNames = { "Hanger_Metal_BaseColor.png", "Hanger_Crystal_BaseColor.png", "Hanger_Bulb_BaseColor.png" };



                string[] normNames = { "Hanger_Metal_Normal.png", "Hanger_Crystal_Normal.png", "Hanger_Bulb_Normal.png" };



                string[] metNames = { "Hanger_Metal_MetallicGloss.png", "Hanger_Crystal_MetallicGloss.png", "Hanger_Bulb_MetallicGloss.png" };



                string[] emNames = { null, null, "Hanger_Bulb_Emissive.png" };







                Shader stdShader = Shader.Find("Standard");



                if (stdShader == null) stdShader = Shader.Find("Universal Render Pipeline/Lit");



                if (stdShader == null) stdShader = Shader.Find("Mobile/Diffuse");







                string basePath = System.IO.Path.Combine(Application.dataPath, "Luxe_Hanger_Lamp");



                string texPath = System.IO.Path.Combine(basePath, "textures");







                for (int i = 0; i < 3; i++)



                {



                    Material m = new Material(stdShader);



                    m.name = matNames[i];



                    m.EnableKeyword("_METALLICGLOSSMAP");



                    m.EnableKeyword("_NORMALMAP");







                    if (i == 0) { m.SetFloat("_Glossiness", 0.52f); m.SetFloat("_Metallic", 0.88f); }



                    else if (i == 1) { m.SetFloat("_Glossiness", 0.96f); m.SetFloat("_Metallic", 0.05f); }



                    else if (i == 2) { m.SetFloat("_Glossiness", 0.92f); m.SetFloat("_Metallic", 0.0f); }







                    string fullBc = System.IO.Path.Combine(texPath, bcNames[i]);



                    if (System.IO.File.Exists(fullBc))



                    {



                        byte[] b = System.IO.File.ReadAllBytes(fullBc);



                        Texture2D t = new Texture2D(2, 2, TextureFormat.RGBA32, true);



                        if (t.LoadImage(b)) { t.Apply(true, false); m.mainTexture = t; }



                    }







                    string fullNorm = System.IO.Path.Combine(texPath, normNames[i]);



                    if (System.IO.File.Exists(fullNorm))



                    {



                        byte[] b = System.IO.File.ReadAllBytes(fullNorm);



                        Texture2D t = new Texture2D(2, 2, TextureFormat.RGBA32, true, true);



                        if (t.LoadImage(b)) { t.Apply(true, false); m.SetTexture("_BumpMap", t); }



                    }







                    string fullMet = System.IO.Path.Combine(texPath, metNames[i]);



                    if (System.IO.File.Exists(fullMet))



                    {



                        byte[] b = System.IO.File.ReadAllBytes(fullMet);



                        Texture2D t = new Texture2D(2, 2, TextureFormat.RGBA32, true);



                        if (t.LoadImage(b)) { t.Apply(true, false); m.SetTexture("_MetallicGlossMap", t); }



                    }







                    if (emNames[i] != null)



                    {



                        string fullEm = System.IO.Path.Combine(texPath, emNames[i]);



                        if (System.IO.File.Exists(fullEm))



                        {



                            byte[] b = System.IO.File.ReadAllBytes(fullEm);



                            Texture2D t = new Texture2D(2, 2, TextureFormat.RGBA32, true);



                            if (t.LoadImage(b)) { t.Apply(true, false); m.SetTexture("_EmissionMap", t); }



                        }



                        m.EnableKeyword("_EMISSION");



                        m.SetColor("_EmissionColor", new Color(2.5f, 1.9f, 0.85f));



                    }







                    mats[i] = m;



                }



                mr.sharedMaterials = mats;



            }







            Transform lightT = lamp.transform.Find("PointLight_WarmGlow");



            GameObject lightObj = (lightT != null) ? lightT.gameObject : new GameObject("PointLight_WarmGlow");



            if (lightT == null)



            {



                lightObj.transform.SetParent(lamp.transform, false);



                lightObj.transform.localPosition = new Vector3(0f, 0.05f, 0f);



            }



            Light lt = lightObj.GetComponent<Light>();



            if (lt == null) lt = lightObj.AddComponent<Light>();



            lt.type = LightType.Point;



            lt.color = new Color(1.0f, 0.82f, 0.55f);



            lt.intensity = 1.8f;



            lt.range = 10f;



            lt.shadows = LightShadows.Soft;



            lt.enabled = true;







            lamp.SetActive(true);



            Debug.Log("[SceneDoorBootstrap] Luxe_Hanger_Lamp initialized at (0, 3.48, 0)!");



        }







        private static Mesh LoadLuxeHangerLampMesh()



        {



            string objPath = System.IO.Path.Combine(Application.dataPath, "Luxe_Hanger_Lamp", "source", "Luxe_Hanger_Lamp.obj");



            if (!System.IO.File.Exists(objPath)) return null;







            try



            {



                var lines = System.IO.File.ReadAllLines(objPath);



                var positions = new System.Collections.Generic.List<Vector3>();



                var uvs = new System.Collections.Generic.List<Vector2>();



                var normals = new System.Collections.Generic.List<Vector3>();







                var finalPositions = new System.Collections.Generic.List<Vector3>();



                var finalUvs = new System.Collections.Generic.List<Vector2>();



                var finalNormals = new System.Collections.Generic.List<Vector3>();







                var submeshTriangles = new System.Collections.Generic.List<System.Collections.Generic.List<int>>



                {



                    new System.Collections.Generic.List<int>(),



                    new System.Collections.Generic.List<int>(),



                    new System.Collections.Generic.List<int>()



                };







                int currentSubmesh = 0;



                var vertMap = new System.Collections.Generic.Dictionary<string, int>();







                foreach (var line in lines)



                {



                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;



                    if (line.StartsWith("v "))



                    {



                        var parts = line.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);



                        if (parts.Length >= 4)



                        {



                            positions.Add(new Vector3(



                                float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture),



                                float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture),



                                float.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture)



                            ));



                        }



                    }



                    else if (line.StartsWith("vt "))



                    {



                        var parts = line.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);



                        if (parts.Length >= 3)



                        {



                            uvs.Add(new Vector2(



                                float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture),



                                float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture)



                            ));



                        }



                    }



                    else if (line.StartsWith("vn "))



                    {



                        var parts = line.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);



                        if (parts.Length >= 4)



                        {



                            normals.Add(new Vector3(



                                float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture),



                                float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture),



                                float.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture)



                            ));



                        }



                    }



                    else if (line.StartsWith("usemtl "))



                    {



                        string mat = line.Substring(7).Trim();



                        if (mat.Contains("Metal")) currentSubmesh = 0;



                        else if (mat.Contains("Crystal")) currentSubmesh = 1;



                        else if (mat.Contains("Bulb")) currentSubmesh = 2;



                    }



                    else if (line.StartsWith("f "))



                    {



                        var tokens = line.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);



                        if (tokens.Length < 4) continue;







                        int[] faceIndices = new int[tokens.Length - 1];



                        for (int t = 1; t < tokens.Length; t++)



                        {



                            string token = tokens[t];



                            if (!vertMap.TryGetValue(token, out int finalIdx))



                            {



                                var subTokens = token.Split('/');



                                int vIdx = int.Parse(subTokens[0]) - 1;



                                int vtIdx = subTokens.Length > 1 && !string.IsNullOrEmpty(subTokens[1]) ? int.Parse(subTokens[1]) - 1 : -1;



                                int vnIdx = subTokens.Length > 2 && !string.IsNullOrEmpty(subTokens[2]) ? int.Parse(subTokens[2]) - 1 : -1;







                                finalPositions.Add(positions[vIdx]);



                                finalUvs.Add(vtIdx >= 0 && vtIdx < uvs.Count ? uvs[vtIdx] : Vector2.zero);



                                finalNormals.Add(vnIdx >= 0 && vnIdx < normals.Count ? normals[vnIdx] : Vector3.up);







                                finalIdx = finalPositions.Count - 1;



                                vertMap[token] = finalIdx;



                            }



                            faceIndices[t - 1] = finalIdx;



                        }







                        // Triangulate polygon fan



                        for (int k = 1; k < faceIndices.Length - 1; k++)



                        {



                            submeshTriangles[currentSubmesh].Add(faceIndices[0]);



                            submeshTriangles[currentSubmesh].Add(faceIndices[k]);



                            submeshTriangles[currentSubmesh].Add(faceIndices[k + 1]);



                        }



                    }



                }







                Mesh mesh = new Mesh();



                mesh.name = "Luxe_Hanger_Lamp_Mesh";



                if (finalPositions.Count > 65535) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;



                mesh.SetVertices(finalPositions);



                mesh.SetUVs(0, finalUvs);



                mesh.SetNormals(finalNormals);



                mesh.subMeshCount = 3;



                for (int s = 0; s < 3; s++)



                {



                    mesh.SetTriangles(submeshTriangles[s], s);



                }



                mesh.RecalculateBounds();



                return mesh;



            }



            catch (System.Exception ex)



            {



                Debug.LogError("[SceneDoorBootstrap] Failed to parse Luxe_Hanger_Lamp.obj: " + ex.Message);



                return null;



            }



        }







        private static void SetupStarcourtMall()



        {



            GameObject mall = GameObject.Find("Starcourt_Mall_Atrium");



            if (mall == null) mall = GameObject.Find("Starcourt_Mall");



            if (mall == null) return;







            MeshRenderer mr = mall.GetComponent<MeshRenderer>();



            if (mr == null) return;







            Material[] mats = mr.sharedMaterials;



            bool needMats = mats == null || mats.Length < 9 || mats[0] == null;



            if (needMats)



            {



                Shader stdShader = Shader.Find("Standard");



                if (stdShader == null) stdShader = Shader.Find("Universal Render Pipeline/Lit");



                if (stdShader == null) stdShader = Shader.Find("Mobile/Diffuse");







                string basePath = System.IO.Path.Combine(Application.dataPath, "Starcourt_Mall");



                string texPath = System.IO.Path.Combine(basePath, "textures");







                string[] matNames = {



                    "Starcourt_Sign", "Starcourt_Pillars", "Starcourt_Storefronts_Upper", "Starcourt_Storefronts_Lower",



                    "Starcourt_Floors_Carpet", "Starcourt_Floors_Tiles", "Starcourt_Railings_Escalators", "Starcourt_Roof_Truss", "Starcourt_Glass"



                };



                string[] bcNames = {



                    "Starcourt_Sign_BaseColor.png", "Starcourt_Pillars_BaseColor.png", "Starcourt_Storefronts_Upper_BaseColor.png", "Starcourt_Storefronts_Lower_BaseColor.png",



                    "Starcourt_Carpet_BaseColor.png", "Starcourt_Tiles_BaseColor.png", "Starcourt_Railings_BaseColor.png", "Starcourt_Truss_BaseColor.png", "Starcourt_Glass_BaseColor.png"



                };



                string[] normNames = {



                    "Starcourt_Sign_Normal.png", "Starcourt_Pillars_Normal.png", "Starcourt_Storefronts_Upper_Normal.png", "Starcourt_Storefronts_Lower_Normal.png",



                    "Starcourt_Carpet_Normal.png", "Starcourt_Tiles_Normal.png", "Starcourt_Railings_Normal.png", "Starcourt_Truss_Normal.png", "Starcourt_Glass_Normal.png"



                };



                string[] emNames = {



                    "Starcourt_Sign_Emissive.png", "Starcourt_Pillars_Emissive.png", "Starcourt_Storefronts_Upper_Emissive.png", "Starcourt_Storefronts_Lower_Emissive.png",



                    null, null, "Starcourt_Railings_Emissive.png", null, null



                };







                mats = new Material[9];



                for (int i = 0; i < 9; i++)



                {



                    Material m = new Material(stdShader);



                    m.name = matNames[i];



                    m.EnableKeyword("_METALLICGLOSSMAP");



                    m.EnableKeyword("_NORMALMAP");







                    string fullBc = System.IO.Path.Combine(texPath, bcNames[i]);



                    if (System.IO.File.Exists(fullBc))



                    {



                        byte[] b = System.IO.File.ReadAllBytes(fullBc);



                        Texture2D t = new Texture2D(2, 2, TextureFormat.RGBA32, true);



                        t.name = bcNames[i];



                        t.LoadImage(b);



                        m.mainTexture = t;



                    }







                    string fullNorm = System.IO.Path.Combine(texPath, normNames[i]);



                    if (System.IO.File.Exists(fullNorm))



                    {



                        byte[] b = System.IO.File.ReadAllBytes(fullNorm);



                        Texture2D t = new Texture2D(2, 2, TextureFormat.RGBA32, true, true);



                        t.name = normNames[i];



                        t.LoadImage(b);



                        m.SetTexture("_BumpMap", t);



                    }







                    if (!string.IsNullOrEmpty(emNames[i]))



                    {



                        string fullEm = System.IO.Path.Combine(texPath, emNames[i]);



                        if (System.IO.File.Exists(fullEm))



                        {



                            byte[] b = System.IO.File.ReadAllBytes(fullEm);



                            Texture2D t = new Texture2D(2, 2, TextureFormat.RGBA32, true);



                            t.name = emNames[i];



                            t.LoadImage(b);



                            m.EnableKeyword("_EMISSION");



                            m.SetTexture("_EmissionMap", t);



                            m.SetColor("_EmissionColor", Color.white * 1.5f);



                        }



                    }







                    mats[i] = m;



                }



                mr.sharedMaterials = mats;



                Debug.Log("[SceneDoorBootstrap] Starcourt Mall materials verified and applied at runtime.");



            }



        }







        private static void SetupIceScreamMall()



        {



            GameObject mall = GameObject.Find("Ice_Scream_Shopping_Center_Map");



            if (mall == null) mall = GameObject.Find("Einkaufszentrum Map Ice Scream 3 FBX");



            if (mall == null) return;







            // Respect manual editor layout for mall root transform:
            // if (mall.transform.position.y < 0.01f) { Vector3 p = mall.transform.position; p.y = 0.02f; mall.transform.position = p; }







            Transform lightT = mall.transform.Find("Mall_Ambient_Lighting");



            if (lightT != null)



            {



                Light lt = lightT.GetComponent<Light>();



                if (lt != null)



                {



                    lt.shadows = LightShadows.None;



                    lt.range = 250f;



                    lt.intensity = 0.35f;



                }



            }







            // Disable roof shadows to prevent cascade clipping at runtime



            MeshRenderer[] renderers = mall.GetComponentsInChildren<MeshRenderer>(true);







            #if UNITY_EDITOR



            Material cartelMat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/New Material.mat");



            if (cartelMat != null)



            {



                for (int i = 0; i < renderers.Length; i++)



                {



                    if (renderers[i] != null && renderers[i].name == "Cartel15" && renderers[i].sharedMaterial != cartelMat)



                    {



                        renderers[i].sharedMaterial = cartelMat;



                    }



                }



            }



            #endif







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



                        mr.enabled = false;



                        mr.gameObject.SetActive(false);



                    }



                }



            }







            foreach (var mr in renderers)



            {



                if (mr == null) continue;



                string nLower = mr.gameObject.name.ToLower();



                if (nLower.Contains("techo") || nLower.Contains("roof") || nLower.Contains("cubierta") || nLower.Contains("ceiling"))



                {



                    mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;



                }



            }







            // Calibrate indoor ambient lighting



            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;



            RenderSettings.ambientSkyColor = new Color(0.48f, 0.50f, 0.55f, 1f);



            RenderSettings.ambientEquatorColor = new Color(0.38f, 0.40f, 0.44f, 1f);



            RenderSettings.ambientGroundColor = new Color(0.30f, 0.32f, 0.35f, 1f);



            RenderSettings.ambientIntensity = 1f;







            RenderSettings.fog = false;







            Camera[] sceneCams = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);



            foreach (var cam in sceneCams)



            {



                if (cam != null)



                {



                    cam.clearFlags = CameraClearFlags.Skybox;



                    cam.backgroundColor = new Color(0.19215687f, 0.3019608f, 0.4745098f, 0f);



                }



            }







            Light[] sceneLights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);



            foreach (var l in sceneLights)



            {



                if (l != null && l.type == LightType.Directional)



                {



                    l.color = new Color(1f, 0.95686275f, 0.8392157f, 1f); // warm daytime sun



                    l.intensity = 0.65f;



                    if (l.shadows != LightShadows.None)



                    {



                        l.shadowStrength = 0.25f;



                        l.shadowBias = 0.05f;



                        l.shadowNormalBias = 0.4f;



                    }



                }



            }







            Debug.Log("[SceneDoorBootstrap] Ice Scream Shopping Center Map initialized with anti-glitch runtime settings.");



        }







        private static void SetupDeskTableWhite()
        {
            GameObject desk = GameObject.Find("Desk Table White");
            if (desk != null)
            {
                // Respect manual editor layout for Desk Table White:
                // if (desk.transform.position.x < -3.5f || desk.transform.position.y < 0f) { desk.transform.position = new Vector3(-3.2f, 0.02f, -19.6f); desk.transform.rotation = Quaternion.Euler(0f, 90f, 0f); }
                desk.SetActive(true);

                Transform drawerT = desk.transform.Find("desk_drawer");
                if (drawerT != null)
                {
                    // Ensure drawer collider covers only the front/handle so it never blocks the interior tray
                    BoxCollider drawerCol = drawerT.GetComponent<BoxCollider>();
                    if (drawerCol != null)
                    {
                        drawerCol.center = new Vector3(0.01f, 0.11f, 0.02f);
                        drawerCol.size = new Vector3(0.40f, 0.22f, 0.06f);
                    }

                    DeskTableWhiteDrawerController drawerCtrl = drawerT.GetComponent<DeskTableWhiteDrawerController>();
                    if (drawerCtrl == null)
                    {
                        drawerCtrl = drawerT.gameObject.AddComponent<DeskTableWhiteDrawerController>();
                    }

                    Transform keyT = drawerT.Find("Stylized_Key");
                    if (keyT != null)
                    {
                        keyT.localPosition = new Vector3(0.009f, 0.065f, -0.15f);
                        keyT.localRotation = Quaternion.Euler(0f, 90f, 0f);
                        keyT.localScale = Vector3.one;

                        BoxCollider keyCol = keyT.GetComponent<BoxCollider>();
                        if (keyCol != null)
                        {
                            keyCol.center = Vector3.zero;
                            keyCol.size = new Vector3(0.12f, 0.08f, 0.25f);
                        }

                        Light keyLight = keyT.GetComponent<Light>();
                        if (keyLight == null)
                        {
                            keyLight = keyT.gameObject.AddComponent<Light>();
                            keyLight.type = LightType.Point;
                            keyLight.color = new Color(1f, 0.85f, 0.45f);
                            keyLight.range = 0.75f;
                            keyLight.intensity = 1.2f;
                        }

                        KeyPickup kp = keyT.GetComponent<KeyPickup>();
                        if (kp == null) kp = keyT.gameObject.AddComponent<KeyPickup>();
                        drawerCtrl.BindKey(kp);
                    }
                }
            }

            // Decommission old desk interactive elements at runtime
            GameObject oldDesk = GameObject.Find("Desk");
            if (oldDesk != null)
            {
                Transform oldDrawerKey = oldDesk.transform.Find("Drawer_Key");
                if (oldDrawerKey != null)
                {
                    Transform oldKey = oldDrawerKey.Find("Key");
                    if (oldKey != null) Object.Destroy(oldKey.gameObject);
                    DrawerController oldDc = oldDrawerKey.GetComponent<DrawerController>();
                    if (oldDc != null) Object.Destroy(oldDc);
                }
                Transform oldDrawerEmpty = oldDesk.transform.Find("Drawer_Empty");
                if (oldDrawerEmpty != null)
                {
                    DrawerController oldDcEmpty = oldDrawerEmpty.GetComponent<DrawerController>();
                    if (oldDcEmpty != null) Object.Destroy(oldDcEmpty);
                }
            }
        }




        private static void SetupReplacementDoors()
        {
            // 1. Door_10_White (replaces Door_R2_R3: Room 2 -> Room 3, Password cipher 4729, opens inward)
            GameObject whiteDoorObj = GameObject.Find("Door_10_White");
            if (whiteDoorObj == null)
            {
                foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    if (root.name.IndexOf("Door_10_White", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        whiteDoorObj = root;
                        break;
                    }
                }
            }

            if (whiteDoorObj != null)
            {
                whiteDoorObj.SetActive(true);
                Transform doorLeaf = whiteDoorObj.transform.Find("Door");
                if (doorLeaf != null)
                {
                    // Disable conflicting third-party Door script
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

                DoorController dcWhite = whiteDoorObj.GetComponent<DoorController>();
                if (dcWhite == null) dcWhite = whiteDoorObj.AddComponent<DoorController>();
                dcWhite.ConfigureHinge(doorLeaf, 0f, -90f, true, "Door_10_White");

                ProgressionDoor pdWhite = whiteDoorObj.GetComponent<ProgressionDoor>();
                if (pdWhite == null) pdWhite = whiteDoorObj.AddComponent<ProgressionDoor>();
                pdWhite.Bind(dcWhite, DoorRequirement.RequiresPassword, false, 3);

                CipherPuzzle cipher = Object.FindFirstObjectByType<CipherPuzzle>(FindObjectsInactive.Include);
                if (cipher != null)
                {
                    KeypadUI keypad = Object.FindFirstObjectByType<KeypadUI>(FindObjectsInactive.Include);
                    cipher.Bind(keypad, pdWhite);
                    pdWhite.BindCipher(cipher);
                }

                // Auto-close and lock Door_10_White when player enters Room 3
                GameObject trap3Obj = GameObject.Find("Room3TrapTrigger");
                if (trap3Obj == null)
                {
                    trap3Obj = new GameObject("Room3TrapTrigger");
                    Vector3 inside = whiteDoorObj.transform.position + Vector3.forward * 2.7f;
                    inside.y = whiteDoorObj.transform.position.y + 1.5f;
                    trap3Obj.transform.position = inside;
                }

                if (trap3Obj != null)
                {
                    BoxCollider box = trap3Obj.GetComponent<BoxCollider>();
                    if (box == null) box = trap3Obj.AddComponent<BoxCollider>();
                    box.isTrigger = true;
                    box.size = new Vector3(6f, 3.5f, 2.5f);
                    box.center = Vector3.zero;

                    RoomTrapTrigger trap = trap3Obj.GetComponent<RoomTrapTrigger>();
                    if (trap == null) trap = trap3Obj.AddComponent<RoomTrapTrigger>();
                    trap.Bind(dcWhite, RoomId.Room3, true);
                }

                GameObject oldR2Door = GameObject.Find("Door_R2_R3");
                if (oldR2Door != null && oldR2Door != whiteDoorObj)
                {
                    oldR2Door.SetActive(false);
                }
            }

            // 2. Door_13_Black (replaces Door_R3_R4: Room 3 -> Room 4, 5 Gems required, auto-opens inward)
            GameObject black13DoorObj = GameObject.Find("Door_13_Black");
            if (black13DoorObj == null)
            {
                foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    if (root.name.IndexOf("Door_13_Black", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        black13DoorObj = root;
                        break;
                    }
                }
            }

            if (black13DoorObj != null)
            {
                black13DoorObj.SetActive(true);
                Transform doorLeaf = black13DoorObj.transform.Find("Door");
                if (doorLeaf != null)
                {
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

                DoorController dc13 = black13DoorObj.GetComponent<DoorController>();
                if (dc13 == null) dc13 = black13DoorObj.AddComponent<DoorController>();
                dc13.ConfigureHinge(doorLeaf, 0f, -90f, true, "Door_13_Black");

                ProgressionDoor pd13 = black13DoorObj.GetComponent<ProgressionDoor>();
                if (pd13 == null) pd13 = black13DoorObj.AddComponent<ProgressionDoor>();
                pd13.Bind(dc13, DoorRequirement.RequiresGems, true, 4);

                // Auto-close and lock Door_13_Black when player enters Room 4
                GameObject trap4Obj = GameObject.Find("Room4TrapTrigger");
                if (trap4Obj == null)
                {
                    trap4Obj = new GameObject("Room4TrapTrigger");
                    Vector3 inside = black13DoorObj.transform.position + Vector3.forward * 2.7f;
                    inside.y = black13DoorObj.transform.position.y + 1.5f;
                    trap4Obj.transform.position = inside;
                }

                if (trap4Obj != null)
                {
                    BoxCollider box = trap4Obj.GetComponent<BoxCollider>();
                    if (box == null) box = trap4Obj.AddComponent<BoxCollider>();
                    box.isTrigger = true;
                    box.size = new Vector3(6f, 3.5f, 2.5f);
                    box.center = Vector3.zero;

                    RoomTrapTrigger trap = trap4Obj.GetComponent<RoomTrapTrigger>();
                    if (trap == null) trap = trap4Obj.AddComponent<RoomTrapTrigger>();
                    trap.Bind(dc13, RoomId.Room4, true);
                }

                GameObject oldR3Door = GameObject.Find("Door_R3_R4");
                if (oldR3Door != null && oldR3Door != black13DoorObj)
                {
                    oldR3Door.SetActive(false);
                }
            }

            // 3. Door_10_Black (replaces Door_R4_R5: Room 4 -> Room 5, Color sequence, auto-opens inward)
            GameObject black10DoorObj = GameObject.Find("Door_10_Black");
            if (black10DoorObj == null)
            {
                foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    if (root.name.IndexOf("Door_10_Black", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        black10DoorObj = root;
                        break;
                    }
                }
            }

            if (black10DoorObj != null)
            {
                black10DoorObj.SetActive(true);
                Transform doorLeaf = black10DoorObj.transform.Find("Door");
                if (doorLeaf != null)
                {
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

                DoorController dc10 = black10DoorObj.GetComponent<DoorController>();
                if (dc10 == null) dc10 = black10DoorObj.AddComponent<DoorController>();
                dc10.ConfigureHinge(doorLeaf, 0f, -90f, true, "Door_10_Black");

                ProgressionDoor pd10 = black10DoorObj.GetComponent<ProgressionDoor>();
                if (pd10 == null) pd10 = black10DoorObj.AddComponent<ProgressionDoor>();
                pd10.Bind(dc10, DoorRequirement.RequiresColorSequence, true, 5);

                // Auto-close and lock Door_10_Black when player enters Room 5
                GameObject trap5Obj = GameObject.Find("Room5TrapTrigger");
                if (trap5Obj == null)
                {
                    trap5Obj = new GameObject("Room5TrapTrigger");
                    Vector3 inside = black10DoorObj.transform.position + Vector3.forward * 2.7f;
                    inside.y = black10DoorObj.transform.position.y + 1.5f;
                    trap5Obj.transform.position = inside;
                }

                if (trap5Obj != null)
                {
                    BoxCollider box = trap5Obj.GetComponent<BoxCollider>();
                    if (box == null) box = trap5Obj.AddComponent<BoxCollider>();
                    box.isTrigger = true;
                    box.size = new Vector3(6f, 3.5f, 2.5f);
                    box.center = Vector3.zero;

                    RoomTrapTrigger trap = trap5Obj.GetComponent<RoomTrapTrigger>();
                    if (trap == null) trap = trap5Obj.AddComponent<RoomTrapTrigger>();
                    trap.Bind(dc10, RoomId.Room5, true);
                }

                GameObject oldR4Door = GameObject.Find("Door_R4_R5");
                if (oldR4Door != null && oldR4Door != black10DoorObj)
                {
                    oldR4Door.SetActive(false);
                }
            }

            // 4. Door_15_Coffe / UniqueDoor (replaces Door_R5_R6: Room 5 -> Room 6, 4 Shapes required, auto-opens inward)
            GameObject door15Obj = GameObject.Find("Door_15_Coffe");
            if (door15Obj == null) door15Obj = GameObject.Find("Door_15_Coffee");
            if (door15Obj == null)
            {
                foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    if (root.name.IndexOf("Door_15", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        door15Obj = root;
                        break;
                    }
                }
            }

            GameObject uniqueDoorObj = GameObject.Find("UniqueDoor");
            if (uniqueDoorObj == null)
            {
                foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    if (root.name.IndexOf("UniqueDoor", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        uniqueDoorObj = root;
                        break;
                    }
                }
            }

#if UNITY_EDITOR
            if (door15Obj == null)
            {
                var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Free Wood Door Pack/Prefab/Wood/Door_15/Door_15_Coffe.prefab");
                if (prefab != null)
                {
                    door15Obj = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
                    door15Obj.name = "Door_15_Coffe";
                    if (uniqueDoorObj != null)
                    {
                        door15Obj.transform.position = uniqueDoorObj.transform.position;
                        door15Obj.transform.rotation = uniqueDoorObj.transform.rotation;
                        door15Obj.transform.localScale = uniqueDoorObj.transform.localScale;
                    }
                    else
                    {
                        door15Obj.transform.position = new Vector3(-0.03f, 0.05f, 24.775f);
                        door15Obj.transform.rotation = Quaternion.identity;
                    }
                }
            }
#endif

            if (door15Obj != null)
            {
                door15Obj.SetActive(true);
                Transform doorLeaf = door15Obj.transform.Find("Door");
                if (doorLeaf == null)
                {
                    foreach (Transform child in door15Obj.transform)
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
                    // Disable default Free Wood Door Pack mouse-click script so only puzzle progression opens it
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

                MonoBehaviour[] rootMbs = door15Obj.GetComponents<MonoBehaviour>();
                for (int i = 0; i < rootMbs.Length; i++)
                {
                    if (rootMbs[i] != null && rootMbs[i].GetType().Name == "Door")
                    {
                        rootMbs[i].enabled = false;
                    }
                }

                DoorController dc15 = door15Obj.GetComponent<DoorController>();
                if (dc15 == null) dc15 = door15Obj.AddComponent<DoorController>();
                dc15.ConfigureHinge(doorLeaf, 0f, -90f, true, "Door_15_Coffe");

                ProgressionDoor pd15 = door15Obj.GetComponent<ProgressionDoor>();
                if (pd15 == null) pd15 = door15Obj.AddComponent<ProgressionDoor>();
                pd15.Bind(dc15, DoorRequirement.RequiresShapes, true, 6);

                // Auto-close and lock Door_15_Coffe when player enters Room 6
                GameObject trap6Obj = GameObject.Find("Room6TrapTrigger");
                if (trap6Obj == null)
                {
                    trap6Obj = new GameObject("Room6TrapTrigger");
                    Vector3 inside = door15Obj.transform.position + Vector3.forward * 2.7f;
                    inside.y = door15Obj.transform.position.y + 1.5f;
                    trap6Obj.transform.position = inside;
                }

                if (trap6Obj != null)
                {
                    BoxCollider box = trap6Obj.GetComponent<BoxCollider>();
                    if (box == null) box = trap6Obj.AddComponent<BoxCollider>();
                    box.isTrigger = true;
                    box.size = new Vector3(6f, 3.5f, 2.5f);
                    box.center = Vector3.zero;

                    RoomTrapTrigger trap = trap6Obj.GetComponent<RoomTrapTrigger>();
                    if (trap == null) trap = trap6Obj.AddComponent<RoomTrapTrigger>();
                    trap.Bind(dc15, RoomId.Room6, true);
                }

                if (uniqueDoorObj != null && uniqueDoorObj != door15Obj)
                {
                    uniqueDoorObj.SetActive(false);
                }

                GameObject oldR5Door = GameObject.Find("Door_R5_R6");
                if (oldR5Door != null && oldR5Door != door15Obj)
                {
                    oldR5Door.SetActive(false);
                }
            }
            else if (uniqueDoorObj != null)
            {
                uniqueDoorObj.SetActive(true);
                Transform frame001 = uniqueDoorObj.transform.Find("Frame.001"); // Right leaf
                Transform frame002 = uniqueDoorObj.transform.Find("Frame.002"); // Left leaf

                if (frame001 != null)
                {
                    BoxCollider col1 = frame001.GetComponent<BoxCollider>();
                    if (col1 == null) col1 = frame001.gameObject.AddComponent<BoxCollider>();
                    col1.center = new Vector3(-0.64f, 1.25f, 0f);
                    col1.size = new Vector3(1.28f, 2.5f, 0.15f);
                    col1.enabled = true;
                }

                if (frame002 != null)
                {
                    BoxCollider col2 = frame002.GetComponent<BoxCollider>();
                    if (col2 == null) col2 = frame002.gameObject.AddComponent<BoxCollider>();
                    col2.center = new Vector3(0.64f, 1.25f, 0f);
                    col2.size = new Vector3(1.28f, 2.5f, 0.15f);
                    col2.enabled = true;
                }

                DoorController dcUnique = uniqueDoorObj.GetComponent<DoorController>();
                if (dcUnique == null) dcUnique = uniqueDoorObj.AddComponent<DoorController>();
                // Double hinge: Frame.001 (right) opens inward (+90 deg), Frame.002 (left) opens inward (-90 deg)
                dcUnique.ConfigureDoubleHinge(frame001, frame002, 0f, 90f, -90f, true, "UniqueDoor");

                ProgressionDoor pdUnique = uniqueDoorObj.GetComponent<ProgressionDoor>();
                if (pdUnique == null) pdUnique = uniqueDoorObj.AddComponent<ProgressionDoor>();
                pdUnique.Bind(dcUnique, DoorRequirement.RequiresShapes, true, 6);

                // Auto-close and lock UniqueDoor when player enters Room 6
                GameObject trap6Obj = GameObject.Find("Room6TrapTrigger");
                if (trap6Obj == null)
                {
                    trap6Obj = new GameObject("Room6TrapTrigger");
                    Vector3 inside = uniqueDoorObj.transform.position + Vector3.forward * 2.7f;
                    inside.y = uniqueDoorObj.transform.position.y + 1.5f;
                    trap6Obj.transform.position = inside;
                }

                if (trap6Obj != null)
                {
                    BoxCollider box = trap6Obj.GetComponent<BoxCollider>();
                    if (box == null) box = trap6Obj.AddComponent<BoxCollider>();
                    box.isTrigger = true;
                    box.size = new Vector3(6f, 3.5f, 2.5f);
                    box.center = Vector3.zero;

                    RoomTrapTrigger trap = trap6Obj.GetComponent<RoomTrapTrigger>();
                    if (trap == null) trap = trap6Obj.AddComponent<RoomTrapTrigger>();
                    trap.Bind(dcUnique, RoomId.Room6, true);
                }

                GameObject oldR5Door = GameObject.Find("Door_R5_R6");
                if (oldR5Door != null && oldR5Door != uniqueDoorObj)
                {
                    oldR5Door.SetActive(false);
                }
            }

            Debug.Log("[SceneDoorBootstrap] Configured replacement doors (Door_10_White, Door_13_Black, Door_10_Black, Door_15_Coffe / UniqueDoor).");
        }

        private static void EnsurePlayerStartingPosition()
        {
            // Do not override player position - preserve user placement at Main Camera
        }

        private static void SetupRoom5ShapesAndSockets()
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

            // 3. Configure the 4 Sockets & Labels: Socket_Traingle/Triangle, Socket_Star, Socket_Circle, Socket_Square
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

            Debug.Log("[SceneDoorBootstrap] Successfully configured Room 5 shapes, sockets, and labels.");
        }
    }
}
