using UnityEditor;
using UnityEditor.SceneManagement;

namespace ColorGateBlast.EditorTools
{
    /// <summary>
    /// Batch-mode launcher for the autoplay bot:
    /// Unity.exe -batchmode -projectPath ... -executeMethod ColorGateBlast.EditorTools.AutoPlayLauncher.Run -cgbBot [-cgbFrom 1 -cgbTo 40]
    /// Writes autoplay_results.txt next to the Assets folder and exits Unity with code 0 only if every level was won without errors.
    /// </summary>
    public static class AutoPlayLauncher
    {
        public static void Run()
        {
            EditorSceneManager.OpenScene(EditorSetupColorGateBlast.ScenePath);
            EditorApplication.EnterPlaymode();
        }
    }
}
