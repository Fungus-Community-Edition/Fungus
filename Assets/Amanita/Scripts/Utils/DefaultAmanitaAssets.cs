using Amanita.SaveSys;
using Amanita.Tweening;
using UnityEngine;

namespace Amanita
{
    public static class DefaultAmanitaAssets 
    {
        public static SaveStorageSettings SaveStorageSettings
        {
            get
            {
                return storageSettings;
            }
            set
            {
                storageSettings = value;
            }
        }
        private static SaveStorageSettings storageSettings;
        public static DefaultTweenAdapter TweenAdapter;
        public static SaveReader SaveReader;
        public static SaveWriter SaveWriter;
        public static Decryptor Decryptor;
        public static Encryptor Encryptor;
        public static SaveSystemSettings SaveSystemSettings;
    }
}