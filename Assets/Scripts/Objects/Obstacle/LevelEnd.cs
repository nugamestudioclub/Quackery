using UnityEngine;
using UnityEngine.SceneManagement;

public class Level : MonoBehaviour
{
    [SerializeField] private string sceneName;
    public void EndLevel()
    {
        // Loads a temporary scene I've made for the proof of concept
        SceneManager.LoadScene(sceneName);
    }

    private void OnTriggerEnter(Collider other)
    {
        // If the object has a player controller, the object must be a player, so end the level
        if (other.GetComponent<PlayerItemController>() != null)
        {
            EndLevel();
        }
    }
}
