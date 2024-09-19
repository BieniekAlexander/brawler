using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform TransTarget { get; set; }
    private static Vector3 relativePosition = new Vector3(0, 15.0f, -5.0f);
    private static Quaternion rotation = Quaternion.Euler(-Mathf.Atan(relativePosition.y/relativePosition.z)*Mathf.Rad2Deg, 0, 0);
    private int cameraLockFactor;
    void Start() {
        Camera.main.transform.rotation = rotation;
        cameraLockFactor = 0;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            cameraLockFactor = (cameraLockFactor==0)?1:0;
        }

        if (TransTarget != null)
        {
            float cameraX = (Input.mousePosition.x - (Screen.width / 2)) / Screen.width;
            float cameraZ = (Input.mousePosition.y - (Screen.height / 2)) / Screen.height;
            Vector3 relativeMousePosition = new Vector3(cameraX, 0, cameraZ) * relativePosition.y * 1f * cameraLockFactor;
            transform.position = TransTarget.position + relativePosition + relativeMousePosition;
        }
    }
}
