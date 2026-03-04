using System.Threading.Tasks;
using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace AtMycelia.SaveSys
{
    public static class SaveSystemBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            SaveSystemInstaller installer = new SaveSystemInstaller();

            // TODO: Set up an interface for third-party code to be able to inject themselves
            // into the installer process, so they can add their own factories, etc. We then
            // look through all the types in the solution, instantiating whatever implements
            // that interface, and calling a method on it to let it do its thing.
            Task.Run(async () =>
            {
                int tinyDelay = 100; // milliseconds
                await Task.Delay(tinyDelay); // Band-aid for us not having a dependency injection system yet.

                installer.Init();
            });
        }
    }
}