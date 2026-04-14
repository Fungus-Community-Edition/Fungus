using AtMycelia.Hyphlow.Tweening;
using UnityEngine;

namespace AtMycelia.Amanita
{
    public class DefaultAmanitaAssets : MonoBehaviour
    {
        public static MyceliaudioTweenAdapter MyceliaudioTweener
        {
            get
            {
                if (_myceliaudioTweener == null)
                {
                    _myceliaudioTweener = SOUtils.EnsureSOExists<MyceliaudioTweenAdapter>("Myceliaudio", "MA_TweenAdapter");
                }

                return _myceliaudioTweener;
            }
        }
        private static MyceliaudioTweenAdapter _myceliaudioTweener;
    }
}