using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathDevice : Device
{
    [Header("Path")]
    [SerializeField] public Bezier[] path = new Bezier[1];
    [SerializeField] float completionTime;
    public float[] segmentLengths;
    public float totalLength;
    public int lengthEstimationSamples;
    [SerializeField] float[] timeMultipliers;
    [SerializeField] Transform moveObject;
    [SerializeField] Transform[] rotationGuides;
    [SerializeField] RotationMode rotationMode;
    float progress;

    [Header("Gizmos")]
    [SerializeField] bool drawGizmos;
    [SerializeField] float GizmoRadius;
    [SerializeField] int GizmoResolution;

    protected override void Start()
    {
        base.Start();
        GetTimes();
        switch (rotationMode)
        {
            case RotationMode.StartOn1:
                moveObject.rotation = rotationGuides[1].rotation;
                break;
            case RotationMode.always0:
            case RotationMode.trackTo0:
                moveObject.rotation = rotationGuides[0].rotation;
                break;
        }
    }
    void GetTimes()
    {
        segmentLengths = new float[path.Length];
        totalLength = 0;
        for (int i = 0; i < path.Length; i++)
        {
            segmentLengths[i] = path[i].LengthEstimate(lengthEstimationSamples);
            totalLength += segmentLengths[i];
        }

        timeMultipliers = new float[path.Length];
        for (int i = 0; i < path.Length; i++)
        {
            timeMultipliers[i] = segmentLengths[i] * completionTime / totalLength;
        }
    }

    protected override void OnSnapEnd()
    {
        base.OnSnapEnd();
        Destroy(ingredient.gameObject);
        StartCoroutine(UseLoop());
    }
    IEnumerator UseLoop()
    {
        progress = 0;
        while (progress < path.Length)
        {
            Advance();
            yield return null;
        }
        EndUse();
    }

    void Advance()
    {
        int segment = Mathf.FloorToInt(progress);
        float t = progress % 1;

        //get bezier point
        Vector3 positionWorld = path[segment].GetPointAt(t, out Vector3 forwardWorld);
        
        //move moveObject
        moveObject.position = positionWorld;
        Quaternion rotation = rotationGuides[0].rotation;
        switch (rotationMode) {
            case RotationMode.StartOn1:
                if (segment == 0)
                    rotation = Quaternion.Lerp(rotationGuides[1].rotation, rotationGuides[0].rotation, t);
                else if (segment == path.Length - 1)
                    rotation = Quaternion.Lerp(rotationGuides[0].rotation, rotationGuides[1].rotation, t);
                break;
            case RotationMode.trackTo0:
                Debug.Log("no code here");
                break;
        }
        moveObject.rotation = rotation;

        //recipe animation
        currentRecipe.AdvanceTransmutation(progress);

        //update progress
        progress += Time.deltaTime * ProgressionDot(positionWorld, forwardWorld) / timeMultipliers[segment];
    }
    float ProgressionDot(Vector3 positionWorld, Vector3 forwardWorld)
    {
        Vector2 currentViewPos = DeviceInput.a.cam.WorldToScreenPoint(positionWorld);
        Vector2 forwardViewPos = DeviceInput.a.cam.WorldToScreenPoint(forwardWorld);
        Vector2 Direction = forwardViewPos - currentViewPos;
        Direction.Normalize();
        Vector2 inputVector = DeviceInput.a.LookInput;
        float dot = Vector2.Dot(inputVector, Direction);
        //Color color = Color.Lerp(Color.red, Color.green, dot);
        //HUDManager.active.ShowArrow(true, currentViewPos, Direction, color);
        return Mathf.Clamp01(dot);
    }

    private void OnDrawGizmos()
    {
        if (drawGizmos && path.Length > 0)
        {
            float[] Ts = new float[GizmoResolution];
            for (int i = 0; i < GizmoResolution; i++)
                Ts[i] = (1f / (float)GizmoResolution) * i;

            List<Vector3> LPoints = new List<Vector3>();
            List<Transform> CPoints = new List<Transform>();
            foreach (Bezier b in path)
            {
                CPoints.AddRange(b.points);
                foreach (float t in Ts)
                    LPoints.Add(b.GetPointAt(t));
            }

            Gizmos.color = Color.cyan;
            foreach (Transform cPoint in CPoints)
                Gizmos.DrawSphere(cPoint.position, GizmoRadius);

            for (int i = 1; i < LPoints.Count; i++)
            {
                Gizmos.color = Color.Lerp(Color.red, Color.green, i / (float)LPoints.Count);                
                Gizmos.DrawLine(LPoints[i - 1], LPoints[i]);
            }
        }
    }
}
public enum RotationMode
{
    always0,
    StartOn1,
    trackTo0
}

[System.Serializable] public struct Bezier
{
    public Transform[] points;

    public Bezier(Transform[] _points)
    {
        points = _points;
    }

    public Vector3 GetPointAt(float t)
    {
        return GetPointAt(t, out Vector3 fp);
    }
    public Vector3 GetPointAt(float t, out Vector3 forwardPoint)
    {
        forwardPoint = Vector3.zero;
        switch (points.Length)
        {
            case 1:
                forwardPoint = points[0].position;
                return points[0].position;
            case 2:
                forwardPoint = points[1].position;
                return Vector3.Lerp(points[0].position, points[1].position, t);
            case 3:
                Vector3[] points2 = new Vector3[2];
                for (int i = 0; i < points2.Length; i++)
                {
                    points2[i] = Vector3.Lerp(points[i].position, points[i + 1].position, t);
                }
                forwardPoint = points2[1];
                return Vector3.Lerp(points2[0], points2[1], t);
            case 4:
                points2 = new Vector3[3];
                for (int i = 0; i < points2.Length; i++)
                {
                    points2[i] = Vector3.Lerp(points[i].position, points[i + 1].position, t);
                }
                Vector3[] points3 = new Vector3[2];
                for (int i = 0; i < points3.Length; i++)
                {
                    points3[i] = Vector3.Lerp(points2[i], points2[i + 1], t);
                }
                forwardPoint = points3[1];
                return Vector3.Lerp(points3[0], points3[1], t);
            default:
                return Vector3.zero;
        }
    }

    public float LengthEstimate(int samples)
    {
        if (points.Length == 2)
        {
            Vector3 difference = points[1].position - points[0].position;
            return difference.magnitude;
        }
        if (points.Length > 2)
        {
            Vector3[] samplePoints = new Vector3[samples + 1];
            samplePoints[0] = points[0].position;
            float length = 0;
            for (int i = 1; i <= samples; i++)
            {
                float t = (1f / samples) * i;
                samplePoints[i] = GetPointAt(t);
                Vector3 difference = samplePoints[i] - samplePoints[i - 1];
                float diff = difference.magnitude;
                length += diff;
            }
            return length;
        }
        return 0;
    }
}
