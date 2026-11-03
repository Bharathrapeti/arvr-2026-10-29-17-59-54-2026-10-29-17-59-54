using UnityEngine;

namespace AREscapeRoom.Core
{
    public static class PerformanceOptimizer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OptimizeGlobalSettings()
        {
            // 1. Disable VSync locks to eliminate 30/20fps frame-drop stuttering
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;

            // 2. Limit real-time shadow distance to escape room corridor (50m instead of 400m)
            QualitySettings.shadowDistance = 50f;
            QualitySettings.shadowCascades = 2;
            QualitySettings.shadowResolution = ShadowResolution.Medium;

            // 3. Prevent realtime GI from starving CPU threads
            QualitySettings.realtimeGICPUUsage = 25;

            // 4. Smooth async asset/texture streaming
            QualitySettings.asyncUploadTimeSlice = 4;
            QualitySettings.asyncUploadBufferSize = 64;

            Debug.Log("[PerformanceOptimizer] Applied engine optimizations (vSync=0, targetFPS=60, shadowDistance=50m, shadowCascades=2).");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OptimizeSceneCamerasAndLights()
        {
            // 1. Cap camera far clip plane to 120m to cull distant outside mall geometry
            Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < cameras.Length; i++)
            {
                Camera cam = cameras[i];
                if (cam != null && cam.farClipPlane > 150f)
                {
                    cam.farClipPlane = 120f;
                    cam.useOcclusionCulling = true;
                }
            }

            // 2. Disable expensive real-time cubemap shadows on point lights
            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < lights.Length; i++)
            {
                Light lt = lights[i];
                if (lt != null && lt.type == LightType.Point && lt.shadows != LightShadows.None)
                {
                    lt.shadows = LightShadows.None;
                }
            }

            Debug.Log("[PerformanceOptimizer] Applied scene optimizations (camera farClip=120m, point light cubemap shadows disabled).");
        }
    }
}
