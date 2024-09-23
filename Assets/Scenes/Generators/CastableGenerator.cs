using UnityEngine;

public class CastableGenerator : MonoBehaviour, ICasts
{
    [SerializeField] private Castable CastablePrefab;

    /* Timing */
    [SerializeField] public int ReloadTime = 300;
    [Tooltip("A number of frames for which the cast reload time can jiggle")]
    [SerializeField] public int TimeJiggle = 50;
    private int Timer;

    /* Targeting */
    [SerializeField] public Transform Target;
    [SerializeField] public float DistanceFromTargetMin = 20f;
    [SerializeField] public float DistanceFromTargetMax = 30f;
    [Tooltip("A maximum number of degrees away from which the generator will cast Castables at the target")]
    [SerializeField] public float TargetJiggle = 30f;


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
        if (++Timer>=ReloadTime) {
            Timer = Random.Range(0, TimeJiggle);
            transform.position = GetRandomPositionInRing(Target, DistanceFromTargetMin, DistanceFromTargetMax);
            transform.LookAt(Target);
            transform.rotation = Quaternion.AngleAxis(
                RandomNegative() * Random.Range(0, TargetJiggle), Vector3.up
                ) * transform.rotation;
            if (Target != null) {
                Castable c = Castable.CreateCast(
                    CastablePrefab,
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

