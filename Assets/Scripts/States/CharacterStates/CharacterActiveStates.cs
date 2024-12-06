using UnityEngine;
using System;

public class CharacterStateStanding : CharacterState {
	public override CharacterStateType Type {get {return CharacterStateType.ACTIVE; }}

	public CharacterStateStanding(Character _machine, CharacterStateFactory _factory): base(_machine, _factory) {}

	protected override Type GetNewStateType() {
		if (Character.InputDash) {
			return typeof(CharacterStateDashing);
		} else if (Character.Busy) {
			return typeof(CharacterStateBusy);
		} else if (Character.InputBlocking && !Character.Parried) {
			return typeof(CharacterStateBlocking);
		} else if (Character.CommandMovement != null) {
			return typeof(CharacterStateCommandMovement);
		} else if (Character.InputJump) {
			return typeof(CharacterStateSquatting);
		} else  {
			return null;
		}
	}
	
	protected override void Tick() {
		if (Character.Velocity.magnitude > Character.BaseSpeed) {
			Character.Velocity = MovementUtils.ChangeMagnitude(
				Vector3.RotateTowards(
					Character.Velocity,
					Character.MoveDirection,
					12.25f*Mathf.Deg2Rad,
					0f
				),
				-2.5f*Character.Friction
			);
		} else {
			float strafeSpeedMult = MovementUtils.StrafeSpeedMultiplier(
				Character.MoveDirection.normalized,
				Character.InputAimVector.normalized
			);

			Character.Velocity = strafeSpeedMult * Character.MoveDirection * Character.BaseSpeed;
		}
	}

	public override bool OnCollideWith(ICollidable collidable, CollisionInfo info) {
		if (collidable is StageTerrain && info!=null) {
			Character.Velocity = MovementUtils.GetBounce(Character.Velocity, info.Normal);
			return true;
		} else if (collidable is Character otherCharacter) {
			Vector3 decollisionVector = CollisionUtils.GetDecollisionVector(Character, otherCharacter);
			Character.Transform.position += MovementUtils.inXZ(decollisionVector);
			return true;
		} else {
			return false;
		}
	}
}

public class CharacterStateSliding : CharacterState {
	public override CharacterStateType Type {get {return CharacterStateType.ACTIVE; }}
	private const float crouchSpeedMult = .6f;
	private float crouchSpeed;

	public CharacterStateSliding(Character _machine, CharacterStateFactory _factory): base(_machine, _factory) {
		crouchSpeed = _machine.BaseSpeed * crouchSpeedMult;
	}

	protected override Type GetNewStateType() {
		if (!Character.InputDash) {
			return typeof(CharacterStateStanding);
		} else {
			return null;
		}
	}
	
	protected override void Tick() {
		Character.Velocity = MovementUtils.ChangeMagnitude(
			Vector3.RotateTowards(
				Character.Velocity,
				Character.MoveDirection,
				12.25f*Mathf.Deg2Rad,
				0f
			),
			-.005f
		);
	}

	public override bool OnCollideWith(ICollidable collidable, CollisionInfo info) {
		if (collidable is Character otherCharacter) {
			Vector3 decollisionVector = CollisionUtils.GetDecollisionVector(Character, otherCharacter);
			Character.Transform.position += MovementUtils.inXZ(decollisionVector);
			return true;
		} else {
			return false;
		}
	}
}

public class CharacterStateBusy : CharacterState {
	public override CharacterStateType Type {get {return CharacterStateType.ACTIVE; }}

	public CharacterStateBusy(Character _machine, CharacterStateFactory _factory): base(_machine, _factory) {}

    protected override void Tick()
    {
		Character.Velocity = MovementUtils.ChangeMagnitude(
			Character.Velocity,
			-2.5f*Character.Friction
		);
    }

    protected override Type GetNewStateType() {
		// TODO make sure that you can't shield if you're dashing, in disadvantage, etc.
		if (!Character.Busy) {
			return typeof(CharacterStateStanding);
		} else {
			return null;
		}
	}
}

public class CharacterStateDashing : CharacterState {
	public override CharacterStateType Type {get {return CharacterStateType.ACTIVE; }}
	private readonly int _duration = 15;
	private readonly float[] velocityCurve = new float[] {
		.01f,	.01f,	.01f,	.01f,	.01f,
		.2f,  	.3f, 	.3f,  	.3f,	.3f,
		.3f,  	.3f, 	.3f,  	.3f,	.3f,
		.125f
	};

	private int _frame;
	public CharacterStateDashing(Character _machine, CharacterStateFactory _factory): base(_machine, _factory) {}

	protected override Type GetNewStateType() {
		if (_frame>=_duration) {
			return typeof(CharacterStateStanding);
		} else if (_frame>=_duration-5) {
			if (Character.InputJump) {
				return typeof(CharacterStateSquatting);
			} else if (Character.InputDash) {
				return typeof(CharacterStateSliding);
			} else {
				return null;
			}
		} else {
			return null;
		}
	}

	protected override void EnterState() {
		Character.Velocity = Character.InputAimVector.normalized*velocityCurve[0];
		_frame = 0;
	}

	protected override void Tick() => Character.Velocity = Character.Velocity.normalized*velocityCurve[++_frame];
	
	public override bool OnCollideWith(ICollidable collidable, CollisionInfo info) {
		if (collidable is StageTerrain terrain && info!=null) {
			Character.Velocity = MovementUtils.GetBounce(Character.Velocity, info.Normal);
			return true;
		} else if (collidable is Character otherCharacter) {
			Vector3 decollisionVector = CollisionUtils.GetDecollisionVector(Character, otherCharacter);
			Character.Transform.position += MovementUtils.inXZ(decollisionVector);
			return true;
		} else {
			return false;
		}
	}
}

public class CharacterStateSquatting : CharacterState {
	public override CharacterStateType Type {get {return CharacterStateType.ACTIVE; }}
	private readonly int duration = 6;
	private int _frame;

	public CharacterStateSquatting(Character _machine, CharacterStateFactory _factory): base(_machine, _factory) {}

	protected override Type GetNewStateType() {
		if (Character.InputDash) {
			return typeof(CharacterStateDashing);
		} else if (_frame>= duration) {
			Character.Velocity += Vector3.up * .2f;
			return typeof(CharacterStateAerial);
		} else {
			return null;
		}
	}

	protected override void EnterState() {
		_frame = 0;
		Character.hasAirDash = true;
	}

	protected override void Tick() => _frame++;

	public override bool OnCollideWith(ICollidable collidable, CollisionInfo info) {
		if (collidable is StageTerrain terrain && info!=null) {
			Character.Velocity = MovementUtils.GetBounce(Character.Velocity, info.Normal);
			return true;
		} else if (collidable is Character otherCharacter) {
			Vector3 decollisionVector = CollisionUtils.GetDecollisionVector(Character, otherCharacter);
			Character.Transform.position += MovementUtils.inXZ(decollisionVector);
			return true;
		} else {
			return false;
		}
	}
}

public class CharacterStateAirDashing : CharacterState {
	public override CharacterStateType Type {get {return CharacterStateType.ACTIVE; }}
	private readonly int duration = 10;
	private int _frame;

	public CharacterStateAirDashing(Character _machine, CharacterStateFactory _factory): base(_machine, _factory) {}

	protected override Type GetNewStateType() {
		if (_frame>= duration) {
			return typeof(CharacterStateAerial);
		} else {
			return null;
		}
	}

	protected override void EnterState() {
		_frame = 0;
		Character.hasAirDash = false;
	}

	protected override void Tick() {
		Character.Velocity = Character.InputAimVector.normalized*.3f; // TODO values
		_frame++;
	}

	public override bool OnCollideWith(ICollidable collidable, CollisionInfo info) {
		if (collidable is StageTerrain terrain && info!=null) {
			Character.Velocity = MovementUtils.GetBounce(Character.Velocity, info.Normal);
			return true;
		} else if (collidable is Character otherCharacter) {
			Vector3 decollisionVector = CollisionUtils.GetDecollisionVector(Character, otherCharacter);
			Character.Transform.position += MovementUtils.inXZ(decollisionVector);
			return true;
		} else {
			return false;
		}
	}
}

public class CharacterStateAerial : CharacterState {
	public override CharacterStateType Type {get {return CharacterStateType.ACTIVE; }}

	public CharacterStateAerial(Character _machine, CharacterStateFactory _factory) : base(_machine, _factory) {}

	protected override Type GetNewStateType() {
		if (Character.InputDash && Character.hasAirDash) {
			return typeof(CharacterStateAirDashing);
		} else if (Character.IsGrounded()){
			if (Character.InputDash) {
				return typeof(CharacterStateSliding);
			} else {
				return typeof(CharacterStateStanding);
			}
		} else {
			return null;
		}
	}

	public override bool OnCollideWith(ICollidable collidable, CollisionInfo info) {
		if (collidable is StageTerrain terrain && info!=null) {
			Character.Velocity = MovementUtils.GetBounce(Character.Velocity, info.Normal);
			return true;
		} else if (collidable is Character otherCharacter) {
			Vector3 decollisionVector = CollisionUtils.GetDecollisionVector(Character, otherCharacter);
			Character.Transform.position += MovementUtils.inXZ(decollisionVector);
			return true;
		} else {
			return false;
		}
	}
}

public class CharacterStateCommandMovement : CharacterState {
	public override CharacterStateType Type {get {return CharacterStateType.ACTIVE; }}

	public CharacterStateCommandMovement(Character _machine, CharacterStateFactory _factory): base(_machine, _factory) {}

	protected override Type GetNewStateType() {
		if (Character.CommandMovement == null) {
			return typeof(CharacterStateStanding);
		} else {
			return null;
		}
	}

	public override bool OnCollideWith(ICollidable collidable, CollisionInfo info) {
		return Character.CommandMovement.OnCollideWith(collidable, info);
	}
}


public class CharacterStateBlocking : CharacterState {
	public override CharacterStateType Type {get {return CharacterStateType.ACTIVE; }}
	private float _maxAcceleration = .005f;
	private float _rotationalSpeed = 5f*Mathf.Deg2Rad;
	private int _exposedDuration = 15;

	public CharacterStateBlocking(Character _machine, CharacterStateFactory _factory): base(_machine, _factory) {}

	protected override Type GetNewStateType() {
		if (Character.InputBlocking) {
			return null;
		} else {
			Character.UnsetBusy();

			if (Character.Parried) {
				Character.Parried = false;
				return typeof(CharacterStateStanding);
			} else {
				Character.SetBusy(_exposedDuration, false, false, 180f);
				return typeof(CharacterStateBusy);
			}
		}
	}

	protected override void EnterState() {
		base.EnterState();
		Character.Shield.gameObject.SetActive(true);
		Character.Parried = false;
		Character.SetBusy(false, false, 180f);
		Character.Velocity = Vector3.zero;
	}

	protected override void ExitState() {
		Character.Shield.gameObject.SetActive(false);
	}

	protected override void Tick() {
		Character.Velocity = MovementUtils.ChangeMagnitude(Character.Velocity, -_maxAcceleration).magnitude*Character.InputAimVector.normalized;
		// TODO return to this implementation because I don't know how to make it more elegant,
		// but the goal is:
		// - shielding towards the direction of movement slows you down, scaled up as direction is more parallel
		// - if you're running, shielding towards the direction of movement should rotate you (but not too much the same dir?)
		// - the faster you move, the more finnicky it should be
		// - :) https://www.youtube.com/watch?v=v3zT3Z5apaM
		// Vector3 shieldDirection = Character.InputAimVector;
		// float directionDot = Mathf.Max(Vector3.Dot(MovementUtils.inXZ(Character.Velocity).normalized, shieldDirection), 0);
		
		// Character.Velocity = Vector3.RotateTowards(
		//     MovementUtils.inXZ(Character.Velocity),
		//     Vector3.RotateTowards(Character.Velocity, -shieldDirection, 90f*Mathf.Deg2Rad*directionDot, 0),
		//     _rotationalSpeed, // rotate at speed according to whether we're running TODO tune rotation scaling
		//     0
		// );

		// if (directionDot > .5f) {
		//     float acceleration = _maxAcceleration*directionDot;
		//     Character.Velocity = MovementUtils.ChangeMagnitude(MovementUtils.inXZ(Character.Velocity), -acceleration);
		// }
	}

	public override bool OnCollideWith(ICollidable collidable, CollisionInfo info) => false;
}
