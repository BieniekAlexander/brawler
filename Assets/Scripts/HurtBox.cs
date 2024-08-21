using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class HurtBox : MonoBehaviour
{
    [SerializeField] private float damage = 40f;
    [SerializeField] private float knockback = 1f;
    [SerializeField] private float scaleFactor = 10f; // TODO make this an actual 3D object
    private Vector3 position;
    private Quaternion rotation;

    // Visual Effects
    [SerializeField] private Transform explosionPrefab;

    public void Initialize(Vector3 _position, Quaternion _rotation)
    {
        position = _position;
        rotation = _rotation;

        Instantiate(
            explosionPrefab,
            position,
            rotation
        );

        GameObject[] gos = GameObject.FindGameObjectsWithTag("Character");
        Vector3 pos2d = new Vector2(position.x, position.z);

        foreach (GameObject go in gos)
        {
            Vector2 goPos2d = new Vector2(go.transform.position.x, go.transform.position.z);
            if (Vector2.Distance(pos2d, goPos2d) < transform.localScale.x*scaleFactor)
            {
                CharacterBehavior cb = go.GetComponent<CharacterBehavior>();
                cb.TakeDamage(damage);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Destroy(gameObject);
    }
}
