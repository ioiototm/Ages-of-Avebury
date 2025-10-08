using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void LoadNextScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;

        LocationRandomiser.Instance.SetAllLocationsToEnabled();

        SceneManager.LoadScene(0);
    }
}
