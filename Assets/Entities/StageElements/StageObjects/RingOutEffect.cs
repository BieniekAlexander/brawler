using UnityEngine;

public class RingOutEffect : Effect
{
    [SerializeField] public int RingOutDamage = 250;
    [SerializeField] public int RingOutDuration = 60*5;

    private Character Character;

    public override void Initialize(MonoBehaviour _target) {
        base.Initialize(_target);
        
        if (Target.CompareTag("Character")) {
            Character = Target.GetComponent<Character>();

            Character.TakeDamage(Character.transform.position, RingOutDamage, HitTier.Pure);
            Character.Velocity = Vector3.zero;

            if (Character == null) {
                Destroy(gameObject);
            } else {
                Character.gameObject.SetActive(false);
            }
        }
    }

    public override void Expire() {
        if (Character != null) {
            GameObject[] spawns = SceneController.GetSceneSpawns();
            GameObject randomSpawn = spawns[Random.Range(0, spawns.Length)];
            Character.transform.position = randomSpawn.transform.position;
            Character.gameObject.SetActive(true);
        }
    }
}
