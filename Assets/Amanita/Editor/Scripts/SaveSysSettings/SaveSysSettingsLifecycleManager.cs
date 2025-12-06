using UnityEditor;
using UnityEngine;

namespace Amanita.SaveSys.EditorUtils
{
    public sealed class SaveSysSettingsLifecycleManager
    {
        private static readonly Vector2 WindowSize = new Vector2(600, 700);
        
        public void HandleOnRefresh(SaveSysSettingsWindow wnd)
        {
            ApplySizeConstraints(wnd);
            EnsureSingleInstance(wnd);
        }

        public void HandleOnEnable(SaveSysSettingsWindow wnd)
        {
            ApplySizeConstraints(wnd);
            EnsureSingleInstance(wnd);
        }

        public void ApplySizeConstraints(EditorWindow wnd)
        {
            wnd.minSize = wnd.maxSize = WindowSize;
        }

        public void EnsureSingleInstance(SaveSysSettingsWindow wnd)
        {
            if (_instance == null)
            {
                _instance = wnd;
                return;
            }

            if (_instance != wnd)
            {
                wnd.Close();
            }
        }

        private static SaveSysSettingsWindow _instance;

        public void HandleOnDestroy(SaveSysSettingsWindow wnd, SaveSysSettingsUiRegistrar registrar)
        {
            if (_instance == wnd)
            {
                registrar.RecordCurrentChoices();
                _instance = null;
            }
        }
    }
}