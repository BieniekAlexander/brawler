using UnityEngine;
using System.Collections.Generic;

public class HazardHandler : MonoBehaviour
{
    [HideInInspector] public Transform Target {  get; set; }
    [SerializeField] List<CastableGenerator> CastableGeneratorsPrefabs;
    private List<CastableGenerator> CastableGenerators = new();

    // Start is called before the first frame update
    public void Initialize(Transform _target)
    {
        Target = _target;

        foreach (CastableGenerator generatorPrefab in CastableGeneratorsPrefabs) {
            CastableGenerator generator = Instantiate(generatorPrefab, transform, false);
            generator.Target = Target;
            CastableGenerators.Add(generator);
        }
    }
}
