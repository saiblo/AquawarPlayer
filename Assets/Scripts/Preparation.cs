using UnityEngine;
using UnityEngine.SceneManagement;

public class Preparation : MonoBehaviour
{
    private void Awake()
    {
        
    }

    public void ConfirmSelection()
    {
        SceneManager.LoadScene("Scenes/Game");
    }
}
