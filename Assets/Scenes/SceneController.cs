using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ref: https://youtube.com/watch?v=bdJZeMsKQg8
/// </summary>
public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    string scene1 = "scene1";
    string scene2 = "scene2";

    // Start is called before the first frame update
    void Awake()   {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    void Update() {
        if (Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.RightAlt)) {
            if (Input.GetKey(KeyCode.R)) {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            } else if (Input.GetKey(KeyCode.Keypad0)) {
                SceneManager.LoadScene(scene1);
            } else if (Input.GetKey(KeyCode.Keypad1)) {
                SceneManager.LoadScene(scene2);
            }
        }
    }
}
