using UnityEngine;

public class RocketGenerator : MonoBehaviour
{
    [SerializeField] private GameObject rocketPrefab;
    [SerializeField] Transform transTarget;
    private float timer;
    private float reloadTime = 5f;
    
    void Start()
    {
        timer = 5f;
    }

    int RandomNegative()
    {
        int num = Random.Range(0, 2);
        if (num == 0) return -1;
        else return 1;
    }

    void FixedUpdate()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            if (transTarget != null)
            {
                float randX = Random.Range(10f, 20f) * RandomNegative();
                float randZ = Random.Range(10f, 20f) * RandomNegative();
                Vector3 initialPosition = new Vector3(randX, transTarget.position.y+.1f, randZ);
                GameObject go = Instantiate(rocketPrefab, initialPosition, Quaternion.Euler(new Vector3(0, 0, 0)));
                RocketBehavior rocket = go.GetComponent<RocketBehavior>();
                rocket.transTarget = transTarget;
            }
            
            timer = reloadTime;
        }
    }
}

