#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;
using Color = UnityEngine.Color;
using Graphics = UnityEngine.Graphics;
using Object = UnityEngine.Object;

namespace lilToon
{
    public class lilTextureUtils
    {
        //------------------------------------------------------------------------------------------------------------------------------
        // Gradient

        #region

        internal static void GradientEditor(Material material, Gradient ingrad, MaterialProperty texprop,
            bool setLinear = false)
        {
#if UNITY_2018_3_OR_NEWER
            ingrad = EditorGUILayout.GradientField(lilLanguageManager.GetLoc("sGradColor"), ingrad);
#else
                var setMethod = typeof(EditorGUILayout).GetMethod(
                    "GradientField",
                    BindingFlags.NonPublic | BindingFlags.Static,
                    null,
                    new [] {typeof(string), typeof(Gradient), typeof(GUILayoutOption[])},
                    null);
                if(setMethod != null) {
                    ingrad =
 (Gradient)setMethod.Invoke(null, new object[]{lilLanguageManager.GetLoc("sGradColor"), ingrad, null});;
                }
#endif
            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUI.indentLevel * 16);
            if (GUILayout.Button("Test")) texprop.textureValue = GradientToTexture(ingrad, setLinear);
            if (GUILayout.Button("Save"))
            {
                var tex = GradientToTexture(ingrad, setLinear);
                tex = SaveTextureToPng(material, tex, texprop.name);
                if (setLinear) SetLinear(tex);
                texprop.textureValue = tex;
            }

            GUILayout.EndHorizontal();
        }

        internal static void GradientEditor(Material material, string emissionName, Gradient ingrad,
            MaterialProperty texprop, bool setLinear = false)
        {
            ingrad = MaterialToGradient(material, emissionName);
#if UNITY_2018_3_OR_NEWER
            ingrad = EditorGUILayout.GradientField(lilLanguageManager.GetLoc("sGradColor"), ingrad);
#else
                var setMethod = typeof(EditorGUILayout).GetMethod(
                    "GradientField",
                    BindingFlags.NonPublic | BindingFlags.Static,
                    null,
                    new [] {typeof(string), typeof(Gradient), typeof(GUILayoutOption[])},
                    null);
                if(setMethod != null) {
                    ingrad =
 (Gradient)setMethod.Invoke(null, new object[]{lilLanguageManager.GetLoc("sGradColor"), ingrad, null});;
                }
#endif
            GradientToMaterial(material, emissionName, ingrad);
            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUI.indentLevel * 16);
            if (GUILayout.Button("Test")) texprop.textureValue = GradientToTexture(ingrad, setLinear);
            if (GUILayout.Button("Save"))
            {
                var tex = GradientToTexture(ingrad, setLinear);
                tex = SaveTextureToPng(material, tex, texprop.name);
                if (setLinear) SetLinear(tex);
                texprop.textureValue = tex;
            }

            GUILayout.EndHorizontal();
        }

        private static void SetLinear(Texture2D tex)
        {
            if (tex != null)
            {
                var path = AssetDatabase.GetAssetPath(tex);
                var textureImporter = (TextureImporter)AssetImporter.GetAtPath(path);
                textureImporter.sRGBTexture = false;
                AssetDatabase.ImportAsset(path);
            }
        }

        private static Gradient MaterialToGradient(Material material, string emissionName)
        {
            var outgrad = new Gradient();
            var colorKey = new GradientColorKey[material.GetInt(emissionName + "ci")];
            var alphaKey = new GradientAlphaKey[material.GetInt(emissionName + "ai")];
            for (var i = 0; i < colorKey.Length; i++)
            {
                colorKey[i].color = material.GetColor(emissionName + "c" + i);
                colorKey[i].time = material.GetColor(emissionName + "c" + i).a;
            }

            for (var j = 0; j < alphaKey.Length; j++)
            {
                alphaKey[j].alpha = material.GetColor(emissionName + "a" + j).r;
                alphaKey[j].time = material.GetColor(emissionName + "a" + j).a;
            }

            outgrad.SetKeys(colorKey, alphaKey);
            return outgrad;
        }

        private static void GradientToMaterial(Material material, string emissionName, Gradient ingrad)
        {
            material.SetInt(emissionName + "ci", ingrad.colorKeys.Length);
            material.SetInt(emissionName + "ai", ingrad.alphaKeys.Length);
            for (var ic = 0; ic < ingrad.colorKeys.Length; ic++)
                material.SetColor(emissionName + "c" + ic,
                    new Color(ingrad.colorKeys[ic].color.r, ingrad.colorKeys[ic].color.g, ingrad.colorKeys[ic].color.b,
                        ingrad.colorKeys[ic].time));
            for (var ia = 0; ia < ingrad.alphaKeys.Length; ia++)
                material.SetColor(emissionName + "a" + ia,
                    new Color(ingrad.alphaKeys[ia].alpha, 0, 0, ingrad.alphaKeys[ia].time));
        }

        private static Texture2D GradientToTexture(Gradient grad, bool setLinear = false)
        {
            var tex = new Texture2D(128, 4, TextureFormat.ARGB32, true, setLinear);

            // Set colors
            for (var w = 0; w < tex.width; w++)
            for (var h = 0; h < tex.height; h++)
                tex.SetPixel(w, h, grad.Evaluate((float)w / tex.width));

            tex.Apply();
            return tex;
        }

        #endregion

        //------------------------------------------------------------------------------------------------------------------------------
        // Load Texture

        #region

        private static void GetReadableTexture(ref Texture2D tex)
        {
            if (tex == null) return;

#if UNITY_2018_3_OR_NEWER
            if (!tex.isReadable)
#endif
            {
                var bufRT = RenderTexture.active;
                var texR = RenderTexture.GetTemporary(tex.width, tex.height);
                Graphics.Blit(tex, texR);
                RenderTexture.active = texR;
                tex = new Texture2D(texR.width, texR.height);
                tex.ReadPixels(new Rect(0, 0, texR.width, texR.height), 0, 0);
                tex.Apply();
                RenderTexture.active = bufRT;
                RenderTexture.ReleaseTemporary(texR);
            }
        }

        public static void LoadTexture(ref Texture2D tex, string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            GetReadableTexture(ref tex);
            if (path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
            {
                var bytes = File.ReadAllBytes(Path.GetFullPath(path));
                tex.LoadImage(bytes);
            }

            if (tex != null) tex.filterMode = FilterMode.Bilinear;
        }

        #endregion

        //------------------------------------------------------------------------------------------------------------------------------
        // Save Texture

        #region

        internal static Texture2D SaveTextureToPng(Material material, Texture2D tex, string texname,
            string customName = "")
        {
            var path = AssetDatabase.GetAssetPath(material.GetTexture(texname));
            if (string.IsNullOrEmpty(path)) path = AssetDatabase.GetAssetPath(material);

            var filename = Path.GetFileNameWithoutExtension(path);
            if (!string.IsNullOrEmpty(customName)) filename += customName;
            else filename += "_2";
            if (!string.IsNullOrEmpty(path))
                path = EditorUtility.SaveFilePanel("Save Texture", Path.GetDirectoryName(path), filename, "png");
            else path = EditorUtility.SaveFilePanel("Save Texture", "Assets", tex.name + ".png", "png");
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllBytes(path, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.Refresh();
                return AssetDatabase.LoadAssetAtPath<Texture2D>(path.Substring(path.IndexOf("Assets")));
            }

            return (Texture2D)material.GetTexture(texname);
        }

        internal static Texture2D SaveTextureToPng(Texture2D tex, Texture2D origTex)
        {
            var path = AssetDatabase.GetAssetPath(origTex);
            if (!string.IsNullOrEmpty(path))
                path = EditorUtility.SaveFilePanel("Save Texture", Path.GetDirectoryName(path),
                    Path.GetFileNameWithoutExtension(path) + "_alpha", "png");
            else path = EditorUtility.SaveFilePanel("Save Texture", "Assets", tex.name + "_alpha.png", "png");
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllBytes(path, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.Refresh();
                return AssetDatabase.LoadAssetAtPath<Texture2D>(path.Substring(path.IndexOf("Assets")));
            }

            return origTex;
        }

        internal static Texture2D SaveTextureToPng(Texture2D tex, string path, string customName = "")
        {
            var filename = customName + Path.GetFileNameWithoutExtension(path);
            if (!string.IsNullOrEmpty(path))
                path = EditorUtility.SaveFilePanel("Save Texture", Path.GetDirectoryName(path), filename, "png");
            else path = EditorUtility.SaveFilePanel("Save Texture", "Assets", filename, "png");
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllBytes(path, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.Refresh();
                return AssetDatabase.LoadAssetAtPath<Texture2D>(path.Substring(path.IndexOf("Assets")));
            }

            return tex;
        }

        public static string SaveTextureToPng(string path, string add, Texture2D tex)
        {
            var savePath = Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(path) + add + ".png";
            File.WriteAllBytes(savePath, tex.EncodeToPNG());
            return savePath;
        }

        #endregion

        //------------------------------------------------------------------------------------------------------------------------------
        // Gif to Atlas

        #region

#if SYSTEM_DRAWING
        public static string ConvertGifToAtlas(Object tex)
        {
            int frameCount, loopXY, duration;
            float xScale, yScale;
            return ConvertGifToAtlas(tex, out frameCount, out loopXY, out duration, out xScale, out yScale);
        }

        public static string ConvertGifToAtlas(Object tex, out int frameCount, out int loopXY, out int duration,
            out float xScale, out float yScale)
        {
            var path = AssetDatabase.GetAssetPath(tex);
            var origGif = Image.FromFile(path);
            var dimension = new FrameDimension(origGif.FrameDimensionsList[0]);
            frameCount = origGif.GetFrameCount(dimension);
            loopXY = Mathf.CeilToInt(Mathf.Sqrt(frameCount));
            duration = BitConverter.ToInt32(origGif.GetPropertyItem(20736).Value, 0);
            var finalWidth = 1;
            var finalHeight = 1;
            if (EditorUtility.DisplayDialog(lilLanguageManager.GetLoc("sDialogGifToAtlas"),
                    lilLanguageManager.GetLoc("sUtilGif2AtlasPow2"), lilLanguageManager.GetLoc("sYes"),
                    lilLanguageManager.GetLoc("sNo")))
            {
                while (finalWidth < origGif.Width * loopXY) finalWidth *= 2;
                while (finalHeight < origGif.Height * loopXY) finalHeight *= 2;
            }
            else
            {
                finalWidth = origGif.Width * loopXY;
                finalHeight = origGif.Height * loopXY;
            }

            var atlasTexture = new Texture2D(finalWidth, finalHeight);
            xScale = (float)(origGif.Width * loopXY) / finalWidth;
            yScale = (float)(origGif.Height * loopXY) / finalHeight;
            for (var x = 0; x < finalWidth; x++)
            for (var y = 0; y < finalHeight; y++)
                atlasTexture.SetPixel(x, finalHeight - 1 - y, Color.clear);
            for (var i = 0; i < frameCount; i++)
            {
                var offsetX = i % loopXY;
                var offsetY = Mathf.FloorToInt(i / loopXY);
                origGif.SelectActiveFrame(dimension, i);
                var frame = new Bitmap(origGif.Width, origGif.Height);
                System.Drawing.Graphics.FromImage(frame).DrawImage(origGif, Point.Empty);

                for (var x = 0; x < frame.Width; x++)
                for (var y = 0; y < frame.Height; y++)
                {
                    var sourceColor = frame.GetPixel(x, y);
                    atlasTexture.SetPixel(x + frame.Width * offsetX, finalHeight - frame.Height * offsetY - 1 - y,
                        new Color32(sourceColor.R, sourceColor.G, sourceColor.B, sourceColor.A));
                }
            }

            atlasTexture.Apply();

            // Save
            var savePath = SaveTextureToPng(path, "_gif2png_" + loopXY + "_" + frameCount + "_" + duration,
                atlasTexture);
            AssetDatabase.Refresh();
            return savePath;
        }
#else
            public static string ConvertGifToAtlas(Object tex)
            {
                int frameCount, loopXY, duration;
                float xScale, yScale;
                return ConvertGifToAtlas(tex, out frameCount, out loopXY, out duration, out xScale, out yScale);
            }

            public static string ConvertGifToAtlas(Object tex, out int frameCount, out int loopXY, out int duration, out float xScale, out float yScale)
            {
                frameCount = 0;
                loopXY = 0;
                duration = 0;
                xScale = 1.0f;
                yScale = 1.0f;
                return "";
            }
#endif

        #endregion

        //------------------------------------------------------------------------------------------------------------------------------
        // Cube to PNG

        #region

        private static Texture3D ReadCube(string path)
        {
            var size = -1;
            var sr = new StreamReader(path);
            string line;
            var colors = new List<Color>();
            while ((line = sr.ReadLine()) != null)
            {
                line = line.Trim();
                var c = line.IndexOf("#");
                if (c >= 0) line = line.Substring(0, c);

                if (line.StartsWith("TITLE") || line.StartsWith("DOMAIN_")) continue;
                if (line.StartsWith("LUT_3D_SIZE"))
                {
                    try
                    {
                        size = int.Parse(line.Substring(11).TrimStart());
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        sr.Close();
                        return null;
                    }

                    continue;
                }

                var vals = line.Split();
                if (vals.Length != 3) continue;
                try
                {
                    colors.Add(new Color(
                        float.Parse(vals[0], CultureInfo.InvariantCulture),
                        float.Parse(vals[1], CultureInfo.InvariantCulture),
                        float.Parse(vals[2], CultureInfo.InvariantCulture)
                    ));
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    sr.Close();
                    return null;
                }
            }

            sr.Close();
            if (size <= 0 || colors == null || colors.Count < size * size * size) return null;

            var tex = new Texture3D(size, size, size, TextureFormat.RGBA32, false);
            tex.SetPixels(colors.GetRange(0, size * size * size).ToArray());
            tex.Apply();
            return tex;
        }

        internal static Texture2D ConvertLUT3Dto2D(Texture3D tex)
        {
            var resX = 16;
            var resY = 1;
            var width = resX * resY * resX;
            var height = resX * resY * resY;

            var material = new Material(lilShaderManager.ltsbaker);
            material.SetTexture("_MainTex3D", tex);
            material.SetFloat("_ResX", resX);
            material.SetFloat("_ResY", resY);
            material.EnableKeyword("_LUT3D_TO_2D");

            var outTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

            var bufRT = RenderTexture.active;
            var srcTexture = RenderTexture.GetTemporary(width, height);
            var dstTexture = RenderTexture.GetTemporary(width, height);
            Graphics.Blit(srcTexture, dstTexture, material);
            RenderTexture.active = dstTexture;
            outTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            outTexture.Apply();
            RenderTexture.active = bufRT;
            RenderTexture.ReleaseTemporary(dstTexture);

            return outTexture;
        }

        internal static void ConvertLUTToPNG(Object obj)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            Texture3D texOrig = null;
            if (path.EndsWith(".cube")) texOrig = ReadCube(path);
            if (texOrig == null)
            {
                EditorUtility.DisplayDialog("[lilToon] Convert LUT to PNG",
                    lilLanguageManager.GetLoc("sUtilInvalidFormat"), lilLanguageManager.GetLoc("sOK"));
                return;
            }

            var tex = ConvertLUT3Dto2D(texOrig);
            SaveTextureToPng(path, "_conv", tex);
            AssetDatabase.Refresh();
        }

        #endregion
    }
}
#endif