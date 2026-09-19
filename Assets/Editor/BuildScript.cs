using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace DeadSignal.EditorTools
{
    public static class BuildScript
    {
        private const string OutputPath = "Build/APK/DeadSignal.apk";

        [MenuItem("Build/Build APK for Quest")]
        public static void BuildAndroid()
        {
            var scenes = System.Array.ConvertAll(
                EditorBuildSettings.scenes,
                s => s.path);

            if (scenes.Length == 0)
            {
                Debug.LogError("No scenes in Build Settings. Add the room scene first.");
                EditorApplication.Exit(1);
                return;
            }

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"Build failed with {report.summary.totalErrors} error(s).");
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log($"Build succeeded: {OutputPath}");
        }
    }
}
