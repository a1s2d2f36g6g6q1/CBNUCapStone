#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace lilToon
{
    internal class lilToonAssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            var id = Shader.PropertyToID("_lilToonVersion");
            foreach (var path in importedAssets)
            {
                if (!path.EndsWith(".mat")) continue;
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (!lilMaterialUtils.CheckShaderIslilToon(material)) continue;

                lilStartup.MigrateMaterial(material);
                if (material.shader.name.Contains("Multi")) lilMaterialUtils.SetupMultiMaterial(material);
            }
        }
    }
}
#endif