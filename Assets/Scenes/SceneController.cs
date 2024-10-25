using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ref: https://youtube.com/watch?v=bdJZeMsKQg8
/// </summary>
public class SceneController : MonoBehaviour
{
    [SerializeField] List<Character> CharacterPrefabs;
    [SerializeField] HazardHandler HazardHandlerPrefab;
    private List<Character> Characters = new();
    private HazardHandler HazardHandler;
    public static SceneController Instance;
    private int _meId = 0;

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
            Char.TeamBitMask = (1<<i);
            Characters.Add(Char);

            if (i == _meId) {
                Char.gameObject.name = "Player";
                Char.SetMe();
                GameObject.Find("Main Camera").GetComponent<CameraMovement>().TransTarget = Char.transform;
            }
        }

        // set up UI, if a canvas is provided
        if (FindAnyObjectByType(typeof(CanvasController)) is CanvasController canvas) {
            canvas.InitializeStatBars(Characters);

            if (HazardHandlerPrefab != null) {
                HazardHandler = Instantiate(HazardHandlerPrefab);
                HazardHandler.Initialize(Characters[0].transform);
            }
        }
    }

    void Update() {
        // switch/reload scene
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
        } else if (Input.GetKeyDown(KeyCode.KeypadMultiply)) {
            Characters[_meId].UnsetMe();
            _meId = MathUtils.mod(_meId+1, Characters.Count);
            Characters[_meId].SetMe();
            GameObject.Find("Main Camera").GetComponent<CameraMovement>().TransTarget = Characters[_meId].transform;
        } else if (Input.GetKey(KeyCode.KeypadPlus)) {
            int charIdx = -1;

            if (Input.GetKeyDown(KeyCode.Keypad0)) {
                charIdx = 0;
            } else if (Input.GetKeyDown(KeyCode.Keypad1)) {
                charIdx = 1;
            }

            if (charIdx>=0) {
                Characters[charIdx].ToggleCollectingControls("characterInput", Input.GetKey(KeyCode.RightShift));
            }
        }
    }
}
