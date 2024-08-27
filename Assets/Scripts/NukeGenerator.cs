using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NukeGenerator : MonoBehaviour
{
    [SerializeField] private GameObject nukePrefab;
    private float timer;
    private float reloadTime = 1f;
    
    void Start()
    {
        timer = reloadTime;
    }

    void FixedUpdate()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            float randX = Random.Range(-20f, 20f);
            float randZ = Random.Range(-20f, 20f);
            float y = 4.1f; // TODO what is this supposed to be?
            Vector3 initialPosition = new Vector3(randX, y, randZ);
            GameObject go = Instantiate(nukePrefab, initialPosition, Quaternion.Euler(new Vector3(0, 0, 0)));
            timer = reloadTime;
        }
    }
}
