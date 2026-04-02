using UnityEngine;

public class ArcheryTarget : MonoBehaviour
{
    [Header("Ring Radii")]
    public float bullseyeRadius = 0.05f;
    public float innerRadius = 0.10f;
    public float midRadius = 0.18f;
    public float outerRadius = 0.26f;
    public float maxRadius = 0.34f;

    public int GetScoreFromHitPoint(Vector3 worldHitPoint)
    {
        Vector3 localPoint = transform.InverseTransformPoint(worldHitPoint);

        float distanceFromCenter = new Vector2(localPoint.x, localPoint.y).magnitude;

        if (distanceFromCenter <= bullseyeRadius) return 10;
        if (distanceFromCenter <= innerRadius) return 8;
        if (distanceFromCenter <= midRadius) return 6;
        if (distanceFromCenter <= outerRadius) return 4;
        if (distanceFromCenter <= maxRadius) return 2;

        return 0;
    }
}