using System.IO;
using System.Linq;
using UnityEditor;

// Prevents materials from being saved while we are in play mode.
// See: https://answers.unity.com/questions/1537335/why-changing-material-shader-at-runtime-also-affec.html
namespace Editor
{
    public class PlayModeMaterials : UnityEditor.AssetModificationProcessor
    {
        private static string[] OnWillSaveAssets(string[] paths)
        {
            return EditorApplication.isPlaying ? paths.Where(path => Path.GetExtension(path) != ".mat").ToArray() : paths;
        }
    }
}