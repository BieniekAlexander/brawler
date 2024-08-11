using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    [SerializeField] Transform transTarget;
    private static Vector3 relativePosition = new Vector3(0, 9.0f, -3.0f);
    private static Quaternion rotation = Quaternion.Euler(-Mathf.Atan(relativePosition.y/relativePosition.z)*Mathf.Rad2Deg, 0, 0);
    private int cameraLockFactor;
    void Start() {
        Camera.main.transform.rotation = rotation;
        cameraLockFactor = 0;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            cameraLockFactor = (cameraLockFactor==0)?1:0;
        }

        if (transTarget != null)
        {
            float cameraX = (Input.mousePosition.x - (Screen.width / 2)) / Screen.width;
            float cameraZ = (Input.mousePosition.y - (Screen.height / 2)) / Screen.height;
            Vector3 relativeMousePosition = new Vector3(cameraX, 0, cameraZ) * relativePosition.y * 1f * cameraLockFactor;
            transform.position = transTarget.position + relativePosition + relativeMousePosition;
        }
    }
}
