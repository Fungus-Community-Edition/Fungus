using UnityEngine;
using UnityEngine.UIElements;

public class KerningTestChart : MonoBehaviour
{
    [SerializeField] private UIDocument doc;
    [SerializeField] private int minSize = 10;
    [SerializeField] private int maxSize = 20;

    private void OnEnable()
    {
        var container = doc.rootVisualElement.Q("chart");

        for (int size = minSize; size <= maxSize; size++)
        {
            var lbl = new Label($"Size {size}: AVATAR ToTo WAVE");
            lbl.AddToClassList("size-sample");
            lbl.style.fontSize = size;
            container.Add(lbl);
        }
    }
}