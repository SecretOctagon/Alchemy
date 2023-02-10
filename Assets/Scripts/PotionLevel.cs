using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionLevel : MonoBehaviour
{
    [Range(0, 1)] public float fill;
    [Header("components")]
    [SerializeField] Rigidbody rb;
    [SerializeField] Renderer rend;
    [Header("shape")]
    [SerializeField] Vector3[] centers;
    [SerializeField] float radius;

    [Header("Wobble")]
    [SerializeField] float restTime;
    [SerializeField] float wavePeriod;
    [SerializeField] float directionalVelocityResponse;
    [SerializeField] bool switchXZ;
    [SerializeField] float angularVelocityResponse;
    
    float pulse { get => 2 * Mathf.PI * wavePeriod; }
    Vector2 wobbleLimit;
    float timeElapsed;

    void Start()
    {
        if (!rend)
            rend = GetComponent<Renderer>();

        timeElapsed = 0;
    }

    void Update()
    {
        rend.material.SetFloat("_Fill", fill);
        Vector2 extremes = GetYExtremes();
        rend.material.SetVector("_Extremes", extremes);
        Vector2 wobble = GetWobble();
        rend.material.SetFloat("_WobbleX", wobble.x);
        rend.material.SetFloat("_WobbleZ", wobble.y);
    }
    Vector2 GetWobble()
    {
        wobbleLimit.x = Mathf.Lerp(wobbleLimit.x, 0, restTime * Time.deltaTime);
        wobbleLimit.y = Mathf.Lerp(wobbleLimit.y, 0, restTime * Time.deltaTime);

        float addx = rb.velocity.z * directionalVelocityResponse * Time.deltaTime;
        float addz = rb.velocity.x * directionalVelocityResponse * Time.deltaTime;
        switch (switchXZ)
        {
            case false:
                addx += (rb.angularVelocity.x * angularVelocityResponse * Time.deltaTime);
                addz += (rb.angularVelocity.z * angularVelocityResponse * Time.deltaTime);
                break;
            case true:
                addx += (rb.angularVelocity.z * angularVelocityResponse * Time.deltaTime);
                addz += (rb.angularVelocity.x * angularVelocityResponse * Time.deltaTime);
                break;
        }
        wobbleLimit += new Vector2(addx, addz);

        timeElapsed += Time.deltaTime * pulse;
        float sinus = Mathf.Sin(timeElapsed);
        float wobbleX = wobbleLimit.x * sinus;
        float wobbleZ = wobbleLimit.y * sinus;

        return new Vector2(wobbleX, wobbleZ);
    }
    Vector2 GetYExtremes()
    {
        float lowest = 0;
        float highiest = 0;

        foreach (Vector3 center in centers)
        {
            Vector3 centerWorld = transform.TransformDirection(center);
            float lowestPoint = centerWorld.y - radius;
            lowest = Mathf.Min(lowest, lowestPoint);
            float highiestPoint = centerWorld.y + radius;
            highiest = Mathf.Max(highiest, highiestPoint);
        }
        //Debug.Log(transform.parent.name + " lowest point is " + lowest + " higiest is " + highiest);
        return new Vector2(lowest, highiest);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        float planeY = 0;
        foreach (Vector3 center in centers)
        {
            Vector3 World = transform.TransformPoint(center);
            Gizmos.DrawWireSphere(World, radius);
            planeY += World.y;
        }
    }
}
