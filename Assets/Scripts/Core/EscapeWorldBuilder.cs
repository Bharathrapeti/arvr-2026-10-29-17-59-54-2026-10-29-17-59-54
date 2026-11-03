using UnityEngine;
using UnityEngine.UI;
using AREscapeRoom.Core;
using AREscapeRoom.Doors;
using AREscapeRoom.Player;
using AREscapeRoom.Puzzles;
using AREscapeRoom.UI;

namespace AREscapeRoom.Core
{
    public static class EscapeWorldBuilder
    {
        public static void Install()
        {
            if (GameObject.Find("EntranceDoor_Sliding") == null &&
                GameObject.Find("Door_5_Brown") == null &&
                GameObject.Find("AR_Escape_Room_Structure") == null)
            {
                return;
            }

            EnsureGameSystems();
            DisableLegacyKeypadLock();
            ConvertRoom1();

            if (GameObject.Find("Room_2") == null)
            {
                BuildRooms2To6();
            }

            ClearMallBlockingCorridor();

            // FIX ALL WORLD TEXT
            SetupExistingWorldText();

            Debug.Log(
                "[EscapeWorldBuilder] 6-room escape game installed."
            );
        }

        private static void EnsureGameSystems()
        {
            GameManager gm =
                Object.FindFirstObjectByType<GameManager>();

            if (gm == null)
            {
                GameObject go =
                    new GameObject("GameManager");

                gm = go.AddComponent<GameManager>();
            }

            gm.Configure(600f, "4729", 5);

            if (Object.FindFirstObjectByType<HUDController>() == null)
            {
                new GameObject("HUD")
                    .AddComponent<HUDController>();
            }

            if (Object.FindFirstObjectByType<GameEndUI>() == null)
            {
                new GameObject("GameEndUI")
                    .AddComponent<GameEndUI>();
            }

            if (Object.FindFirstObjectByType<KeypadUI>(FindObjectsInactive.Include) == null)
            {
                EnsureRuntimeKeypadUI();
            }

            PlayerController player =
                Object.FindFirstObjectByType<PlayerController>();

            if (player != null &&
                player.GetComponent<PlayerInteraction>() == null)
            {
                player.gameObject.AddComponent<PlayerInteraction>();
            }
        }

        private static void EnsureRuntimeKeypadUI()
        {
            if (Object.FindFirstObjectByType<KeypadUI>(FindObjectsInactive.Include) != null)
                return;

            GameObject canvasObj = new GameObject("KeypadCanvas");
            Canvas c = canvasObj.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 90;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();

            KeypadUI keypadUI = canvasObj.AddComponent<KeypadUI>();

            GameObject bgObj = UiFactory.Panel(canvasObj.transform, "DisplayBackground",
                new Vector2(0.3f, 0.2f), new Vector2(0.7f, 0.8f),
                Vector2.zero, Vector2.zero, new Color(0.1f, 0.1f, 0.1f, 0.95f));

            GameObject statusObj = new GameObject("StatusText");
            statusObj.transform.SetParent(bgObj.transform, false);
            RectTransform statusRect = statusObj.AddComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0f, 0.85f);
            statusRect.anchorMax = new Vector2(1f, 0.95f);
            statusRect.offsetMin = Vector2.zero;
            statusRect.offsetMax = Vector2.zero;
            UiFactory.AddText(statusObj, "ENTER PASSWORD", 24, TextAnchor.MiddleCenter, Color.white);

            GameObject displayObj = UiFactory.Panel(bgObj.transform, "DisplayTextPanel",
                new Vector2(0.1f, 0.68f), new Vector2(0.9f, 0.82f),
                Vector2.zero, Vector2.zero, Color.black);

            GameObject textValObj = new GameObject("TextValue");
            textValObj.transform.SetParent(displayObj.transform, false);
            RectTransform tvRect = textValObj.AddComponent<RectTransform>();
            tvRect.anchorMin = Vector2.zero;
            tvRect.anchorMax = Vector2.one;
            tvRect.offsetMin = Vector2.zero;
            tvRect.offsetMax = Vector2.zero;
            UiFactory.AddText(textValObj, "", 40, TextAnchor.MiddleCenter, Color.green);

            float startX = 0.1f;
            float startY = 0.5f;
            float spacingX = 0.28f;
            float spacingY = 0.15f;
            float w = 0.2f;
            float h = 0.12f;

            int num = 1;
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    CreateKeypadButton(bgObj.transform, $"Btn_{num}", num.ToString(),
                        new Vector2(startX + col * spacingX, startY - row * spacingY),
                        new Vector2(w, h), Color.gray);
                    num++;
                }
            }

            CreateKeypadButton(bgObj.transform, "Btn_CLR", "CLR", new Vector2(startX, startY - 3 * spacingY), new Vector2(w, h), Color.red);
            CreateKeypadButton(bgObj.transform, "Btn_0", "0", new Vector2(startX + spacingX, startY - 3 * spacingY), new Vector2(w, h), Color.gray);
            CreateKeypadButton(bgObj.transform, "Btn_ENT", "ENT", new Vector2(startX + 2 * spacingX, startY - 3 * spacingY), new Vector2(w, h), Color.green);
            CreateKeypadButton(bgObj.transform, "Btn_Close", "X", new Vector2(0.88f, 0.88f), new Vector2(0.1f, 0.1f), Color.red);

            keypadUI.BindButtonListeners();

            if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        private static void CreateKeypadButton(Transform parent, string name, string label, Vector2 position, Vector2 size, Color color)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            RectTransform rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = position;
            rect.anchorMax = position + size;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image img = btnObj.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = true;

            btnObj.AddComponent<Button>();

            GameObject txtObj = new GameObject("Text");
            txtObj.transform.SetParent(btnObj.transform, false);
            RectTransform txtRect = txtObj.AddComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = Vector2.zero;
            txtRect.offsetMax = Vector2.zero;
            Text txt = UiFactory.AddText(txtObj, label, 24, TextAnchor.MiddleCenter, Color.black);
            txt.raycastTarget = false;
        }

        private static void DisableLegacyKeypadLock()
        {
            ExitDoorLock[] locks =
                Object.FindObjectsByType<ExitDoorLock>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            for (int i = 0; i < locks.Length; i++)
            {
                if (locks[i] != null)
                    locks[i].enabled = false;
            }
        }

        private static void ConvertRoom1()
        {
            Transform room =
                GameObject.Find(
                    "AR_Escape_Room_Structure"
                )?.transform;

            if (room == null)
                return;

            PaintRoom(
                room,
                new Color(0.82f, 0.55f, 0.42f),
                new Color(0.35f, 0.22f, 0.14f),
                new Color(0.2f, 0.35f, 0.55f)
            );

            EnsureRoomZone(
                room,
                RoomId.Room1,
                new Vector3(0f, 1.5f, 0f),
                new Vector3(8f, 3f, 8f)
            );

            if (room.Find("Desk") == null)
            {
                BuildLivingHallFurniture(room);
            }

            GameObject exit =
                GameObject.Find("ExitDoor_Keypad");

            if (exit != null)
            {
                DoorController dc =
                    exit.GetComponent<DoorController>();

                if (dc == null)
                    dc = exit.AddComponent<DoorController>();

                ProgressDoor pd =
                    exit.GetComponent<ProgressDoor>();

                if (pd == null)
                    pd = exit.AddComponent<ProgressDoor>();

                pd.Bind(
                    dc,
                    DoorRequirement.RequiresKey,
                    false,
                    2
                );

                Collider col =
                    exit.GetComponent<Collider>();

                if (col != null)
                    col.enabled = true;
            }
        }

        private static void BuildLivingHallFurniture(
            Transform room)
        {
            Color wood =
                new Color(0.28f, 0.16f, 0.08f);

            Color fabric =
                new Color(0.15f, 0.25f, 0.45f);

            Color cream =
                new Color(0.92f, 0.88f, 0.78f);

            GameObject sofa = Cube(
                "Sofa",
                room,
                new Vector3(-3.2f, 0.45f, 0.5f),
                new Vector3(2.4f, 0.9f, 1.1f),
                fabric
            );

            Cube(
                "SofaBack",
                sofa.transform,
                new Vector3(0f, 0.55f, 0.4f),
                new Vector3(1f, 0.9f, 0.25f),
                fabric
            );

            Cube(
                "SideTable",
                room,
                new Vector3(2.6f, 0.4f, -1.2f),
                new Vector3(0.7f, 0.8f, 0.7f),
                wood
            );

            Cube(
                "Pineapple",
                room,
                new Vector3(2.6f, 0.95f, -1.2f),
                new Vector3(0.25f, 0.35f, 0.25f),
                new Color(0.95f, 0.75f, 0.15f)
            );

            Cube(
                "Desk",
                room,
                new Vector3(3.2f, 0.7f, 1.6f),
                new Vector3(1.6f, 0.12f, 0.8f),
                wood
            );

            Cube(
                "DeskLegL",
                room,
                new Vector3(2.6f, 0.35f, 1.6f),
                new Vector3(0.12f, 0.7f, 0.6f),
                wood
            );

            Cube(
                "DeskLegR",
                room,
                new Vector3(3.8f, 0.35f, 1.6f),
                new Vector3(0.12f, 0.7f, 0.6f),
                wood
            );

            GameObject drawerEmpty = Cube(
                "Drawer_Empty",
                room,
                new Vector3(2.85f, 0.45f, 1.75f),
                new Vector3(0.55f, 0.18f, 0.5f),
                cream
            );

            // Legacy drawers are static decorative assets only - the original key has been relocated
            // to the Stylized_Key inside Desk Table White's desk_drawer.
            GameObject drawerKey = Cube(
                "Drawer_Key",
                room,
                new Vector3(3.5f, 0.45f, 1.75f),
                new Vector3(0.55f, 0.18f, 0.5f),
                cream
            );

            MakePainting(
                room,
                "Painting_Turtle",
                new Vector3(0f, 2.2f, 4.35f),
                new Vector3(1.4f, 1.1f, 0.08f),
                new Color(0.1f, 0.45f, 0.35f),
                "Sea Turtle",
                "A turtle rests near the shore."
            );

            MakePainting(
                room,
                "Sign_Seashell",
                new Vector3(0f, 3.3f, 4.35f),
                new Vector3(2.2f, 0.4f, 0.08f),
                new Color(0.15f, 0.7f, 0.25f),
                "SEASHELL BEND",
                "Welcome to the Living Hall."
            );

            MakePainting(
                room,
                "Painting_Beach",
                new Vector3(-4.45f, 2.2f, -1.5f),
                new Vector3(0.08f, 1.2f, 1.6f),
                new Color(0.2f, 0.4f, 0.75f),
                "Beach",
                "The first secret is in the desk."
            );
        }

        private static void BuildRooms2To6()
        {
            Transform room1 =
                GameObject.Find(
                    "AR_Escape_Room_Structure"
                )?.transform;

            if (room1 == null)
                return;

            Transform root =
                new GameObject("EscapeRooms").transform;

            root.position = room1.position;

            Transform r2 = BuildRoomShell(
                "Room_2",
                root,
                new Vector3(0f, 0f, 10f),
                new Color(0.92f, 0.9f, 0.85f),
                new Color(0.18f, 0.12f, 0.1f),
                new Color(0.95f, 0.95f, 0.95f),
                true,
                true
            );

            Transform r3 = BuildRoomShell(
                "Room_3",
                root,
                new Vector3(0f, 0f, 20f),
                new Color(0.12f, 0.16f, 0.22f),
                new Color(0.05f, 0.06f, 0.08f),
                new Color(0.1f, 0.12f, 0.16f),
                true,
                true
            );

            Transform r4 = BuildRoomShell(
                "Room_4",
                root,
                new Vector3(0f, 0f, 30f),
                new Color(0.15f, 0.15f, 0.18f),
                new Color(0.08f, 0.08f, 0.1f),
                new Color(0.2f, 0.2f, 0.24f),
                true,
                true
            );

            Transform r5 = BuildRoomShell(
                "Room_5",
                root,
                new Vector3(0f, 0f, 40f),
                new Color(0.22f, 0.2f, 0.18f),
                new Color(0.12f, 0.1f, 0.08f),
                new Color(0.25f, 0.22f, 0.18f),
                true,
                true
            );

            Transform r6 = BuildRoomShell(
                "Room_6",
                root,
                new Vector3(0f, 0f, 50f),
                new Color(0.08f, 0.08f, 0.1f),
                new Color(0.04f, 0.04f, 0.05f),
                new Color(0.12f, 0.1f, 0.08f),
                true,
                true
            );

            EnsureRoomZone(
                r2,
                RoomId.Room2,
                new Vector3(0f, 1.5f, 0f),
                new Vector3(8f, 3f, 8f)
            );

            EnsureRoomZone(
                r3,
                RoomId.Room3,
                new Vector3(0f, 1.5f, 0f),
                new Vector3(8f, 3f, 8f)
            );

            EnsureRoomZone(
                r4,
                RoomId.Room4,
                new Vector3(0f, 1.5f, 0f),
                new Vector3(8f, 3f, 8f)
            );

            EnsureRoomZone(
                r5,
                RoomId.Room5,
                new Vector3(0f, 1.5f, 0f),
                new Vector3(8f, 3f, 8f)
            );

            EnsureRoomZone(
                r6,
                RoomId.Room6,
                new Vector3(0f, 1.5f, 0f),
                new Vector3(8f, 3f, 8f)
            );

            SetupRoom2(r2);
            SetupRoom3(r3);
            SetupRoom4(r4);
            SetupRoom5(r5);
            SetupRoom6(r6);
        }

        private static Transform BuildRoomShell(
            string name,
            Transform parent,
            Vector3 localPos,
            Color wall,
            Color floor,
            Color ceiling,
            bool frontHole,
            bool backHole)
        {
            GameObject room =
                new GameObject(name);

            room.transform.SetParent(
                parent,
                false
            );

            room.transform.localPosition =
                localPos;

            Cube(
                "Floor",
                room.transform,
                new Vector3(0f, 0.02f, 0f),
                new Vector3(10f, 0.08f, 10f),
                floor
            );

            Cube(
                "Ceiling",
                room.transform,
                new Vector3(0f, 5.2f, 0f),
                new Vector3(10f, 0.2f, 10f),
                ceiling
            );

            Cube(
                "Wall_Left",
                room.transform,
                new Vector3(-5f, 2.5f, 0f),
                new Vector3(0.3f, 5f, 10f),
                wall
            );

            Cube(
                "Wall_Right",
                room.transform,
                new Vector3(5f, 2.5f, 0f),
                new Vector3(0.3f, 5f, 10f),
                wall
            );

            if (frontHole)
            {
                Cube(
                    "Wall_Front_L",
                    room.transform,
                    new Vector3(-3.2f, 2.5f, -4.9f),
                    new Vector3(3.6f, 5f, 0.2f),
                    wall
                );

                Cube(
                    "Wall_Front_R",
                    room.transform,
                    new Vector3(3.2f, 2.5f, -4.9f),
                    new Vector3(3.6f, 5f, 0.2f),
                    wall
                );

                Cube(
                    "Wall_Front_T",
                    room.transform,
                    new Vector3(0f, 4.2f, -4.9f),
                    new Vector3(2.8f, 1.6f, 0.2f),
                    wall
                );
            }
            else
            {
                Cube(
                    "Wall_Front",
                    room.transform,
                    new Vector3(0f, 2.5f, -4.9f),
                    new Vector3(10f, 5f, 0.2f),
                    wall
                );
            }

            if (backHole)
            {
                Cube(
                    "Wall_Back_L",
                    room.transform,
                    new Vector3(-3.2f, 2.5f, 4.9f),
                    new Vector3(3.6f, 5f, 0.2f),
                    wall
                );

                Cube(
                    "Wall_Back_R",
                    room.transform,
                    new Vector3(3.2f, 2.5f, 4.9f),
                    new Vector3(3.6f, 5f, 0.2f),
                    wall
                );

                Cube(
                    "Wall_Back_T",
                    room.transform,
                    new Vector3(0f, 4.2f, 4.9f),
                    new Vector3(2.8f, 1.6f, 0.2f),
                    wall
                );
            }
            else
            {
                Cube(
                    "Wall_Back",
                    room.transform,
                    new Vector3(0f, 2.5f, 4.9f),
                    new Vector3(10f, 5f, 0.2f),
                    wall
                );
            }

            return room.transform;
        }

        private static void SetupRoom2(Transform room)
        {
            string[] letters =
                { "L", "U", "S", "T" };

            string[] digits =
                { "4", "7", "2", "9" };

            for (int i = 0; i < 4; i++)
            {
                float x =
                    -2.4f + i * 1.6f;

                MakePainting(
                    room,
                    $"Cipher_{letters[i]}",
                    new Vector3(x, 2.4f, -4.7f),
                    new Vector3(1.2f, 1.2f, 0.08f),
                    Color.black,
                    letters[i],
                    $"The {letters[i]} tile hides the digit {digits[i]}."
                );

                CreateWorldLabel(
                    room,
                    $"Digit_{digits[i]}",
                    digits[i],
                    new Vector3(x, 2.4f, -4.64f),
                    0.35f,
                    Color.white
                );
            }

            MakePainting(
                room,
                "Cipher_Heart",
                new Vector3(4.45f, 2.4f, 0f),
                new Vector3(0.08f, 1.1f, 1.1f),
                Color.red,
                "Heart",
                "Follow the four tiles in order."
            );

            MakePainting(
                room,
                "Cipher_Grid",
                new Vector3(0f, 2.6f, 4.35f),
                new Vector3(2.4f, 1.4f, 0.08f),
                new Color(0.15f, 0.15f, 0.15f),
                "Grid",
                "Code: 4729"
            );

            Cube(
                "Rug",
                room,
                new Vector3(0f, 0.06f, 0f),
                new Vector3(4f, 0.04f, 3f),
                new Color(0.7f, 0.12f, 0.12f)
            );

            Cube(
                "Chandelier",
                room,
                new Vector3(0f, 4.6f, 0f),
                new Vector3(1.2f, 0.15f, 1.2f),
                new Color(0.85f, 0.7f, 0.2f)
            );

            ProgressionDoor door =
                MakeExitDoor(
                    room,
                    "Door_R2_R3",
                    DoorRequirement.RequiresPassword,
                    false,
                    3
                );

            CipherPuzzle cipher =
                room.gameObject.AddComponent<CipherPuzzle>();

            cipher.Bind(
                Object.FindFirstObjectByType<KeypadUI>(
                    FindObjectsInactive.Include
                ),
                door
            );

            door.BindCipher(cipher);
        }

        private static void SetupRoom3(Transform room)
        {
            Vector3[] spots =
            {
                new Vector3(-4.5f, 2.2f, -1.5f),
                new Vector3(4.5f, 1.6f, 1.2f),
                new Vector3(-3.2f, 3.4f, 4.4f),
                new Vector3(0f, 1.4f, 3.8f),
                new Vector3(3.4f, 2.8f, -4.4f)
            };

            Color gemColor =
                new Color(0.2f, 0.9f, 1f);

            for (int i = 0; i < spots.Length; i++)
            {
                GameObject gem =
                    new GameObject($"Blue_Diamond_{i + 1}");

                gem.transform.SetParent(
                    room,
                    false
                );

                gem.transform.localPosition =
                    spots[i];

                gem.transform.localScale =
                    Vector3.one * 0.35f;

                MeshFilter mf = gem.AddComponent<MeshFilter>();
                MeshRenderer mr = gem.AddComponent<MeshRenderer>();
                SphereCollider sc = gem.AddComponent<SphereCollider>();
                sc.radius = 0.7f;
                sc.center = Vector3.zero;

                gem.AddComponent<GemCollectible>();
            }

            MakePainting(
                room,
                "Painting_Cover",
                new Vector3(0.4f, 1.4f, 3.95f),
                new Vector3(0.9f, 1.1f, 0.06f),
                new Color(0.4f, 0.25f, 0.15f),
                "Canvas",
                "Something glows behind this canvas."
            );

            MakeExitDoor(
                room,
                "Door_R3_R4",
                DoorRequirement.RequiresGems,
                true,
                4
            );
        }

        private static void SetupRoom4(Transform room)
        {
            ColorSequencePuzzle puzzle =
                room.gameObject.AddComponent<ColorSequencePuzzle>();

            puzzle.Configure(
                new[] { "Red", "Green", "Yellow", "Blue" }
            );

            CreateColorButton(
                room,
                puzzle,
                "Red",
                Color.red,
                new Vector3(-1.8f, 1.8f, 4.6f)
            );

            CreateColorButton(
                room,
                puzzle,
                "Green",
                Color.green,
                new Vector3(-0.6f, 1.8f, 4.6f)
            );

            CreateColorButton(
                room,
                puzzle,
                "Yellow",
                Color.yellow,
                new Vector3(0.6f, 1.8f, 4.6f)
            );

            CreateColorButton(
                room,
                puzzle,
                "Blue",
                Color.blue,
                new Vector3(1.8f, 1.8f, 4.6f)
            );

            CreateWorldLabel(
                room,
                "SeqHint",
                "R  G  Y  B",
                new Vector3(0f, 3.1f, 4.6f),
                0.18f,
                Color.white
            );

            MakeExitDoor(
                room,
                "Door_R4_R5",
                DoorRequirement.RequiresColorSequence,
                true,
                5
            );
        }

        private static void SetupRoom5(Transform room)
        {
            ShapePlacementPuzzle puzzle = room.GetComponent<ShapePlacementPuzzle>();
            if (puzzle == null)
            {
                puzzle = room.gameObject.AddComponent<ShapePlacementPuzzle>();
            }

            // Check if user-placed custom shapes already exist in Room 5 or in scene
            bool hasCustomShapes = room.Find("Traingle_shape") != null || room.Find("Triangle_Shape") != null || room.Find("Shape_Triangle") != null ||
                                   room.Find("Star_Shape") != null || room.Find("Shape_Star") != null ||
                                   room.Find("Square_Shape") != null || room.Find("Shape_Square") != null ||
                                   room.Find("Circle_Shape") != null || room.Find("Shape_Circle") != null;

            // Only spawn primitive shapes if custom shapes are completely missing
            if (!hasCustomShapes)
            {
                CreateShape(
                    room,
                    ShapeType.Triangle,
                    new Vector3(-2.4f, 0.45f, -1.5f),
                    new Color(0.9f, 0.4f, 0.2f)
                );

                CreateShape(
                    room,
                    ShapeType.Star,
                    new Vector3(-0.8f, 0.35f, -2.2f),
                    new Color(0.95f, 0.85f, 0.2f)
                );

                CreateShape(
                    room,
                    ShapeType.Square,
                    new Vector3(0.8f, 0.4f, -1.6f),
                    new Color(0.3f, 0.7f, 0.4f)
                );

                CreateShape(
                    room,
                    ShapeType.Circle,
                    new Vector3(2.4f, 0.4f, -2.0f),
                    new Color(0.4f, 0.55f, 0.95f)
                );
            }

            if (room.Find("Socket_Triangle") == null && room.Find("Socket_Traingle") == null)
            {
                CreateSocket(
                    room,
                    puzzle,
                    ShapeType.Triangle,
                    new Vector3(-2.4f, 1.8f, 4.7f),
                    new Color(0.9f, 0.4f, 0.2f)
                );
            }

            if (room.Find("Socket_Star") == null)
            {
                CreateSocket(
                    room,
                    puzzle,
                    ShapeType.Star,
                    new Vector3(-0.8f, 1.8f, 4.7f),
                    new Color(0.95f, 0.85f, 0.2f)
                );
            }

            if (room.Find("Socket_Square") == null)
            {
                CreateSocket(
                    room,
                    puzzle,
                    ShapeType.Square,
                    new Vector3(0.8f, 1.8f, 4.7f),
                    new Color(0.3f, 0.7f, 0.4f)
                );
            }

            if (room.Find("Socket_Circle") == null)
            {
                CreateSocket(
                    room,
                    puzzle,
                    ShapeType.Circle,
                    new Vector3(2.4f, 1.8f, 4.7f),
                    new Color(0.4f, 0.55f, 0.95f)
                );
            }

            if (room.Find("Door_R5_R6") == null)
            {
                MakeExitDoor(
                    room,
                    "Door_R5_R6",
                    DoorRequirement.RequiresShapes,
                    true,
                    6
                );
            }
        }

        private static void SetupRoom6(Transform room)
        {
            GameObject pedestal = Cube(
                "Pedestal",
                room,
                new Vector3(0f, 0.6f, 0.8f),
                new Vector3(1.2f, 1.2f, 1.2f),
                new Color(0.25f, 0.22f, 0.18f)
            );

            Cube(
                "PedestalTop",
                pedestal.transform,
                new Vector3(0f, 0.6f, 0f),
                new Vector3(1.15f, 0.12f, 1.15f),
                new Color(0.75f, 0.65f, 0.25f)
            );

            Cube(
                "KeySlot",
                pedestal.transform,
                new Vector3(0f, 0.7f, 0f),
                new Vector3(0.25f, 0.08f, 0.5f),
                new Color(0.9f, 0.8f, 0.2f)
            );

            for (int i = 0; i < 5; i++)
            {
                Cube(
                    $"GemSlot_{i}",
                    pedestal.transform,
                    new Vector3(
                        -0.4f + i * 0.2f,
                        0.55f,
                        0.4f
                    ),
                    new Vector3(0.12f, 0.12f, 0.12f),
                    new Color(0.2f, 0.8f, 1f)
                );
            }

            ProgressionDoor finalDoor =
                MakeExitDoor(
                    room,
                    "Door_Final",
                    DoorRequirement.RequiresFinalValidation,
                    false,
                    6
                );

            FinalVaultController vault =
                pedestal.AddComponent<FinalVaultController>();

            vault.Bind(finalDoor);

            CreateWorldLabel(
                room,
                "VaultLabel",
                "FINAL VAULT",
                new Vector3(0f, 2.2f, 0.8f),
                0.16f,
                Color.yellow
            );
        }

        private static ProgressionDoor MakeExitDoor(
            Transform room,
            string name,
            DoorRequirement req,
            bool autoOpen,
            int nextRoom)
        {
            GameObject door = Cube(
                name,
                room,
                new Vector3(0f, 1.5f, 4.85f),
                new Vector3(2f, 3f, 0.18f),
                new Color(0.3f, 0.55f, 0.75f, 0.85f)
            );

            DoorController dc =
                door.AddComponent<DoorController>();

            dc.ConfigureSliding(
                door.transform.localPosition,
                door.transform.localPosition +
                new Vector3(-2.6f, 0f, 0f),
                true,
                name
            );

            ProgressionDoor pd =
                door.AddComponent<ProgressionDoor>();

            pd.Bind(
                dc,
                req,
                autoOpen,
                nextRoom
            );

            // If a realistic replacement door is present in the scene, deactivate the old procedural door
            if (name == "Door_R2_R3" && (GameObject.Find("Door_10_White") != null || GameObject.Find("Door_10_white") != null))
            {
                door.SetActive(false);
            }
            else if (name == "Door_R3_R4" && (GameObject.Find("Door_13_Black") != null || GameObject.Find("Door_13_black") != null))
            {
                door.SetActive(false);
            }
            else if (name == "Door_R4_R5" && (GameObject.Find("Door_10_Black") != null || GameObject.Find("Door_10_black") != null))
            {
                door.SetActive(false);
            }
            else if (name == "Door_R5_R6" && (GameObject.Find("UniqueDoor") != null || GameObject.Find("unique door") != null))
            {
                door.SetActive(false);
            }

            return pd;
        }

        private static void CreateColorButton(
            Transform room,
            ColorSequencePuzzle puzzle,
            string colorName,
            Color color,
            Vector3 localPos)
        {
            GameObject btn = Cube(
                $"Btn_{colorName}",
                room,
                localPos,
                new Vector3(0.7f, 0.7f, 0.18f),
                color
            );

            ColorButton cb =
                btn.AddComponent<ColorButton>();

            cb.Bind(
                colorName,
                puzzle,
                color
            );
        }

        private static void CreateShape(
            Transform room,
            ShapeType type,
            Vector3 pos,
            Color color)
        {
            GameObject obj;

            if (type == ShapeType.Circle)
            {
                obj =
                    GameObject.CreatePrimitive(
                        PrimitiveType.Sphere
                    );
            }
            else if (type == ShapeType.Triangle)
            {
                obj =
                    GameObject.CreatePrimitive(
                        PrimitiveType.Capsule
                    );

                obj.transform.localScale =
                    new Vector3(
                        0.55f,
                        0.55f,
                        0.55f
                    );
            }
            else if (type == ShapeType.Star)
            {
                obj = Cube(
                    "StarCore",
                    room,
                    pos,
                    new Vector3(
                        0.7f,
                        0.18f,
                        0.18f
                    ),
                    color
                );

                Cube(
                    "StarCross",
                    obj.transform,
                    Vector3.zero,
                    new Vector3(
                        0.25f,
                        1f,
                        1f
                    ),
                    color
                );

                obj.name = "Shape_Star";

                ShapeObject star =
                    obj.AddComponent<ShapeObject>();

                star.Bind(type);

                Rigidbody rbStar =
                    obj.GetComponent<Rigidbody>();

                if (rbStar == null)
                    rbStar =
                        obj.AddComponent<Rigidbody>();

                rbStar.isKinematic = true;

                return;
            }
            else
            {
                obj =
                    GameObject.CreatePrimitive(
                        PrimitiveType.Cube
                    );
            }

            obj.name =
                $"Shape_{type}";

            obj.transform.SetParent(
                room,
                false
            );

            obj.transform.localPosition =
                pos;

            if (type == ShapeType.Square)
            {
                obj.transform.localScale =
                    Vector3.one * 0.55f;
            }

            ApplyMat(
                obj,
                color,
                false
            );

            ShapeObject shape =
                obj.AddComponent<ShapeObject>();

            shape.Bind(type);

            Rigidbody rb =
                obj.GetComponent<Rigidbody>();

            if (rb == null)
                rb =
                    obj.AddComponent<Rigidbody>();

            rb.isKinematic = true;
        }

        private static void CreateSocket(
            Transform room,
            ShapePlacementPuzzle puzzle,
            ShapeType type,
            Vector3 pos,
            Color color)
        {
            GameObject socket = Cube(
                $"Socket_{type}",
                room,
                pos,
                new Vector3(
                    0.85f,
                    0.85f,
                    0.12f
                ),
                new Color(
                    color.r,
                    color.g,
                    color.b,
                    0.35f
                )
            );

            ShapeSocket ss =
                socket.AddComponent<ShapeSocket>();

            ss.Bind(
                type,
                puzzle
            );

            CreateWorldLabel(
                room,
                $"SocketLabel_{type}",
                type.ToString(),
                pos +
                new Vector3(
                    0f,
                    0.7f,
                    0f
                ),
                0.12f,
                color
            );
        }

        private static void MakePainting(
            Transform room,
            string name,
            Vector3 pos,
            Vector3 scale,
            Color color,
            string title,
            string clue)
        {
            GameObject painting =
                Cube(
                    name,
                    room,
                    pos,
                    scale,
                    color
                );

            CluePainting clueComp =
                painting.AddComponent<CluePainting>();

            clueComp.Bind(
                title,
                clue
            );
        }

        // =========================================================
        // WORLD TEXT
        // =========================================================

        private static void CreateWorldLabel(
            Transform parent,
            string name,
            string text,
            Vector3 localPos,
            float charSize,
            Color color)
        {
            GameObject go =
                new GameObject(name);

            go.transform.SetParent(
                parent,
                false
            );

            go.transform.localPosition =
                localPos;

            TextMesh tm =
                go.AddComponent<TextMesh>();

            tm.text = text;
            tm.fontSize = 64;
            tm.characterSize = charSize;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = color;

            Font font = UiFactory.Font;

            if (font != null)
                tm.font = font;

            EscapeRoomWorldText textController =
                go.AddComponent<EscapeRoomWorldText>();

            textController.SetRoom(
                GetRoomIdFromHierarchy(parent)
            );
        }

        private static void SetupExistingWorldText()
        {
            TextMesh[] texts =
                Object.FindObjectsByType<TextMesh>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            int fixedCount = 0;

            for (int i = 0; i < texts.Length; i++)
            {
                TextMesh tm = texts[i];

                if (tm == null)
                    continue;

                EscapeRoomWorldText controller =
                    tm.GetComponent<EscapeRoomWorldText>();

                if (controller == null)
                {
                    controller =
                        tm.gameObject.AddComponent<EscapeRoomWorldText>();
                }

                RoomId roomId =
                    GetRoomIdFromHierarchy(
                        tm.transform
                    );

                controller.SetRoom(roomId);

                fixedCount++;
            }

            Debug.Log(
                $"[EscapeWorldBuilder] Fixed {fixedCount} world text objects."
            );
        }

        private static RoomId GetRoomIdFromHierarchy(
            Transform current)
        {
            if (current == null)
                return RoomId.Mall;

            while (current != null)
            {
                if (current.name ==
                    "AR_Escape_Room_Structure")
                {
                    return RoomId.Room1;
                }

                if (current.name == "Room_2")
                    return RoomId.Room2;

                if (current.name == "Room_3")
                    return RoomId.Room3;

                if (current.name == "Room_4")
                    return RoomId.Room4;

                if (current.name == "Room_5")
                    return RoomId.Room5;

                if (current.name == "Room_6")
                    return RoomId.Room6;

                current =
                    current.parent;
            }

            return RoomId.Mall;
        }

        private static void EnsureRoomZone(
            Transform room,
            RoomId id,
            Vector3 localPos,
            Vector3 size)
        {
            Transform existing =
                room.Find("RoomZone");

            GameObject zone =
                existing != null
                    ? existing.gameObject
                    : new GameObject("RoomZone");

            zone.transform.SetParent(
                room,
                false
            );

            zone.transform.localPosition =
                localPos;

            BoxCollider box =
                zone.GetComponent<BoxCollider>();

            if (box == null)
            {
                box =
                    zone.AddComponent<BoxCollider>();
            }

            box.isTrigger = true;
            box.size = size;

            RoomZone rz =
                zone.GetComponent<RoomZone>();

            if (rz == null)
            {
                rz =
                    zone.AddComponent<RoomZone>();
            }

            rz.Bind(id);
        }

        private static void PaintRoom(
            Transform room,
            Color wall,
            Color floor,
            Color ceiling)
        {
            SetIfExists(
                room,
                "Wall_Front_Left",
                wall
            );

            SetIfExists(
                room,
                "Wall_Front_Right",
                wall
            );

            SetIfExists(
                room,
                "Wall_Front_Top",
                wall
            );

            SetIfExists(
                room,
                "Wall_Left",
                wall
            );

            SetIfExists(
                room,
                "Wall_Right",
                wall
            );

            SetIfExists(
                room,
                "Wall_Back",
                wall
            );

            SetIfExists(
                room,
                "Wall_Back_Left",
                wall
            );

            SetIfExists(
                room,
                "Wall_Back_Right",
                wall
            );

            SetIfExists(
                room,
                "Wall_Back_Top",
                wall
            );

            SetIfExists(
                room,
                "Roof",
                ceiling
            );

            SetIfExists(
                room,
                "Floor",
                floor
            );
        }

        private static void SetIfExists(
            Transform room,
            string name,
            Color color)
        {
            Transform t =
                room.Find(name);

            if (t != null)
            {
                ApplyMat(
                    t.gameObject,
                    color,
                    false
                );
            }
        }

        private static void ClearMallBlockingCorridor()
        {
            GameObject mall =
                GameObject.Find("Mall_Environment");

            Transform room1 =
                GameObject.Find(
                    "AR_Escape_Room_Structure"
                )?.transform;

            if (mall == null || room1 == null)
                return;

            Bounds corridor =
                new Bounds(
                    room1.position +
                    new Vector3(
                        0f,
                        2.5f,
                        25f
                    ),
                    new Vector3(
                        12f,
                        6f,
                        62f
                    )
                );

            Collider[] colliders =
                mall.GetComponentsInChildren<Collider>(
                    true
                );

            int disabled = 0;

            for (int i = 0;
                 i < colliders.Length;
                 i++)
            {
                Collider col =
                    colliders[i];

                if (col == null ||
                    col.isTrigger)
                {
                    continue;
                }

                if (col.bounds.Intersects(corridor))
                {
                    col.enabled = false;
                    disabled++;
                }
            }

            Debug.Log(
                $"[EscapeWorldBuilder] Disabled {disabled} mall colliders blocking the room corridor."
            );
        }

        private static GameObject Cube(
            string name,
            Transform parent,
            Vector3 localPos,
            Vector3 scale,
            Color color)
        {
            GameObject go =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube
                );

            go.name = name;

            go.transform.SetParent(
                parent,
                false
            );

            go.transform.localPosition =
                localPos;

            go.transform.localScale =
                scale;

            ApplyMat(
                go,
                color,
                false
            );

            return go;
        }

        private static void ApplyMat(
            GameObject go,
            Color color,
            bool emit)
        {
            Renderer r =
                go.GetComponent<Renderer>();

            if (r == null)
                return;

            Shader shader =
                Shader.Find("Standard");

            if (shader == null)
            {
                shader =
                    Shader.Find(
                        "Universal Render Pipeline/Lit"
                    );
            }

            Material mat =
                shader != null
                    ? new Material(shader)
                    : r.material;

            mat.color = color;

            if (emit)
            {
                mat.EnableKeyword("_EMISSION");

                mat.SetColor(
                    "_EmissionColor",
                    color * 2f
                );
            }

            r.sharedMaterial = mat;
        }
    }


    // =============================================================
    // ROOM-SPECIFIC WORLD TEXT CONTROLLER
    // =============================================================

    public class EscapeRoomWorldText : MonoBehaviour
    {
        private RoomId assignedRoom =
            RoomId.Mall;

        private TextMesh textMesh;

        private Renderer textRenderer;

        public void SetRoom(RoomId room)
        {
            assignedRoom = room;

            Refresh();
        }

        private void Awake()
        {
            textMesh =
                GetComponent<TextMesh>();

            textRenderer =
                GetComponent<Renderer>();
        }

        private void OnEnable()
        {
            GameManager.OnStateChanged += Refresh;
        }

        private void OnDisable()
        {
            GameManager.OnStateChanged -= Refresh;
        }

        private void Start()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (textRenderer == null)
            {
                textRenderer =
                    GetComponent<Renderer>();
            }

            if (textRenderer == null)
                return;

            GameManager gm =
                GameManager.Instance;

            if (gm == null)
                return;

            bool shouldShow =
                gm.CurrentRoom ==
                assignedRoom;

            textRenderer.enabled =
                shouldShow;
        }
    }
}