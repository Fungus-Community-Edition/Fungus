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
                if (storageSettings == null)
                {
                    storageSettings = ScriptableObject.CreateInstance<SaveStorageSettings>();
                }
                return storageSettings;
            }
            set
            {
                storageSettings = value;
            }
        }
        private static SaveStorageSettings storageSettings;
        public static DefaultTweenAdapter TweenAdapter;
        public static Decryptor Decryptor;
        public static Encryptor Encryptor;
    }
}