using UnityEngine;

public class AnimationStateController : MonoBehaviour
{
    Animator _animator;
    Character _character;

    // movement
    int _runningHash;
    int _sprintingHash;
    // TODO maybe some jumping stuff

    // disadvantage
    int _knockedBackHash;
    int _blownBackHash;

    // recovery
    int _rollingHash;
    int _gettingUpHash;
    int _getUpAttackingHash;

    // action
    int _attackingHash;
    int _grabbingHash;
    int _castingHash;


    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _character = GetComponent<Character>();

        _runningHash = Animator.StringToHash("Running");
        _sprintingHash = Animator.StringToHash("Sprinting");
        _knockedBackHash = Animator.StringToHash("KnockedBack");
        _blownBackHash = Animator.StringToHash("BlownBack");
        _rollingHash = Animator.StringToHash("Rolling");
        _gettingUpHash = Animator.StringToHash("GettingUp");
        _getUpAttackingHash = Animator.StringToHash("GetUpAttacking");
        _attackingHash = Animator.StringToHash("Attacking");
        _grabbingHash = Animator.StringToHash("Grabbing");
        _castingHash = Animator.StringToHash("Casting");

    }

    // Update is called once per frame
    void Update()
    {
        _animator.SetBool(_runningHash, _character.Velocity.sqrMagnitude > 1);
        _animator.SetBool(_sprintingHash, _character.Velocity.sqrMagnitude > Mathf.Pow(_character.BaseSpeed+.5f, 2));
    }
}
