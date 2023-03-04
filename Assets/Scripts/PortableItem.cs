using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortableItem : MonoBehaviour
{
    public float radius;
    public string Name;
    public ItemStorage storage;
    public bool hasUse;
    [HideInInspector] public Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
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
    toCollection,
    toBelt
}
