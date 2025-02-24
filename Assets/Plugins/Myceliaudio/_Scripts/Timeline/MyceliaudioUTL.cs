using UnityEngine;
using UnityEngine.Playables;

namespace Myceliaudio
{
    public class MyceliaudioUTL : MonoBehaviour, INotificationReceiver
    {
        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification is not PlayAudioMarker)
            {
                return;
            }

            // We want to apply the skillSpawnPos script logic here
            var marker = (PlayAudioMarker) notification;

            foreach (var argEl in marker.ArgSet)
            {
                AudioSystem.S.Play(argEl);
            }
            
        }
    }
}