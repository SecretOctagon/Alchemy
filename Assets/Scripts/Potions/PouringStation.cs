using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PouringStation : MonoBehaviour
{
    [SerializeField] float snapTime;
    [SerializeField] int potionLayer;
    bool isSnapping;
    [Header("Contents")]
    public PotionGlass glass;
    public PotionStats potion;
    public float amount;

    private void OnTriggerEnter(Collider other)
    {
        if (glass || isSnapping) return;

        PotionGlass newGlass = other.attachedRigidbody.GetComponent<PotionGlass>();
        if (newGlass)
        {
            switch (!newGlass.stats || newGlass.stats == potion)
            {
                case true:
                    PlayerGrab.active.Release();
                    StartCoroutine(LockGlass(other.attachedRigidbody, newGlass));
                    break;
                case false:
                    Debug.Log(newGlass.name + " is not empty");
                    break;
            }
        }
    }
    IEnumerator LockGlass(Rigidbody rb, PotionGlass newGlass)
    {
        isSnapping = true;

        rb.isKinematic = true;
        rb.useGravity = false;

        Vector3 startPosition = rb.position;
        Quaternion StartRotation = rb.rotation;
        Vector3 offset = rb.transform.GetChild(0).position - rb.position;
        Vector3 endPosition = transform.position - offset;
        float timeElapsed = 0;

        do
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / snapTime;
            rb.MovePosition(Vector3.Lerp(startPosition, endPosition, t));
            rb.MoveRotation(Quaternion.Slerp(StartRotation, transform.rotation, t));
            yield return null;
        }
        while (timeElapsed < snapTime);

        ChangeLayer(rb.transform, potionLayer);
        isSnapping = false;
        glass = newGlass;
    }
    void ChangeLayer(Transform tr, int layer)
    {
        List<Transform> transforms = new List<Transform>() { tr };
        transforms.AddRange(tr.GetComponentsInChildren<Transform>());
        foreach (Transform t in transforms)
            t.gameObject.layer = layer;
    }
    private void OnTriggerExit(Collider other)
    {
        PotionGlass leaving = other.attachedRigidbody.GetComponent<PotionGlass>();
        if (leaving == glass && !isSnapping)
            glass = null;
    }
}
