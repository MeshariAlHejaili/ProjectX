using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[AddComponentMenu("Game/UI/Main Menu Controller")]
    public class MainMenuController : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Exact name of the scene to load when Start is clicked")]
        [SerializeField] private string _gameSceneName = "MainScene";

        [Header("UI References (Optional)")]
        [Tooltip("Disable this button if WebGL (since Quit doesn't work)")]
        [SerializeField] private Button _quitButton;

        private void Start()
        {
            // WebGL platform check example for production polish
            if (Application.platform == RuntimePlatform.WebGLPlayer && _quitButton != null)
            {
                _quitButton.interactable = false;
            }
        }

        /// <summary>
        /// Wired to the Start Button OnClick event.
        /// </summary>
        public void OnStartGameClicked()
        {
            // In a larger architecture, this might call GameManager.Instance.StartGame()
            // For now, direct loading is efficient and clean.
            if (Application.CanStreamedLevelBeLoaded(_gameSceneName))
            {
                SceneManager.LoadSceneAsync(_gameSceneName);
            }
            else
            {
                Debug.LogError($"[MainMenu] Scene '{_gameSceneName}' not found in Build Settings!");
            }
        }

        /// <summary>
        /// Wired to the Exit Button OnClick event.
        /// </summary>
        public void OnExitGameClicked()
        {
            Debug.Log("[MainMenu] Quit Application requested.");
            
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
