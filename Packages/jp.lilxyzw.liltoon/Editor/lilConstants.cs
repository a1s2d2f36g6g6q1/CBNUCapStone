#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace lilToon
{
    public class lilConstants
    {
        public const string currentVersionName = "1.9.0";
        public const int currentVersionValue = 45;

        internal const string boothURL = "https://lilxyzw.booth.pm/";
        internal const string githubURL = "https://github.com/lilxyzw/lilToon";
        internal const string versionInfoURL = "https://raw.githubusercontent.com/lilxyzw/lilToon/master/version.json";

        internal static readonly string[] mainTexCheckWords =
        {
            "mask", "shadow", "shade", "outline", "normal", "bumpmap", "matcap", "rimlight", "emittion", "reflection",
            "specular", "roughness", "smoothness", "metallic", "metalness", "opacity", "parallax", "displacement",
            "height", "ambient", "occlusion"
        };

        public static readonly Vector2 defaultTextureOffset = new(0.0f, 0.0f);
        public static readonly Vector2 defaultTextureScale = new(1.0f, 1.0f);
        public static readonly Vector4 defaultScrollRotate = new(0.0f, 0.0f, 0.0f, 0.0f);
        public static readonly Vector4 defaultHSVG = new(0.0f, 1.0f, 1.0f, 1.0f);
        public static readonly Vector4 defaultKeys = new(0.0f, 0.0f, 0.0f, 0.0f);
        public static readonly Vector4 defaultDecalAnim = new(1.0f, 1.0f, 1.0f, 30.0f);
        public static readonly Vector4 defaultDissolveParams = new(0.0f, 0.0f, 0.5f, 0.1f);
        public static readonly Vector4 defaultDistanceFadeParams = new(0.1f, 0.01f, 0.0f, 0.0f);

        public static readonly Color lineColor = EditorGUIUtility.isProSkin
            ? new Color(0.35f, 0.35f, 0.35f, 1.0f)
            : new Color(0.4f, 0.4f, 0.4f, 1.0f);
    }
}
#endif