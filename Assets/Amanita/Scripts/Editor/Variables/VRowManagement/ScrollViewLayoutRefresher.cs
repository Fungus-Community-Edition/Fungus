using UnityEngine.UIElements;

namespace Amanita.VScripting.EditorUtils
{
    public class ScrollViewLayoutRefresher : ILayoutRefresher
    {
        public void Refresh(ScrollView sView)
        {
            var cContainer = sView.contentContainer;
            cContainer.style.display = DisplayStyle.None;
            sView.schedule.Execute(() => cContainer.style.display = DisplayStyle.Flex).StartingIn(0);
        }
    }

    public interface ILayoutRefresher
    {
        void Refresh(ScrollView sv);
    }
}