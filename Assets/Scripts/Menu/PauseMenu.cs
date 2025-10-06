using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cosmobot
{
    public class PauseMenu : MonoBehaviour
    {
        public static bool IsGamePaused  ;

        public GameObject pauseMenuUi;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (IsGamePaused)
                {
                    Resume();
                }
                else
                {
                    Pause();
                }
            }
        }

        public void Resume()
        {
            pauseMenuUi.SetActive(false);
            Time.timeScale = 1f;
            IsGamePaused = false;
        }

        private void Pause()
        {
            pauseMenuUi.SetActive(true);
            Time.timeScale = 0f;
            IsGamePaused = true;
        }

        public void BackToMenu()
        {
            SceneManager.LoadScene(0);
        }
    }
}
