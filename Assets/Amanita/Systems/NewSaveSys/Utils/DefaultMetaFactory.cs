using Amanita.Utils;
using System.Threading;
using UnityEngine.SceneManagement;
using DateTime = System.DateTime;
using Guid = System.Guid;

namespace Amanita.SaveSys
{
    public class DefaultMetaFactory : IMetaFactory
    {
        public DefaultMetaFactory(IVersionProvider versionProvider)
        {
            this.versionProvider = versionProvider;
        }

        protected readonly IVersionProvider versionProvider;

        public virtual ISaveMetaData CreateMeta(int slotNumber)
        {
            string saveId = Guid.NewGuid().ToString();
            DateTime timeStamp = DateTime.UtcNow;
            string version = versionProvider.GetVersion();
            if (string.IsNullOrEmpty(version))
            {
                version = SaveSysConstants.DefaultSaveVer;
            }

            SaveMetaData meta = new SaveMetaData(saveId, timeStamp, version, slotNumber);
            RegisterCurrentSceneInfo(meta);

            return meta;
        }

        void RegisterCurrentSceneInfo(SaveMetaData saveMeta)
        {
            void GetTheInfo()
            {
                var scene = SceneManager.GetActiveScene();
                saveMeta.SceneName = scene.name;
                saveMeta.SceneBuildIndex = scene.buildIndex;
            }

            bool onMainThread = UnityThreadUtil.IsMainThread;
            if (onMainThread)
            {
                GetTheInfo();
            }
            else
            {
                using (var countdown = new CountdownEvent(1))
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        GetTheInfo();
                        countdown.Signal();
                    }
                    );

                    countdown.Wait();
                }
            }

        }
    }
}