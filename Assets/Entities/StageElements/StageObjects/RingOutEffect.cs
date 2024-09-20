using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingOutEffect : EffectBase
{
    [SerializeField] public int RingOutDamage = 250;
    [SerializeField] public int RingOutDuration = 60*5;

    public override void Initialize(Character _character) {
        target = _character;
        target.TakeDamage(RingOutDamage);
        target.Velocity = Vector3.zero;

        if (target == null) {
            Destroy(gameObject);
        } else {
            target.gameObject.SetActive(false);
        }
    }

    public override void Expire() {
        GameObject[] spawns = SceneController.GetSceneSpawns();
        GameObject randomSpawn = spawns[Random.Range(0, spawns.Length)];
        target.transform.position = randomSpawn.transform.position;
        target.gameObject.SetActive(true);
    }
}
