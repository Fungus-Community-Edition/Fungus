using System.Collections;
using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace AtMycelia.SaveSys
{
    public static class SaveSystemBootstrapper
    {
        
        public static ISaveSystemInstaller Installer
        {
            get
            {
                return _lastInstaller;
            }
            set
            {
                if (value == null)
                {
                    Debug.LogError($"[SaveSystemBootstrapper] Cannot set Installer to null!");
                    return;
                }

                if (InstallerChanges == _maxInstallerChangesAllowed)
                {
                    Debug.LogError($"[SaveSystemBootstrapper] Installer has already been changed {InstallerChanges} times, " +
                        $"which is the maximum allowed. Further changes will be ignored. Please ensure that the installer " +
                        $"is only set once, and before the first scene load.");
                    return;
                }

                if (value != _lastInstaller)
                {
                    InstallerChanges++;
                    _lastInstaller = value;
                }
            }
        }
        
        private static ISaveSystemInstaller _lastInstaller = new SaveSystemInstaller();

        public static int InstallerChanges { get; private set; } = 0;
        private static readonly int _maxInstallerChangesAllowed = 1;

        public static SaveSystemInstallContext InstallContext { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (!Application.isPlaying)
            {
                return;
            }


            _lastInstaller.Init(InstallContext); 
            // ^So the Save System has some modules ready right away. We do a delay after this so that
            // third party code can inject their own dependencies.
            GameObject tempGo = new GameObject("[SaveSystemBootstrapper Temp GO]");
            _tempMb = tempGo.AddComponent<BootstrapperWaiter>();
            _tempMb.StartCoroutine(InitAltInstallerAfterDelay());
        }

        private static MonoBehaviour _tempMb;

        // We do this in a coroutine so that we're still on the main thread, which allows us
        // to use Unity APIs in the installer if needed. We also want to delay it slightly
        // to give time for any user code that sets up the installer or install context
        // to run first.
        private static IEnumerator InitAltInstallerAfterDelay()
        {
            yield return DoWait();
            IEnumerator DoWait()
            {
                float timer = 0f;
                while (timer < _installDelaySeconds)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }
            }

#if UNITY_EDITOR
            if (!Enabled)
            {
                yield break;
            }
#endif
            if (InstallerChanges > 0)
            {
                Installer.Init(InstallContext);
            }

            SaveSystem.Init();
            // ^We only want to init the save system after the installers managed to inject 
            // their dependencies. We don't want the installers themselves to init the save
            // sys, since that would block others from injecting their dependencies if they
            // run after the first installer.

            UnityObj.Destroy(_tempMb.gameObject);
        }

        private static readonly float _installDelaySeconds = 0.15f;

#if UNITY_EDITOR
        // For testing only.
        public static bool Enabled { get; set; } = true;
        public static void ResetStaticsForTest()
        {
            _lastInstaller = new SaveSystemInstaller();
            InstallContext = null;
            InstallerChanges = 0;
        }

#endif
    }
}