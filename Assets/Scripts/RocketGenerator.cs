using UnityEngine;

public class RocketGenerator : MonoBehaviour
{
    [SerializeField] private ProjectileBehavior rocketPrefab;
    [SerializeField] Transform target;
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
            if (target != null)
            {
                float randX = Random.Range(10f, 20f) * RandomNegative();
                float randZ = Random.Range(10f, 20f) * RandomNegative();
                Vector3 initialPosition = new Vector3(randX, target.position.y+.1f, randZ);
                Instantiate(rocketPrefab, initialPosition, Quaternion.LookRotation(target.position - initialPosition)).Initialize(null, target);
            }
            
            timer = reloadTime;
        }
    }
}

