using UnityEngine;

public class RespawnDelayEffect : Effect
{
    private Character Effected;

    protected override void OnInitialize() {
        Effected = About.GetComponent<Character>();
        Effected.Velocity = Vector3.zero;
        Effected.gameObject.SetActive(false);
    }

    protected override void OnDestruction() {
        if (Effected != null) {
            Character c = Effected.GetComponent<Character>();
            GameObject[] spawns = SceneController.GetSceneSpawns();
            GameObject randomSpawn = spawns[Random.Range(0, spawns.Length)];
            c.transform.position = randomSpawn.transform.position;
            Effected.gameObject.SetActive(true);
            Effected.OnRespawn();
        }
    }

    override public bool AppliesTo(GameObject go) => go.CompareTag("Character");
}
