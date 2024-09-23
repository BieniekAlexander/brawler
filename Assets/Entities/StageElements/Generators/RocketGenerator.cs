using UnityEngine;

public class RocketGenerator : MonoBehaviour, ICasts
{
    [SerializeField] private Projectile rocketPrefab;
    [SerializeField] Transform Target;
    private int Timer;
    private int ReloadTime = 300;
    
    void Start()
    {
        Timer = 0;
    }

    public static int RandomNegative()
    {
        int num = Random.Range(0, 2);
        if (num == 0) return -1;
        else return 1;
    }

    public static Vector3 GetRandomPositionInRing(Transform center, float minDistance, float maxDistance) {
        float distance = Random.Range(minDistance, maxDistance);
        Vector3 randomHorizontalDirection = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up)*Vector3.forward;
        return center.position + distance*randomHorizontalDirection;
    }

    void FixedUpdate()
    {
        if (++Timer==ReloadTime) {
            Timer = 0;
            if (Target != null) {
                transform.position = GetRandomPositionInRing(Target.transform, 30f, 50f);
                Castable c = Castable.CreateCast(
                    rocketPrefab,
                    this,
                    transform,
                    Target,
                    IsRotatingClockwise()
                );
            }
        }
    }

    public Transform GetOriginTransform() {
        return transform;
    }

    public Transform GetTargetTransform() {
        return Target; // TODO return to this - maybe I want the generator to handle its own target
    }

    public bool IsRotatingClockwise() {
        return true; // TODO return to this - make sure that I'm calculating the rotation shits
    }
}

