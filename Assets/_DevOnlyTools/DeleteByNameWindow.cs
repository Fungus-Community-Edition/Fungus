using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class DeleteByNameWindow : EditorWindow
{
    private TextField nameField;

    [MenuItem("Window/Atelier Mycelia/Delete By Name")]
    public static void ShowWindow()
    {
        var wnd = GetWindow<DeleteByNameWindow>();
        wnd.titleContent = new GUIContent("Delete By Name");
    }

    public void CreateGUI()
    {
        var root = rootVisualElement;

        // Text field
        nameField = new TextField("Name to Delete:");
        nameField.style.marginBottom = 8;
        root.Add(nameField);

        // Button
        var deleteButton = new Button(OnDeleteClicked)
        {
            text = "Delete Matching Objects"
        };
        root.Add(deleteButton);
    }

    private void OnDeleteClicked()
    {
        string target = nameField.value?.Trim();
        if (string.IsNullOrEmpty(target))
        {
            Debug.LogWarning("No name entered.");
            return;
        }

        int deletedCount = 0;
        var scene = SceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();

        foreach (var root in roots)
        {
            // Search recursively
            deletedCount += DeleteMatchesRecursive(root.transform, target);
        }

        Debug.Log($"Deleted {deletedCount} GameObject(s) matching \"{target}\" (case-insensitive).");
    }

    private int DeleteMatchesRecursive(Transform t, string target)
    {
        int count = 0;

        // Check children first (so we don't break the hierarchy while iterating)
        for (int i = t.childCount - 1; i >= 0; i--)
        {
            count += DeleteMatchesRecursive(t.GetChild(i), target);
        }

        // Check this object
        if (t.name.Equals(target, System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log($"DeleteByNameWindow: Deleting GameObject \"{t.name}\" at " +
                $"path \"{GetFullPath(t)}\". in scene {CurrentSceneName}", t.gameObject);
            Undo.DestroyObjectImmediate(t.gameObject);
            count++;
        }

        return count;
    }

    private string CurrentSceneName => SceneManager.GetActiveScene().name;

    private object GetFullPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}
