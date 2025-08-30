using UnityEngine.UIElements;

namespace Amanita.VScripting.EditorUtils
{
    public class ListViewLayoutRefresher : ILayoutRefresher
    {
        public void Refresh(ListView sView)
        {
            sView.RefreshItems();
            return;
            var cContainer = sView.contentContainer;
            cContainer.style.display = DisplayStyle.None;
            sView.schedule.Execute(() => cContainer.style.display = DisplayStyle.Flex).StartingIn(0);
        }
    }

    public interface ILayoutRefresher
    {
        void Refresh(ListView sv);
    }
}