using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrab : MonoBehaviour
{
    public static PlayerGrab active;
    PortableItem holdObject;
    [HideInInspector] public bool isHolding;
    [SerializeField] LayerMask ScanMask;
    [SerializeField] int grabLayer;
    [SerializeField] int holdLayer;
    [SerializeField] float MaxGrabDistance;
    [SerializeField] float HoldDistance;
    [SerializeField] Transform targetTransform;
    [SerializeField] Camera cam;

    void Awake()
    {
        active = this;
    }
    void Start()
    {
        if (!cam)
        {
            cam = GetComponent<Camera>();
            if (!cam)
            {
                cam = GetComponentInChildren<Camera>();
                if (!cam)
                    cam = Camera.main;
            }
        }
        if (!targetTransform)
            targetTransform = cam.transform.GetChild(0);
    }

    void Update()
    {
        Ray ray = cam.ViewportPointToRay(Vector3.one * 0.5f);
        switch (isHolding)
        {
            case false:
                switch (Physics.Raycast(ray, out RaycastHit hitInfo, MaxGrabDistance, ScanMask, QueryTriggerInteraction.Ignore))
                {
                    case true:
                        switch (hitInfo.rigidbody && hitInfo.rigidbody.gameObject.layer == grabLayer)
                        {
                            case true:
                                holdObject = hitInfo.rigidbody.GetComponent<PortableItem>();
                                //Debug.Log("looking at " + holdObject);
                                switch ((bool)holdObject)
                                {
                                    case true:
                                        HUDManager.active.InteractionPrompt(true, holdObject.Name);
                                        return;
                                    case false:
                                        HUDManager.active.InteractionPrompt(true, "Interact");
                                        return;
                                }
                        }
                        break;
                }
                HUDManager.active.InteractionPrompt(false, "");
                holdObject = null;
                break;
            case true:
                switch (Physics.SphereCast(ray, holdObject.radius, out RaycastHit hit, HoldDistance, ScanMask, QueryTriggerInteraction.Ignore))
                {
                    case true:
                        targetTransform.position = hit.point + holdObject.radius * hit.normal;
                        //Debug.Log("hit at " + hit.point);
                        break;
                    case false:
                        targetTransform.position = cam.transform.TransformPoint(0, 0, HoldDistance);
                        //Debug.Log("no hit, moving to " + targetTransform.position);
                        break;
                }
                holdObject.rb.MovePosition(targetTransform.position);
                holdObject.rb.MoveRotation(targetTransform.rotation);
                break;
        }
    }

    public void Grab()
    {
        if (isHolding || !holdObject) return;

        isHolding = true;
        holdObject.rb.isKinematic = true;
        holdObject.rb.useGravity = false;

        List<Transform> withChildren = GetChildren(holdObject.transform);
        foreach (Transform c in withChildren) c.gameObject.layer = holdLayer;

        HUDManager.active.InteractionPrompt(false, "");
        List<ButtonPrompt> prompts = new List<ButtonPrompt> { ButtonPrompt.Drop };
        switch (holdObject.storage)
        {
            case ItemStorage.toBelt:
                prompts.Add(ButtonPrompt.toBelt);
                break;
            case ItemStorage.toCollection:
                prompts.Add(ButtonPrompt.ToInventory);
                break;
        }
        if (holdObject.hasUse)
            prompts.Add(ButtonPrompt.Use);
        HUDManager.active.HoldingPrompt(prompts);
    }
    public void Release()
    {
        if (!isHolding || !holdObject) return;

        isHolding = false;
        holdObject.rb.isKinematic = false;
        holdObject.rb.useGravity = true;

        List<Transform> withChildren = GetChildren(holdObject.transform);
        foreach (Transform c in withChildren) c.gameObject.layer = grabLayer;

        HUDManager.active.HoldingPrompt(new List<ButtonPrompt>());
    }
    public void GrabOrRelease()
    {
        if (!holdObject) return;

        if (isHolding)
        {
            Debug.Log("releasing " + holdObject);
            Release();
        }
        else
        {
            Debug.Log("grabbing " + holdObject);
            Grab();
        }
    }
    List<Transform> GetChildren(Transform parent)
    {
        List<Transform> children = new List<Transform>();
        children.Add(parent);
        for (int i = 0; i < parent.childCount; i++)
            children.Add(parent.GetChild(i));
        return children;
    }

    public void ToInventory()
    {
        switch ((bool)holdObject)
        {
            case true:
                Debug.Log("storing " + holdObject.name);
                switch (holdObject.storage)
                {
                    case ItemStorage.toBelt: //send to belt
                        Debug.Log("add " + holdObject.Name + " to belt");
                        Release();
                        return;
                    case ItemStorage.toCollection: //send to book's collection
                        switch (IngredientListing.list.ContainsKey(holdObject.Name))
                        {
                            case true:
                                Debug.Log("sending " + holdObject.Name + " to book");
                                IngredientListing listing = IngredientListing.list[holdObject.Name];
                                listing.count++;
                                BookManager.active.SetPage(listing.GetPage());
                                Release();
                                Destroy(holdObject.gameObject);
                                return;
                            case false:
                                Debug.Log("Ingredient name not found");
                                break;
                        }
                        break;
                }
                break;
        }
    }
}
