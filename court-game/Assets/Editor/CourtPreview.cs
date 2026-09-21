using System.IO;
using UnityEditor;
using UnityEngine;

namespace EduAI.Court.Editor
{
    public static class CourtPreview
    {
        [MenuItem("EduAI/Capture Preview")]
        public static void Capture()
        {
            CourtProjectBuilder.ValidateScene();
            var camera = Camera.main;
            var hud = UnityEngine.Object.FindFirstObjectByType<InteractionUI>().GetComponent<Canvas>();
            var renderTexture = RenderTexture.GetTemporary(1280, 720, 24);
            var oldActive = RenderTexture.active;
            var oldTarget = camera.targetTexture;
            var oldMode = hud.renderMode;
            var oldCamera = hud.worldCamera;
            float oldDistance = hud.planeDistance;
            var pixels = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            try
            {
                hud.renderMode = RenderMode.ScreenSpaceCamera;
                hud.worldCamera = camera; hud.planeDistance = .5f;
                camera.targetTexture = renderTexture;
                Canvas.ForceUpdateCanvases();
                camera.Render();
                RenderTexture.active = renderTexture;
                pixels.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
                pixels.Apply();
                Directory.CreateDirectory("Logs");
                File.WriteAllBytes("Logs/court-preview.png", pixels.EncodeToPNG());
                Debug.Log("COURT_PREVIEW_CAPTURED");
            }
            finally
            {
                hud.renderMode = oldMode; hud.worldCamera = oldCamera; hud.planeDistance = oldDistance;
                camera.targetTexture = oldTarget; RenderTexture.active = oldActive;
                RenderTexture.ReleaseTemporary(renderTexture);
                UnityEngine.Object.DestroyImmediate(pixels);
            }
        }
    }
}
