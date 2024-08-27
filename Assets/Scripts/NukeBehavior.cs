using UnityEngine;

public class NukeBehavior : MonoBehaviour
{
    [SerializeField] private float durationMax = 3f;
    private float duration;
    float planeScale = 10f; // https://discussions.unity.com/t/size-of-a-plane/415068

    /* Visuals */
    private Material _material;
    public Transform explosionPrefab;

    void Awake()
    {
        duration = durationMax;
        _material = GetComponent<Renderer>().material;
    }

    private void HandleVisuals()
    {
        /*
         * Bolt Indicator
         */
        // duration
        _material.color = new Color((durationMax-duration) /durationMax,0f,0f,1f);
    }

    private void Update() {
        HandleVisuals();
    }

    void FixedUpdate()
    {
        duration -= Time.deltaTime;
        if (duration < 0)
            Destroy(gameObject);
    }

    private void OnDestroy()
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Character");
        Vector2 pos2d = new Vector2(transform.position.x, transform.position.z);

        foreach (GameObject go in gos) {
            Vector2 goPos2d = new Vector2(go.transform.position.x, go.transform.position.z);
            if (Vector2.Distance(pos2d, goPos2d) < transform.localScale.x * planeScale/2)
            {
                Destroy(go);
            }
        }
        
        Transform nuke = Instantiate(
            explosionPrefab,
           transform.position,
           Quaternion.identity
        );
        nuke.localScale = new Vector3(planeScale/2, planeScale/2, planeScale/2);
    }
}
