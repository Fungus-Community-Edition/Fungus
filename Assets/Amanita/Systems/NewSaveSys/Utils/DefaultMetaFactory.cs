using UnityEngine;

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
            var meta = new SaveMetaData
            {
                SlotNumber = slotNumber,
                ProgressMarkers = SaveSystem.S.ProgressMarkers
            };

            meta.TimeStamp = System.DateTime.UtcNow;

            meta.RegisterCurrentSceneInfo();

            string version = versionProvider.GetVersion();
            if (!string.IsNullOrEmpty(version))
            {
                meta.SaveVersion = version;
            }

            return meta;
        }
    }
}