using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cosmobot
{
    public class MainMenu : MonoBehaviour
    {
        public void NewGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}
