using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace AtMycelia.Amanita
{
    /// <summary>
    /// Helper component for loading a new scene.
    /// A fullscreen loading image is displayed while loading the new scene.
    /// All Rooms are destroyed and unused assets are released from memory before loading the new 
    /// scene to minimize memory footprint.
    /// For streaming Web Player builds, the loading image will be displayed until the requested 
    /// level has finished downloading.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        protected Texture2D loadingTexture;
        protected string sceneToLoad;
        protected bool displayedImage;

        protected virtual void Start()
        {
            StartCoroutine(DoLoadBlock());
        }

        protected virtual IEnumerator DoLoadBlock()
        {
            // Wait until loading image has been displayed in OnGUI
            while (loadingTexture != null && 
                   !displayedImage)
            {
                yield return new WaitForEndOfFrame();
            }

            // Wait for objects to actually be destroyed at end of run loop
            yield return new WaitForEndOfFrame();

            // All Room assets should no longer be referenced now, so unload them.
            yield return Resources.UnloadUnusedAssets();

            // Wait until scene has finished downloading (WebPlayer only)
            while (!Application.CanStreamedLevelBeLoaded(sceneToLoad))
            {
                yield return new WaitForEndOfFrame();
            }

            // Load the scene (happens at end of frame)
            SceneManager.LoadScene(sceneToLoad);

            yield return new WaitForEndOfFrame();

            // Clean up any remaining unused assets
            Resources.UnloadUnusedAssets();

            // We're now finished with the SceneLoader
            Destroy(gameObject);
        }

        protected virtual void OnGUI()
        {
            if (loadingTexture == null)
            {
                return;
            }

            GUI.depth = -2000;
            
            float height = Screen.height;
            float width = (float)loadingTexture.width * (height / (float)loadingTexture.height);
            
            float xPos = Screen.width / 2 - width / 2;
            float yPos = 0;
            
            Rect rect = new Rect(xPos, yPos, width, height);

            GUI.DrawTexture(rect, loadingTexture);

            if (Event.current.type == EventType.Repaint)
            {
                // Flag that image is now being shown
                displayedImage = true;
            }
        }

        #region Public members

        /// <summary>
        /// Asynchronously load a new scene.
        /// </summary>
        /// <param name="_sceneToLoad">The name of the scene to load. Scenes must be added in project build settings.</param>
        /// <param name="_loadingTexture">Loading image to display while loading the new scene.</param>
        public static void LoadScene(string _sceneToLoad, Texture2D _loadingTexture)
        {
            // Unity does not provide a way to check if the named scene actually exists in the project.
            GameObject go = new GameObject("SceneLoader");
            DontDestroyOnLoad(go);

            SceneLoader sceneLoader = go.AddComponent<SceneLoader>();
            sceneLoader.sceneToLoad = _sceneToLoad;
            sceneLoader.loadingTexture = _loadingTexture;
        }

        #endregion
    }
}
