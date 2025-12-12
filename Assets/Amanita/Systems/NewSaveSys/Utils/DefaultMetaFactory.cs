using UnityEngine;
using DateTime = System.DateTime;

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
            string saveId = System.Guid.NewGuid().ToString();
            SaveMetaData meta = new SaveMetaData()
            {
                SaveID = saveId,
                TimeStamp = DateTime.UtcNow,
                SlotNumber = slotNumber
            };
            meta.SlotNumber = slotNumber;

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