using UnityEngine;

public enum MovementOrientation {
    Neutral = 0,
    Advancing = 1,
    Retreating = 2
}

public enum CharacterPosture {
    Normal = 0,
    Squatting = 1,
    Aerial = 2
}

public static class MovementUtils {
    /* Math Stuff */
    public static Vector3 toXZ(Vector2 vector) {
        return new Vector3(vector.x, 0f, vector.y);
    }

    public static Vector3 inXZ(Vector3 vector) {
        return Vector3.Scale(vector, new Vector3(1, 0, 1));
    }

    public static Vector3 setXZ(Vector3 vector, Vector3 change) {
        return inY(vector) + inXZ(change);
    }

    public static Vector3 inY(Vector3 vector) {
        return Vector3.Scale(vector, Vector3.up);
    }

    public static Vector3 setY(Vector3 vector, Vector3 change) {
        return inY(change) + inXZ(vector);
    }

    public static Vector3 ChangeMagnitude(Vector3 vector, float changeInMagnitude, float clampMinimum=0f, float clampMaximum=Mathf.Infinity) {
        if ((vector.magnitude+changeInMagnitude)<0) {
            changeInMagnitude=-vector.magnitude;
        }

        return ClampMagnitude(
                vector.normalized * (vector.magnitude + changeInMagnitude),
                clampMinimum,
                clampMaximum
            );
    }

    public static Vector3 ClampMagnitude(Vector3 v, float min, float max) {
        double sm = v.sqrMagnitude;
        if (sm > (double)max * (double)max) return v.normalized * max;
        else if (sm < (double)min * (double)min) return v.normalized * min;
        return v;
    }

    /* Collision Stuff */
    public static Vector3 GetBounce(Vector3 velocity, Vector3 normal) {
            Vector3 bounceDirection = (velocity-2*Vector3.Project(velocity, normal)).normalized;
            return setXZ(velocity, bounceDirection*velocity.magnitude);
    }

    public static void Push(IMoves pushing, IMoves pushed, Vector3 normal) {
            Vector3 dvNormal = normal*Mathf.Max(pushing.BaseSpeed, pushed.BaseSpeed)*.1f;
            pushing.Velocity += dvNormal;
            pushed.Velocity = pushing.Velocity-dvNormal;
    }

    /* Controls Stuff */
    public static float StrafeSpeedMultiplier(Vector3 aimDirection, Vector3 moveDirection) {
        float dot = Vector3.Dot(aimDirection, moveDirection);

        if (dot>0) {
            return 1f;
        } else {
            return 1f+dot/5f;
        }
    }

    public static MovementOrientation GetAimMovementOrientation(Vector3 aimDirVector, Vector3 moveDirVector) {
        float aimMovementNorm = Vector3.Dot(moveDirVector.normalized, aimDirVector.normalized);
        return aimMovementNorm switch {
            >.707f => MovementOrientation.Advancing,
            <-.707f => MovementOrientation.Retreating,
            _ => MovementOrientation.Neutral
        };
    }

    /* Movement Stuff */
    public static void Slide(Character c) {
        c.Velocity = ChangeMagnitude(c.Velocity, -c.Friction);
    }
}