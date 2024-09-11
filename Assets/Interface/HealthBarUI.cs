using UnityEngine.UIElements;
using UnityEngine;


public class HealthBarUI : MonoBehaviour {
    public Transform TransformToFollow;

    private VisualElement m_ve;
    private ProgressBar m_progressBar;
    private Camera m_MainCamera;
    private Character character;

    private void Start() {
        m_MainCamera = Camera.main;
        m_ve = GetComponent<UIDocument>().rootVisualElement.Q("Container");
        m_progressBar = m_ve.Q<ProgressBar>();
        character = GetComponentInParent<Character>();

        SetHP();
        SetPosition();
    }

    public void SetPosition() {
        Vector2 newPosition = RuntimePanelUtils.CameraTransformWorldToPanel(
            m_progressBar.panel, TransformToFollow.position, m_MainCamera);

        m_progressBar.transform.position = new Vector3(newPosition.x -m_progressBar.layout.width / 2, newPosition.y, 0);
    }

    public void SetHP() {
        m_progressBar.value = character.HP;
    }

    private void LateUpdate() {
        if (TransformToFollow != null) {
            SetPosition();
            SetHP();
        }
    }
}