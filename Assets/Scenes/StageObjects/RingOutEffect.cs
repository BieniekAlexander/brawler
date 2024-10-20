using UnityEngine;

public class RingOutEffect : Cast
{
    [SerializeField] public int RingOutDamage = 250;
    private Character Effected;

    protected override void OnInitialize() {
        Effected = About.GetComponent<Character>();
        Effected.TakeDamage(Effected.transform.position, RingOutDamage, HitTier.Pure);
        Effected.Velocity = Vector3.zero;

        if (Effected == null) {
            Destroy(gameObject);
        } else {
            Effected.enabled = false;
        }
    }

    protected override void OnDestruction() {
        if (Effected != null) {
            Character c = Effected.GetComponent<Character>();
            GameObject[] spawns = SceneController.GetSceneSpawns();
            GameObject randomSpawn = spawns[Random.Range(0, spawns.Length)];
            c.transform.position = randomSpawn.transform.position;
            c.enabled = true;
        }
    }

    public override bool AppliesTo(GameObject go) => go.CompareTag("Character");
}
