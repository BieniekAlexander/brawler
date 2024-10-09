using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterBehavior : MonoBehaviour
{
    private Character Character;
    private int WhackCounter;
    [SerializeField] int WhackTimer = 20;
    [SerializeField] CastId CastId = CastId.Light2;

    // Start is called before the first frame update
    void Start()
    {
        Character = GetComponent<Character>();
        WhackCounter = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (++WhackCounter == WhackTimer) {
            Character.InputCastId = (int) CastId;
            WhackCounter = 0;
        } else {
            Character.InputCastId = - 1;
        }
    }
}
