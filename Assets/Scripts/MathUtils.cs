using UnityEngine;

public class MathUtils
{
    public static int mod(int x, int m) {
        int r = x
            %m;
        return r<0 ? r+m : r;
    }

    public static bool pointInTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c) {
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
}
