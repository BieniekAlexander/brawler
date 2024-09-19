using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

public class SceneInitializer : MonoBehaviour {
    [SerializeField] List<Character> CharacterPrefabs;

    // Start is called before the first frame update
    void Awake() {
        GameObject[] spawnGameObjects = GameObject.FindGameObjectsWithTag("SpawnPoint"); // "GetComponentsInChildren" name misleading - will return parent too

        for (int i = 0; i < CharacterPrefabs.Count; i++) {
            Character Char = Instantiate(CharacterPrefabs[i], spawnGameObjects[i].transform.position, Quaternion.identity);
            if (i == 0) {
                Char.SetMe();
                GameObject.Find("Main Camera").GetComponent<CameraMovement>().TransTarget = Char.transform;
            }
        }
    }
}
