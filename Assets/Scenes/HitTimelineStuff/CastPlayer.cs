using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public enum CastEditorControls {
    StepBackward = 0,
    StepForward = 1,
    CopyOnStep = 2,
    Mirror = 3,
    NextCast = 4,
    PreviousCast = 5,
    Save = 6
}

public class CastPlayer : MonoBehaviour {
    // TODO if the user changes the prefab allocated to the prefab, propagate some kind of change to the CastPlayer
    // https://discussions.unity.com/t/prefabutility-check-if-changes-have-been-made-to-prefab/167857
    [SerializeField] Character Caster;
    [SerializeField] Transform Target;
    private Cast CastPrefab;
    private Dictionary<int, List<Cast>> StartFrameCastablesMap = null;
    private Dictionary<int, List<Cast>> ExpireFrameCastablesMap = null;
    private List<Cast> ActiveCastables = null;
    private Cast castable;
    private bool _rotatingClockwise = true; // let's assume that all editing assumes a cast direction of clockwise
    private int editorIndex;
    private int frame;
    private int duration;

    private int[] castIds = new int[] {
        (int) CastId.Light1,
        (int) CastId.Light2,
        (int) CastId.LightS,
        (int) CastId.Medium1,
        (int) CastId.Medium2,
        (int) CastId.MediumS,
        (int) CastId.Heavy1,
        (int) CastId.Heavy2,
        (int) CastId.HeavyS
    };

    private Dictionary<CastEditorControls, KeyCode[]> EditorControlMapping = new Dictionary<CastEditorControls, KeyCode[]>(){
        {CastEditorControls.StepBackward, new KeyCode[]{KeyCode.Z } },
        {CastEditorControls.StepForward, new KeyCode[]{KeyCode.X } },
        {CastEditorControls.CopyOnStep, new KeyCode[]{KeyCode.LeftShift, KeyCode.RightShift} },
        {CastEditorControls.Mirror, new KeyCode[]{KeyCode.Space} },
        {CastEditorControls.Save, new KeyCode[]{KeyCode.Backspace} }, // TODO find better button
        {CastEditorControls.NextCast, new KeyCode[]{ KeyCode.PageDown } }, // TODO find better button
        {CastEditorControls.PreviousCast, new KeyCode[]{ KeyCode.PageUp } }, // TODO find better button
    };

    private bool CheckControl(CastEditorControls control, Func<KeyCode, bool> checker) {
        for (int i = 0; i < EditorControlMapping[control].Length; i++) {
            if (checker(EditorControlMapping[control][i])) {
                return true;
            }
        }

        return false;
    }

    private void Awake() {
        Time.timeScale = 0;
        frame = 0;
        editorIndex = 0;
        InitializeCast(castIds[editorIndex]);
    }

    // returns duration, because hitboxes might last longer than the base Castable
    private int GetActiveFrameCastablesMap(
        Cast prefab,
        out Dictionary<int, List<Cast>> startFrameCastablesMap,
        out Dictionary<int, List<Cast>> expireFrameCastablesMap
    ) {
        int finalDuration = prefab.Duration;

        foreach (int frame in prefab.FrameCastablesMap.Keys) {
            Cast[] casts = prefab.FrameCastablesMap[frame];
            foreach (Cast cast in casts) {
                finalDuration = Math.Max(finalDuration, frame+cast.Duration);
            }
        }

        // TODO recursive
        startFrameCastablesMap = Enumerable.Range(0, finalDuration).ToDictionary(i => i, i => new List<Cast>());
        expireFrameCastablesMap = Enumerable.Range(0, finalDuration).ToDictionary(i => i, i => new List<Cast>());

        for (int f = 0; f < finalDuration; f++) {
            // inefficient, but nice if I want to create new castables
            if (prefab.FrameCastablesMap.ContainsKey(f)) {
                for (int i = 0; i <  prefab.FrameCastablesMap[f].Length; i++) {
                    Cast newCastable = Cast.Initiate(
                        prefab.FrameCastablesMap[f][i],
                        Caster,
                        Caster.transform,
                        Target,
                        !_rotatingClockwise,
                        null
                    );

                    newCastable.gameObject.SetActive(false);
                    startFrameCastablesMap[f].Add(newCastable);

                    if (f+newCastable.Duration < finalDuration) {
                        expireFrameCastablesMap[f+newCastable.Duration].Add(newCastable);
                    }
                }
            }
        }

        return finalDuration;
    }

    private void InitializeCast(int castId) {
        DeleteInstantiatedCasts();

        CastPrefab = Caster.CastContainers[castId].CastSlot.CastPrefab;
        ActiveCastables = new();
        Debug.Log(CastPrefab.name);

        duration = GetActiveFrameCastablesMap(
            CastPrefab,
            out Dictionary<int, List<Cast>> starts,
            out Dictionary<int, List<Cast>> expirations
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
        foreach (Cast activeCastable in ActiveCastables) {
            if (activeCastable is Trigger activeTrigger) {
                TriggerTransformation newTransformation = TriggerTransformation.FromTransformCoordinates(activeTrigger.transform, Caster.transform, !_rotatingClockwise);

                if (newTransformation != activeTrigger.TriggerTransformations[activeTrigger.Frame]) {
                    activeTrigger.TriggerTransformations[activeTrigger.Frame] = newTransformation;
                }
            }
        }
    }

    private void MoveKeyFrame(int direction, bool copyAdjacentFrame) {
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

        foreach (Cast castable in ActiveCastables) {
            if (castable is Trigger trigger) {
                // NOTE - this will apply to all active casts - I don't have a means of selecting hits for copying, yet
                trigger.Frame += direction;

                if (copyAdjacentFrame
                    && (
                        (direction>0 && trigger.Frame!=0) // time stepping forward, and frame didn't just start
                        || (direction<0 && trigger.Frame!=trigger.Duration-1) // time stepping backwards, and the frame will not have expired
                    )
                ) {
                    // copy the frame we just stepped from to this frame
                    trigger.TriggerTransformations[trigger.Frame] = trigger.TriggerTransformations[trigger.Frame-direction].GetHardCopy();
                }

                trigger.UpdateTransform(trigger.Frame);
            } else {
                // TODO what will I do for projectiles and such?
                Debug.Log($"Castable type is unhandled for this editor: {castable}");
            }
        }
    }

    private void Update() {
        // TODO what if multiple frames have the same castable?
        // presumably, the update is applied to the given prefab and all active instances in the editor
        // TODO my eyes are BLEEDING right now - I need to organize this code better
        if (CheckControl(CastEditorControls.StepForward, Input.GetKeyDown) && frame<(duration-1)) {
            MoveKeyFrame(1, CheckControl(CastEditorControls.CopyOnStep, Input.GetKey));
        } else if (CheckControl(CastEditorControls.StepBackward, Input.GetKeyDown) && frame>0) {
            MoveKeyFrame(-1, CheckControl(CastEditorControls.CopyOnStep, Input.GetKey));
        } else if (CheckControl(CastEditorControls.Mirror, Input.GetKeyDown)) {
            MirrorFrame();
        } else if (CheckControl(CastEditorControls.Save, Input.GetKeyDown)) {
            SaveCurrentCast();
            InitializeCast(castIds[editorIndex]);
        } else if (CheckControl(CastEditorControls.NextCast, Input.GetKeyDown)) {
            editorIndex = MathUtils.mod(editorIndex+1, castIds.Length);
            InitializeCast(castIds[editorIndex]);
        } else if (CheckControl(CastEditorControls.PreviousCast, Input.GetKeyDown)) {
            editorIndex = MathUtils.mod(editorIndex-1, castIds.Length);
            InitializeCast(castIds[editorIndex]);
        }
    }

    private void MirrorFrame() {
        foreach (Cast activeCastable in ActiveCastables) {
            if (activeCastable is Trigger activeTrigger) {
                activeTrigger.TriggerTransformations[activeTrigger.Frame].Mirror();
                activeTrigger.UpdateTransform(activeTrigger.Frame);
            }
        }
    }

    private void SaveCurrentCast() {
        UpdateCastableTransformations();

        for (int i = 0; i < duration; i++) {
            if (!CastPrefab.FrameCastablesMap.ContainsKey(i))
                continue;

            // https://discussions.unity.com/t/test-if-prefab-is-an-instance/716592/3
            List<Cast> startFrameCastables = StartFrameCastablesMap[i];
            Cast[] prefabMap = CastPrefab.FrameCastablesMap[i];

            for (int j = 0; j < prefabMap.Length; j++) {
                Cast toCopy = startFrameCastables[j];
                Cast copyToPrefab = prefabMap[j];

                Assert.IsTrue(toCopy.Duration==copyToPrefab.Duration); // TODO not sure how to check that these refer to the same hit

                if (toCopy is Trigger toCopyTrigger && copyToPrefab is Trigger copyToPrefabTrigger) {
                    toCopyTrigger.UpdatePrefabTransformations(copyToPrefabTrigger);
                    UnityEditor.EditorUtility.SetDirty(copyToPrefabTrigger);
                    Destroy(toCopyTrigger.gameObject);
                    Debug.Log("saving changes");
                }
            }
        }
    }

    private void DeleteInstantiatedCasts() {
        for (int i = 0; i < duration; i++) {
            if (!CastPrefab.FrameCastablesMap.ContainsKey(i))
                continue;

            List<Cast> startFrameCastables = StartFrameCastablesMap[i];

            for (int j = 0; j < startFrameCastables.Count; j++) {
                Cast instantiatedCastable = startFrameCastables[j];
                Destroy(instantiatedCastable.gameObject);
            }
        }
    }

    void OnGUI() {
        // TODO remove: here for debugging
        GUI.Label(new Rect(20, 40, 200, 20), $"Cast: {(CastId) castIds[editorIndex]}");
        GUI.Label(new Rect(20, 70, 200, 20), $"Frame {frame}/{duration-1} (total {duration})");
    }
}
