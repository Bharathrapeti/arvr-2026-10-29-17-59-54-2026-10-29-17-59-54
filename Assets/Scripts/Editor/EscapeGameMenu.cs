using UnityEditor;
using UnityEngine;
using AREscapeRoom.Core;

public class EscapeGameMenu
{
    [MenuItem("AR Escape Room/2. Install 6-Room Game Into Current Scene")]
    public static void InstallIntoCurrentScene()
    {
        if (Application.isPlaying)
        {
            EscapeWorldBuilder.Install();
            return;
        }

        EscapeWorldBuilder.Install();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("Installed 6-room game into the open scene. Save the scene (Ctrl+S).");
    }
}
