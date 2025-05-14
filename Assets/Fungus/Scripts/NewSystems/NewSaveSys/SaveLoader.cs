using UnityEngine;

namespace Amanita.SaveSys
{
    public class SaveLoader
    {
        public void ApplySave(SaveData saveData)
        {
            Debug.Log($"Applying save: {saveData.SaveID}");
            // Add logic for restoring game state here (e.g., updating Flowchart variables)
        }
    }
}