using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// https://discussions.unity.com/t/how-to-convert-from-world-space-to-canvas-space/117981/3
/// </summary>
public class Canvas : MonoBehaviour {
    [SerializeField] List<HPBarVisuals> HealthBars = new List<HPBarVisuals>();
    [SerializeField] Transform cameraPosition;
    
    private RectTransform CanvasRect;
    private Camera Cam;
    private List<RectTransform> hpBarTransforms = new List<RectTransform>();
    [SerializeField] Vector3 offset = new Vector3(0, 1, .5f);

    private void Awake() {
        CanvasRect = GetComponent<RectTransform>();
        Cam = Camera.main;

        foreach (HPBarVisuals healthBar in HealthBars) {
            hpBarTransforms.Add(healthBar.GetComponent<RectTransform>());
        }
    }

    // Update is called once per frame
    void FixedUpdate() {
        HealthBars.RemoveAll(bar => bar==null);

        for (int i = 0; i < HealthBars.Count; i++) {
            Vector2 ViewportPosition = Cam.WorldToViewportPoint(HealthBars[i].character.transform.position+offset);
            Vector2 WorldObject_ScreenPosition = new Vector2(
                (ViewportPosition.x*CanvasRect.sizeDelta.x)-(CanvasRect.sizeDelta.x*0.5f),
                (ViewportPosition.y*CanvasRect.sizeDelta.y)-(CanvasRect.sizeDelta.y*0.5f)
            );

            hpBarTransforms[i].anchoredPosition=WorldObject_ScreenPosition;
        }
    }
}
