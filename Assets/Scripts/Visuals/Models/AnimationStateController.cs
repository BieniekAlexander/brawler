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
    int _hitStunnedHash;
    int _pushedBackHash;
    int _knockedBackHash;
    int _blownBackHash;

    // recovery
    int _tumblingHash;
    int _rollingHash;
    int _gettingUpHash;
    int _knockedDownHash;

    // action
    int _readyHash;
    int _shieldingHash;
    int _busyHash;


    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _character = GetComponent<Character>();

        // movement
        _runningHash = Animator.StringToHash("MovementRunning");
        _sprintingHash = Animator.StringToHash("MovementSprinting");

        // disadvantage
        _hitStunnedHash = Animator.StringToHash("DisadvantageHitStunned");
        _pushedBackHash = Animator.StringToHash("DisadvantagePushedBack");
        _knockedBackHash = Animator.StringToHash("DisadvantageKnockedBack");
        _blownBackHash = Animator.StringToHash("DisadvantageBlownBack");

        // recovery
        _tumblingHash = Animator.StringToHash("RecoveryTumbling");
        _rollingHash = Animator.StringToHash("RecoveryRolling");
        _gettingUpHash = Animator.StringToHash("RecoveryGettingUp");
        _knockedDownHash = Animator.StringToHash("RecoveryKnockedDown");

        // action
        _readyHash = Animator.StringToHash("ActionReady");
        _shieldingHash = Animator.StringToHash("ActionShielding");
        _busyHash = Animator.StringToHash("ActionBusy");

    }

    // Update is called once per frame
    void Update()
    {
        // movement
        _animator.SetBool(_runningHash, _character.Velocity.sqrMagnitude > .0025);
        _animator.SetBool(_sprintingHash, _character.Velocity.sqrMagnitude > Mathf.Pow(_character.BaseSpeed+.01f, 2));

        // disadvantage
        _animator.SetBool(_hitStunnedHash, _character.StateIsActive(typeof(CharacterStateHitStopped)));
        _animator.SetBool(_pushedBackHash, _character.StateIsActive(typeof(CharacterStatePushedBack)));
        _animator.SetBool(_knockedBackHash, _character.StateIsActive(typeof(CharacterStateKnockedBack)));
        _animator.SetBool(_blownBackHash, _character.StateIsActive(typeof(CharacterStateBlownBack)));

        // recovery
        _animator.SetBool(_tumblingHash, _character.StateIsActive(typeof(CharacterStateTumbling)));
        _animator.SetBool(_rollingHash, _character.StateIsActive(typeof(CharacterStateRolling)));
        _animator.SetBool(_gettingUpHash, _character.StateIsActive(typeof(CharacterStateGettingUp)));
        _animator.SetBool(_knockedDownHash, _character.StateIsActive(typeof(CharacterStateKnockedDown)));

        // action
        _animator.SetBool(_readyHash, _character.StateIsActive(typeof(CharacterStateReady)));
        _animator.SetBool(_busyHash, _character.StateIsActive(typeof(CharacterStateBusy)));
        _animator.SetBool(_shieldingHash,
            _character.StateIsActive(typeof(CharacterStateBlocking))
            || _character.StateIsActive(typeof(CharacterStateShielding))
        );
    }
}
