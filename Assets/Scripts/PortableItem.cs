using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortableItem : MonoBehaviour
{
    public float radius;
    public string Name;
    public ItemStorage storage;
    public bool canBeCarried;
    public bool hasUse;
    public Device InDevice;
    [HideInInspector] public Rigidbody rb;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SnapTo(Vector3 position, Quaternion rotation, float time, int layer, bool lockInPosition)
    {
        gameObject.layer = layer;
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.layer = layer;
        
        rb.isKinematic = true;
        rb.useGravity = false;

        StartCoroutine(SnapCor(position, rotation, time, lockInPosition));
    }
    IEnumerator SnapCor(Vector3 endPos, Quaternion endRot, float time, bool lockInPosition)
    {
        Vector3 startPos = rb.position;
        Quaternion startRot = rb.rotation;

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / time;
            rb.MovePosition(Vector3.Lerp(startPos, endPos, t));
            rb.MoveRotation(Quaternion.Lerp(startRot, endRot, t));
            yield return null;
        }

        rb.isKinematic = lockInPosition;
        rb.useGravity = !lockInPosition;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);  
    }
}
public enum ItemStorage
{
    none,
    toHerbarium,
    toBelt
}
