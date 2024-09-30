using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CastablePlayer : MonoBehaviour
{
    [SerializeField] Character Caster;
    [SerializeField] Transform Target;
    [SerializeField] Castable Castable;
    private Castable casted;
    private int f = 0;

    private void Awake() {
        Time.timeScale = 0;
        // WIP

        casted = Castable.CreateCast(
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
        if (casted is Hit trigger){
            Debug.Log("trying to update");
            trigger.UpdateTransform(_frame);
        }
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.PageDown) && f<(Castable.Duration-2)) {
            f++;
            Debug.Log($"--> frame {f}");
            SetKeyFrame(f);
        } else if (Input.GetKeyDown(KeyCode.PageUp) && f>0) {
            f--;
            Debug.Log($"<-- frame {f}");
            SetKeyFrame(f);
        }

        if (Input.GetKeyDown(KeyCode.Backspace)) { // TODO find better button
            if (casted is Trigger t) {
                // TODO apply changes to each frame
                t.UpdatePrefab(f, Castable as Trigger);
                Debug.Log("saving changes");
            }
        }
    }

}
