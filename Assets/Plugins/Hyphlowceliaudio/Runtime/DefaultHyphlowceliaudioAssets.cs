using UnityEngine;

namespace AtMycelia.Hyphlowceliaudio
{
    public class DefaultHyphlowceliaudioAssets
    {
        public static MyceliaudioTweenAdapter MyceliaudioTweener
        {
            get
            {
                if (_myceliaudioTweener == null)
                {
                    _myceliaudioTweener = Resources.LoadAll<MyceliaudioTweenAdapter>("")[0];
                    if (_myceliaudioTweener == null)
                    {
                        Debug.LogError("Could not find MyceliaudioTweenAdapter in Resources folder. Please make sure it is there and try again.");
                    }
                }
                return _myceliaudioTweener;
            }
        }
        private static MyceliaudioTweenAdapter _myceliaudioTweener;
    }
}