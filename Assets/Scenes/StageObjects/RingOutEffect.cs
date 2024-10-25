using UnityEngine;

public class RingOutEffect : Effect
{
    [SerializeField] public int RingOutDamage = 250;
    private Character Effected;

    protected override void OnInitialize() {
        Effected = About.GetComponent<Character>();
        Effected.TakeDamage(Effected.transform.position, RingOutDamage, HitTier.Pure);
        Effected.Velocity = Vector3.zero;
        // TODO this is currently damaging shields - probably strip all effects from the character first (Armor is an effect)

        if (Effected == null) {
            Destroy(gameObject);
        } else {
            Effected.gameObject.SetActive(false);
        }
    }

    protected override void OnDestruction() {
        if (Effected != null) {
            Character c = Effected.GetComponent<Character>();
            GameObject[] spawns = SceneController.GetSceneSpawns();
            GameObject randomSpawn = spawns[Random.Range(0, spawns.Length)];
            c.transform.position = randomSpawn.transform.position;
            Effected.gameObject.SetActive(true);
        }
    }

    override public bool AppliesTo(GameObject go) => go.CompareTag("Character");
}
