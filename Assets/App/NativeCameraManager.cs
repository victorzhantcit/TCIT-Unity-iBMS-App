using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace iBMSApp.App
{
    public static class NativeCameraManager
    {
        public static void TakePictureBase64(RawImage rawImage, Action<string> callback)
        {
            TakePicture(texture =>
            {
                if (rawImage != null) 
                    SaveRawImageTexture(rawImage, texture);
                callback?.Invoke(TextureToBase64(texture));
            });
        }

        public static void TakePictureTexture(RawImage rawImage, Action<Texture2D> callback)
        {
            TakePicture(texture =>
            {
                if (rawImage != null)
                    SaveRawImageTexture(rawImage, texture);
                callback?.Invoke(texture);
            });
        }

        private static void SaveRawImageTexture(RawImage rawImage, Texture2D texture)
        {
            rawImage.texture = texture;

            // 設定正確的比例
            if (rawImage.TryGetComponent<AspectRatioFitter>(out var aspectFitter))
            {
                float aspectRatio = (float)texture.width / texture.height;
                aspectFitter.aspectRatio = aspectRatio;
            }
        }

        public static string TextureToBase64(Texture2D texture)
        {
            if (!texture.isReadable)
                texture = MakeReadable(texture);

            byte[] imageBytes = texture.EncodeToPNG();
            return Convert.ToBase64String(imageBytes);
        }

        public static Texture2D MakeReadable(Texture texture)
        {
            RenderTexture rt = RenderTexture.GetTemporary(
                texture.width, texture.height, 0,
                RenderTextureFormat.Default, RenderTextureReadWrite.Linear);

            Graphics.Blit(texture, rt);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;

            Texture2D readableTex = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
            readableTex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            readableTex.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);

            return readableTex;
        }

        public static async Task<Texture2D> Base64ToTexture(string base64)
        {
            byte[] imageBytes = await Task.Run(() => Convert.FromBase64String(base64));
            Texture2D texture = new Texture2D(2, 2); // 大小會自動調整
            texture.LoadImage(imageBytes);
            return texture;
        }

        private static void TakePicture(Action<Texture2D> callback)
        {
            NativeCamera.TakePicture((path) =>
            {
                Debug.Log("Image path: " + path);
                if (path != null)
                {
                    Texture2D originalTexture = NativeCamera.LoadImageAtPath(path, maxSize: 1024, markTextureNonReadable: false);
                    if (originalTexture == null)
                    {
                        Debug.LogWarning("Couldn't load texture from " + path);
                        callback?.Invoke(null);
                        return;
                    }

                    // 壓縮至原本一半的尺寸
                    int width = originalTexture.width / 2;
                    int height = originalTexture.height / 2;
                    Debug.Log($"Original size: {originalTexture.width}x{originalTexture.height}");
                    Bilinear(originalTexture, width, height);
                    Debug.Log($"Resized size: {originalTexture.width}x{originalTexture.height}");

                    callback?.Invoke(originalTexture);
                }
            });
        }

        public static void Bilinear(Texture2D tex, int newWidth, int newHeight)
        {
            Texture2D newTex = new Texture2D(newWidth, newHeight, tex.format, false);
            float ratioX = 1.0f / ((float)newWidth / (tex.width - 1));
            float ratioY = 1.0f / ((float)newHeight / (tex.height - 1));
            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    float px = x * ratioX;
                    float py = y * ratioY;
                    newTex.SetPixel(x, y, tex.GetPixelBilinear(px / tex.width, py / tex.height));
                }
            }
            newTex.Apply();
            tex.Reinitialize(newWidth, newHeight);
            tex.SetPixels(newTex.GetPixels());
            tex.Apply();
        }


        // 尚未詳細規劃
        //private static void RecordVideo()
        //{
        //    NativeCamera.RecordVideo((path) =>
        //    {
        //        Debug.Log("Video path: " + path);
        //        if (path != null)
        //        {
        //            // Play the recorded video
        //            Handheld.PlayFullScreenMovie("file://" + path); // Build 紀錄 : WebGL context not support Handheld
        //        }
        //    });
        //}
    }

}
