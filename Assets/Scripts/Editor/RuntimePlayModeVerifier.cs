using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using AREscapeRoom.Core;
using AREscapeRoom.Player;
using AREscapeRoom.Puzzles;
using AREscapeRoom.UI;

namespace AREscapeRoom.Editor
{
    [InitializeOnLoad]
    public static class RuntimePlayModeVerifier
    {
        private const string REPORT_PATH = "scratch/play_mode_verification_results.txt";
        private const string FLAG_PATH = "scratch/run_verifier.flag";

        private static bool isTestActive = false;
        private static int testFrame = 0;
        private static Vector3 editPlayerPos;
        private static Vector3 editPlayerEuler;
        private static Vector3 editCamWorldPos;
        private static Vector3 editCamLocalPos;
        private static Vector3 editCamEuler;
        private static StringBuilder report = new StringBuilder();

        static RuntimePlayModeVerifier()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.delayCall += CheckFlagTrigger;
        }

        private static void CheckFlagTrigger()
        {
            if (File.Exists(FLAG_PATH) && !EditorApplication.isPlaying)
            {
                try { File.Delete(FLAG_PATH); } catch { }
                StartVerification();
            }
        }

        [MenuItem("EscapeRoom/Run Full Play Mode Verification")]
        public static void StartVerification()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Verifier] Already in play mode. Stop play mode first.");
                return;
            }

            report.Clear();
            report.AppendLine("=================================================");
            report.AppendLine("UNITY AR ESCAPE ROOM - RUNTIME VERIFICATION REPORT");
            report.AppendLine("=================================================");
            report.AppendLine($"Timestamp: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            var scene = EditorSceneManager.GetActiveScene();
            report.AppendLine($"Active Scene: {scene.name} ({scene.path})");

            // 1. Inspect Edit Mode Player and Camera
            GameObject playerObj = GameObject.Find("Player");
            GameObject camObj = GameObject.Find("Main Camera");

            if (playerObj == null)
            {
                report.AppendLine("ERROR: Player GameObject not found in edit mode!");
                SaveReport();
                return;
            }

            editPlayerPos = playerObj.transform.position;
            editPlayerEuler = playerObj.transform.eulerAngles;

            report.AppendLine("\n--- 1. EDIT MODE TRANSFORMS ---");
            report.AppendLine($"Player Position: {editPlayerPos}");
            report.AppendLine($"Player Euler: {editPlayerEuler}");

            if (camObj != null)
            {
                editCamWorldPos = camObj.transform.position;
                editCamLocalPos = camObj.transform.localPosition;
                editCamEuler = camObj.transform.eulerAngles;
                report.AppendLine($"Main Camera World Pos: {editCamWorldPos}");
                report.AppendLine($"Main Camera Local Pos: {editCamLocalPos}");
                report.AppendLine($"Main Camera Euler: {editCamEuler}");
                report.AppendLine($"Main Camera Parent: {(camObj.transform.parent != null ? camObj.transform.parent.name : "None")}");
            }
            else
            {
                report.AppendLine("ERROR: Main Camera not found in edit mode!");
            }

            // Inspect scene environment objects
            report.AppendLine("\n--- 2. EDIT MODE SCENE OBJECTS ---");
            CheckObject("SolidGround");
            CheckObject("AR_Escape_Room_Structure");
            CheckObject("Door_5_Brown");
            CheckObject("Ice_Scream_Shopping_Center_Map");
            CheckObject("EscapeRooms");
            CheckObject("Room_2");
            CheckObject("Room_3");
            CheckObject("Room_4");
            CheckObject("Room_5");
            CheckObject("Room_6");

            isTestActive = true;
            testFrame = 0;
            report.AppendLine("\nStarting Play Mode for runtime verification...");
            EditorApplication.isPlaying = true;
        }

        private static void CheckObject(string name)
        {
            GameObject go = GameObject.Find(name);
            if (go != null)
            {
                report.AppendLine($"[PRESENT] {name:32} pos={go.transform.position} active={go.activeInHierarchy}");
            }
            else
            {
                report.AppendLine($"[MISSING] {name:32}");
            }
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (!isTestActive) return;

            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                testFrame = 0;
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                isTestActive = false;
                SaveReport();
            }
        }

        private static void OnEditorUpdate()
        {
            if (File.Exists(FLAG_PATH) && !EditorApplication.isPlaying && !isTestActive)
            {
                try { File.Delete(FLAG_PATH); } catch { }
                StartVerification();
                return;
            }

            if (!isTestActive || !EditorApplication.isPlaying) return;

            testFrame++;

            // Run verification once start methods have executed (frame 5)
            if (testFrame == 5)
            {
                ExecutePlayModeChecks();
                // End play mode after checks complete
                EditorApplication.isPlaying = false;
            }
        }

        private static void ExecutePlayModeChecks()
        {
            report.AppendLine("\n--- 3. RUNTIME PLAY MODE TRANSFORMS (Frame 5) ---");

            GameObject playerObj = GameObject.Find("Player");
            GameObject camObj = GameObject.Find("Main Camera");

            if (playerObj == null || camObj == null)
            {
                report.AppendLine("ERROR: Player or Main Camera missing at runtime!");
                return;
            }

            Vector3 playPlayerPos = playerObj.transform.position;
            Vector3 playPlayerEuler = playerObj.transform.eulerAngles;
            Vector3 playCamWorldPos = camObj.transform.position;
            Vector3 playCamLocalPos = camObj.transform.localPosition;
            Vector3 playCamEuler = camObj.transform.eulerAngles;

            report.AppendLine($"Runtime Player Position: {playPlayerPos}");
            report.AppendLine($"Runtime Player Euler: {playPlayerEuler}");
            report.AppendLine($"Runtime Main Camera World Pos: {playCamWorldPos}");
            report.AppendLine($"Runtime Main Camera Local Pos: {playCamLocalPos}");
            report.AppendLine($"Runtime Main Camera Euler: {playCamEuler}");

            // Verify position match
            float posDiff = Vector3.Distance(editPlayerPos, playPlayerPos);
            report.AppendLine($"Player Position Delta vs Edit Mode: {posDiff:F4}m ({(posDiff < 0.1f ? "PASSED - ZERO TELEPORT" : "WARNING - Position shifted")})");

            float camWorldDiff = Vector3.Distance(editCamWorldPos, playCamWorldPos);
            report.AppendLine($"Camera World Position Delta: {camWorldDiff:F4}m ({(camWorldDiff < 0.1f ? "PASSED - CAMERA PRESERVED" : "WARNING - Camera shifted")})");

            // Verify camera orientation
            report.AppendLine($"Camera Pitch (X rotation): {playCamEuler.x:F2} degrees");
            bool pitchSafe = Mathf.Abs(Mathf.DeltaAngle(0f, playCamEuler.x)) < 15f;
            report.AppendLine($"Camera Facing Horizon: {(pitchSafe ? "PASSED - Looking Forward, NOT into sky" : "FAILED - Looking at sky/floor")}");

            // 4. Verify HUD Prompt (Pick up Star check)
            report.AppendLine("\n--- 4. INTERACTION PROMPT VERIFICATION ---");
            string currentPrompt = HUDController.Instance != null ? HUDController.Instance.CurrentPrompt : "";
            report.AppendLine($"HUD Prompt at Start: '{(string.IsNullOrEmpty(currentPrompt) ? "<EMPTY>" : currentPrompt)}'");
            bool promptClean = string.IsNullOrEmpty(currentPrompt);
            report.AppendLine($"Prompt Clean at Start: {(promptClean ? "PASSED - Zero unwanted pickup prompts" : "FAILED - Unwanted prompt showing")}");

            // Check ShapeObject.CanInteract from outside Room 5
            ShapeObject[] shapes = Object.FindObjectsByType<ShapeObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            report.AppendLine($"Total ShapeObjects in Scene: {shapes.Length}");
            bool anyInteractableFromOutside = false;
            foreach (var s in shapes)
            {
                if (s.CanInteract)
                {
                    anyInteractableFromOutside = true;
                    report.AppendLine($"WARNING: Shape '{s.name}' CanInteract is TRUE from spawn!");
                }
            }
            report.AppendLine($"Room 5 Shapes Inaccessible from Spawn: {(!anyInteractableFromOutside ? "PASSED" : "FAILED")}");

            // 5. Test Simulated Movement Toward Door_5_Brown
            report.AppendLine("\n--- 5. MOVEMENT DIRECTION VERIFICATION ---");
            CharacterController cc = playerObj.GetComponent<CharacterController>();
            if (cc != null)
            {
                Vector3 beforeMove = playerObj.transform.position;
                Vector3 forwardStep = playerObj.transform.forward * 1.0f;
                cc.Move(forwardStep);
                Vector3 afterMove = playerObj.transform.position;

                report.AppendLine($"Player Position Before Move: {beforeMove}");
                report.AppendLine($"Player Position After 1m Forward Move: {afterMove}");

                GameObject door = GameObject.Find("Door_5_Brown");
                if (door != null)
                {
                    float distBefore = Vector3.Distance(beforeMove, door.transform.position);
                    float distAfter = Vector3.Distance(afterMove, door.transform.position);
                    report.AppendLine($"Distance to Door_5_Brown Before: {distBefore:F3}m");
                    report.AppendLine($"Distance to Door_5_Brown After: {distAfter:F3}m");
                    bool movedCloser = distAfter < distBefore;
                    report.AppendLine($"Walking Forward Advances Toward Door_5_Brown: {(movedCloser ? "PASSED" : "FAILED")}");
                }
            }
            else
            {
                report.AppendLine("ERROR: CharacterController missing on Player!");
            }

            report.AppendLine("\n=================================================");
            report.AppendLine("VERIFICATION COMPLETE - ALL CHECKS EVALUATED");
            report.AppendLine("=================================================");
        }

        private static void SaveReport()
        {
            try
            {
                string text = report.ToString();
                File.WriteAllText(REPORT_PATH, text, Encoding.UTF8);
                // Also save to both workspaces
                File.WriteAllText("D:/documents/AREscapeRoom/arvr-2026-10-29-17-59-54-2026-10-29-17-59-54/" + REPORT_PATH, text, Encoding.UTF8);
                Debug.Log($"[RuntimePlayModeVerifier] Verification report written to {REPORT_PATH}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[RuntimePlayModeVerifier] Failed to write report: {ex.Message}");
            }
        }
    }
}
