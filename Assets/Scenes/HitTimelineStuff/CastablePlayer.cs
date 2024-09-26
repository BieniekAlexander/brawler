using UnityEngine;
using UnityEngine.TextCore.Text;

public class CastablePlayer : MonoBehaviour
{
    [SerializeField] Character Caster;
    [SerializeField] Transform Target;
    [SerializeField] Castable Castable;
    public int f;

    private void Awake() {
        // WIP

        Castable.CreateCast(
            Castable,
            Caster,
            Caster.GetOriginTransform(),
            Target,
            false
        );
    }

    // TODO this might become relevant if I'm able to simulate timesteps
    // for things without simple HandleTransform physics
    /*private void setKeyFrameRecursive(int _frame, Castable _castable) {
        for (Castable child in _castable.)
    }*/

    private void SetKeyFrame(int _frame) {
        if (Castable is Trigger trigger)
            trigger.UpdateTransform(_frame);
    }
}
