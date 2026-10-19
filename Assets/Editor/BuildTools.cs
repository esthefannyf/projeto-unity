using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CrystalRuins.EditorTools
{
    public static class BuildTools
    {
        const string OutputPath = "Build/WebGL";

        [MenuItem("Tools/Build WebGL")]
        public static void BuildWebGL()
        {
            // itch.io serves the files as-is, so compression has to be off.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = false;

            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = OutputPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"WebGL build: {report.summary.result} ({report.summary.totalSize / 1024 / 1024} MB) -> {OutputPath}");
        }
    }
}
