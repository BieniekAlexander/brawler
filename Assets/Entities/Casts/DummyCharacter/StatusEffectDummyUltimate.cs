using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class StatusEffectDummyUltimate: StatusEffectBase {
    [SerializeField] int damage;
    [SerializeField] StatusEffectBase slowPrefab;
    private int buffAbilityCounter = 5;

    public override void Initialize(Character _character) {
        base.Initialize(_character);
        target.castContainers[(int)CastId.Ability4].castPrefab.hitEvents[0].hit.effects.Add(slowPrefab);
    }
    public override void Expire() {
        // TODO this is how I can think to get another ability to temporarily behave differently,
        // but it seems unsafe to modify the prefab because the properties will stay changed if not cleaned up on ending the scene
        target.castContainers[(int)CastId.Ability4].castPrefab.hitEvents[0].hit.effects.Remove(slowPrefab);
    }

    public override void OnCast(CastId castId) {
        if (castId == CastId.Ability4) {
            buffAbilityCounter--;
        }

        if (buffAbilityCounter == 0) {
            Destroy(this);
        }
    }
}
