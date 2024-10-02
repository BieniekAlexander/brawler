using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

public class CastPlayer : MonoBehaviour {
    // TODO if the user changes the prefab allocated to the prefab, propagate some kind of change to the CastPlayer
    // https://discussions.unity.com/t/prefabutility-check-if-changes-have-been-made-to-prefab/167857
    [SerializeField] Character Caster;
    [SerializeField] Transform Target;
    [SerializeField] Cast CastPrefab;
    private Dictionary<int, List<Castable>> StartFrameCastablesMap = null;
    private Dictionary<int, List<Castable>> ExpireFrameCastablesMap = null;
    private List<Castable> ActiveCastables = null;
    private Cast cast;
    private int frame;
    private int duration;

    private void Awake() {
        Time.timeScale = 0;
        frame = 0;
        InitializeCast(CastPrefab);
    }

    private void GetActiveFrameCastablesMap(
        Cast prefab,
        out Dictionary<int, List<Castable>> startFrameCastablesMap,
        out Dictionary<int, List<Castable>> expireFrameCastablesMap
    ) {
        // TODO recursive
        startFrameCastablesMap = Enumerable.Range(0, duration).ToDictionary(i => i, i => new List<Castable>());
        expireFrameCastablesMap = Enumerable.Range(0, duration).ToDictionary(i => i, i => new List<Castable>());

        for (int f = 0; f < prefab.duration; f++) {
            // inefficient, but nice if I want to create new castables
            startFrameCastablesMap[f] = new();

            if (prefab.FrameCastablesMap.ContainsKey(f)) {
                for (int i = 0; i <  prefab.FrameCastablesMap[f].Length; i++) {
                    Castable newCastable = Castable.CreateCast(
                        prefab.FrameCastablesMap[f][0],
                        Caster,
                        Caster.transform,
                        Target,
                        !Caster.IsRotatingClockwise()
                    );

                    newCastable.gameObject.SetActive(false);
                    startFrameCastablesMap[f].Add(newCastable);

                    if (f+newCastable.Duration < duration){
                        expireFrameCastablesMap[f+newCastable.Duration].Add(newCastable);
                    } else if (f+newCastable.Duration > duration) {
                        Debug.LogWarning($"Hitbox extends past cast duration\nstart frame: {f}\ncastable duration: {newCastable.Duration}\ncast duration: {duration}");
                    } // NOTE: it's okay if the Castable lasts as long as the Cast, I think!
                }
            }
        }
    }

    private void InitializeCast(Cast CastPrefab) {
        duration = CastPrefab.duration;
        ActiveCastables = new();

        GetActiveFrameCastablesMap(
            CastPrefab,
            out Dictionary<int, List<Castable>> starts,
            out Dictionary<int, List<Castable>> expirations
        );

        StartFrameCastablesMap = starts;
        ExpireFrameCastablesMap = expirations;
        frame = 0;

        // enable any initial casts
        for (int i = 0; i < StartFrameCastablesMap[0].Count; i++) {
            StartFrameCastablesMap[0][i].gameObject.SetActive(true);
            StartFrameCastablesMap[0][i].Frame = -1; // will be incremented
            ActiveCastables.Add(StartFrameCastablesMap[0][i]);
        }
    }

    private void UpdateCastableTransformations() {
        foreach (Castable activeCastable in ActiveCastables) {
            if (activeCastable is Trigger activeTrigger) {
                TriggerTransformation newTransformation = TriggerTransformation.FromTransformCoordinates(activeTrigger.transform, Caster.transform, !Caster.IsRotatingClockwise());

                if (newTransformation != activeTrigger.TriggerTransformations[activeTrigger.Frame]) {
                    Debug.Log("Updating transformation");
                    activeTrigger.TriggerTransformations[activeTrigger.Frame] = newTransformation;
                }
            }
        }
    }

    private void MoveKeyFrame(int direction) {
        Assert.IsTrue(direction==1 || direction==-1);

        UpdateCastableTransformations();
        frame += direction;

        if (direction == 1) { // enable, disable castables as defined per frame
            // playing forward
            for (int i = 0; i < StartFrameCastablesMap[frame].Count; i++) {
                StartFrameCastablesMap[frame][i].gameObject.SetActive(true);
                StartFrameCastablesMap[frame][i].Frame = -1; // will be incremented
                ActiveCastables.Add(StartFrameCastablesMap[frame][i]);
            }

            for (int i = 0; i < ExpireFrameCastablesMap[frame].Count; i++) {
                ExpireFrameCastablesMap[frame][i].gameObject.SetActive(false);
                ActiveCastables.Remove(ExpireFrameCastablesMap[frame][i]);
            }
        } else { // direction == -1 - playing backward
            for (int i = 0; i < ExpireFrameCastablesMap[frame+1].Count; i++) {
                ExpireFrameCastablesMap[frame+1][i].gameObject.SetActive(true);
                ExpireFrameCastablesMap[frame+1][i].Frame = ExpireFrameCastablesMap[frame+1][i].Duration;
                ActiveCastables.Add(ExpireFrameCastablesMap[frame+1][i]);
            }

            for (int i = 0; i < StartFrameCastablesMap[frame+1].Count; i++) {
                StartFrameCastablesMap[frame+1][i].gameObject.SetActive(false);
                ActiveCastables.Remove(StartFrameCastablesMap[frame+1][i]);
            }
        }

        foreach (Castable castable in ActiveCastables) {
            if (castable is Trigger trigger) {
                trigger.Frame += direction;
                trigger.UpdateTransform(trigger.Frame);
            } else {
                // TODO what will I do for projectiles and such?
                Debug.Log($"Castable type is unhandled for this editor: {castable}");
            }
        }
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.PageDown) && frame<(duration-1)) {
            MoveKeyFrame(1);
        } else if (Input.GetKeyDown(KeyCode.PageUp) && frame>0) {
            MoveKeyFrame(-1);
        }

        // TODO what if multiple frames have the same castable?
        // presumably, the update is applied to the given prefab and all active instances in the editor

        // TODO save updates to the Cast prefab, as well as updates to its castables
        if (Input.GetKeyDown(KeyCode.Backspace)) { // TODO find better button
            UpdateCastableTransformations();

            for (int i = 0; i < duration; i++) {
                if (!CastPrefab.FrameCastablesMap.ContainsKey(i))
                    continue;

                // https://discussions.unity.com/t/test-if-prefab-is-an-instance/716592/3
                List<Castable> startFrameCastables = StartFrameCastablesMap[i];
                Castable[] prefabMap =  CastPrefab.FrameCastablesMap[i];

                for (int j = 0; j < prefabMap.Length; j++) {
                    Castable toCopy = startFrameCastables[j];
                    Castable copyToPrefab = prefabMap[j];

                    Assert.IsTrue(toCopy.Duration==copyToPrefab.Duration); // TODO not sure how to check that these refer to the same hit

                    if (toCopy is Trigger toCopyTrigger && copyToPrefab is Trigger copyToPrefabTrigger) {
                        toCopyTrigger.UpdatePrefabTransformations(copyToPrefabTrigger);
                        Destroy(toCopyTrigger.gameObject);
                        Debug.Log("saving changes");
                    }
                }
            }

            InitializeCast(CastPrefab);
        }
        
    }

    void OnGUI() {
        // TODO remove: here for debugging
        GUI.Label(new Rect(20, 40, 200, 20), $"Frame {frame}/{duration-1} (total {duration})");
    }
}
