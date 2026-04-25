using System.IO;

namespace AtMycelia.Myceliaudio.Editor
{
    public static class Paths
    {
        static Paths()
        {
            ToPlugins = Path.Combine("Assets", "_Plugins");
            ToMainSys = Path.Combine(ToPlugins, "CGT_AudioManagementSys");

            ToEditor = Path.Combine(ToMainSys, "Editor");
            ToEditorWindow = Path.Combine(ToEditor, "Window");
            ToStyles = Path.Combine(ToEditor, "Styles");
        }

        public static string ToPlugins { get; private set; }
        public static string ToMainSys { get; private set; }
        public static string ToEditor { get; private set; }
        public static string ToEditorWindow { get; private set; }
        public static string ToStyles { get; private set; }

    }
}