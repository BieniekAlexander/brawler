using UnityEngine;

public class MathUtils
{
    public static int mod(int x, int m) {
        int r = x
            %m;
        return r<0 ? r+m : r;
    }

    public static bool PointInTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c) {
        // source: https://stackoverflow.com/questions/2049582/how-to-determine-if-a-point-is-in-a-2d-triangle/9755252#9755252
        float as_x = point.x - a.x;
        float as_y = point.y - a.y;
        bool s_ab = (b.x - a.x) * as_y - (b.y - a.y) * as_x > 0;

        if ((c.x - a.x) * as_y - (c.y - a.y) * as_x > 0 == s_ab)
            return false;
        if ((c.x - b.x) * (point.y - b.y) - (c.y - b.y)*(point.x - b.x) > 0 != s_ab)
            return false;
        return true;
    }

    public static void LineCircleIntersectionPoints(
        Vector2 circleCenter, float circleRadius,
        Vector2 rayPoint, Vector2 rayDirection,
        out Vector2 intersection0, out Vector2 intersection1
    ) {
        // adapted from https://www.csharphelper.com/howtos/howto_line_circle_intersection.html
        float dx, dy, A, B, C, det, t;
        Vector2 point1 = rayPoint;
        Vector2 point2 = rayPoint+rayDirection;

        dx = point2.x - point1.x;
        dy = point2.y - point1.y;

        A = dx * dx + dy * dy;
        B = 2 * (dx * (point1.x - circleCenter.x) + dy * (point1.y - circleCenter.y));
        C = (point1.x - circleCenter.x) * (point1.x - circleCenter.x) +
            (point1.y - circleCenter.y) * (point1.y - circleCenter.y) -
            circleRadius * circleRadius;

        det = B * B - 4 * A * C;
        if ((A <= 0.0000001) || (det < 0)) {
            // No real solutions.
            intersection0 = new Vector2(Mathf.Infinity, Mathf.Infinity);
            intersection1 = new Vector2(Mathf.Infinity, Mathf.Infinity);
        } else if (det == 0) { // One solution.
            t = -B / (2 * A);
            intersection0 = new Vector2(point1.x + t * dx, point1.y + t * dy);
            intersection1 =  new Vector2(Mathf.Infinity, Mathf.Infinity);
        } else { // Two solutions.
            t = (float)((-B + Mathf.Sqrt(det)) / (2 * A));
            intersection0 =
                new Vector2(point1.x + t * dx, point1.y + t * dy);
            t = (float)((-B - Mathf.Sqrt(det)) / (2 * A));
            intersection1 =
                new Vector2(point1.x + t * dx, point1.y + t * dy);
        }
    }

    public static float RayDistanceToCircle(
        Vector2 rayPoint, Vector2 rayDirection,
        Vector2 circleCenter, float circleRadius
    ) {
        LineCircleIntersectionPoints(
            circleCenter, circleRadius,
            rayPoint, rayDirection,
            out Vector2 i0, out Vector2 i1
        );

        // ternary operator checks if the point is towards the ray or away from it
        // TODO find a more performant way to do this
        return Mathf.Min(
            Vector2.Dot(i0-rayPoint, rayDirection)>0f
            ? (i0-rayPoint).magnitude
            : Mathf.Infinity,
            Vector2.Dot(i1-rayPoint, rayDirection)>0f
            ? (i1-rayPoint).magnitude
            : Mathf.Infinity
        );
    }
}

public class Range {
    // NOTE: not templating yet because Unity doesn't support the cool new number interfaces,
    // so templating this would take a bit of work
    public float Min { get; set ; }
    public float Max { get; set ; }

    public bool Contains(float arg) {
        return arg>Min && arg<Max;
    }

    public float Mean() => (Max+Min)/2f;

}