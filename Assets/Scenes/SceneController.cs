using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ref: https://youtube.com/watch?v=bdJZeMsKQg8
/// </summary>
public class SceneController : MonoBehaviour
{
    [SerializeField] List<Character> CharacterPrefabs;
    public static SceneController Instance;

    string scene0 = "TempleSummit";
    string scene1 = "Forsaken";
    private string activeScene;

    public static GameObject[] GetSceneSpawns() {
        return GameObject.FindGameObjectsWithTag("SpawnPoint"); // "GetComponentsInChildren" name misleading - will return parent too
    }


    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeScene();
        } else {
            Destroy(gameObject);
            InitializeScene();
        }
    }

    void InitializeScene() {
        GameObject[] spawnGameObjects = GetSceneSpawns();

        for (int i = 0; i < CharacterPrefabs.Count; i++) {
            Character Char = Instantiate(CharacterPrefabs[i], spawnGameObjects[i].transform.position, Quaternion.identity);
            if (i == 0) {
                Char.gameObject.name = "Player";
                Char.SetMe();
                GameObject.Find("Main Camera").GetComponent<CameraMovement>().TransTarget = Char.transform;
            }
        }
    }

    void Update() {
        if (Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.RightAlt)) {
            if (Input.GetKey(KeyCode.R)) {
                activeScene = scene0;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                InitializeScene();
                Debug.Log($"[{this.name}] Reloading scene");
            } else if (Input.GetKey(KeyCode.Keypad0)) {
                SceneManager.LoadScene(scene0);
                InitializeScene();
                Debug.Log($"[{this.name}] Loading {scene0}");
            } else if (Input.GetKey(KeyCode.Keypad1)) {
                SceneManager.LoadScene(scene1);
                InitializeScene();
                Debug.Log($"[{this.name}] Loading {scene1}");
            }
        }
    }
}
