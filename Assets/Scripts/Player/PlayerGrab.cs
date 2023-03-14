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
    void OnDisable()
    {
        HUDManager.active.ShowPrompts(new List<ButtonPrompt>());
    }

    void Update()
    {
        Ray ray = cam.ViewportPointToRay(Vector3.one * 0.5f);
        switch (isHolding)
        {
            case false: //search for a PortableItem
                switch (Physics.Raycast(ray, out RaycastHit hitInfo, MaxGrabDistance, ScanMask, QueryTriggerInteraction.Ignore))
                {
                    case true: //physics object hit
                        switch (hitInfo.rigidbody && hitInfo.rigidbody.gameObject.layer == grabLayer)
                        {
                            case true: //rigidbody found
                                holdObject = hitInfo.rigidbody.GetComponent<PortableItem>();
                                Debug.Log("looking at " + holdObject);
                                switch ((bool)holdObject)
                                {
                                    case true: //PortableItem found
                                        LookHUD();
                                        return;
                                }
                                break;
                        }
                        break;
                }
                //no PortableItem found
                HUDManager.active.ShowPrompts(new List<ButtonPrompt>());
                holdObject = null;
                break;
            case true: //manipulate object held
                switch (Physics.SphereCast(ray, holdObject.radius, out RaycastHit hit, HoldDistance, ScanMask, QueryTriggerInteraction.Ignore))
                {
                    case true:
                        targetTransform.position = hit.point + holdObject.radius * hit.normal;
                        break;
                    case false:
                        targetTransform.position = cam.transform.TransformPoint(0, 0, HoldDistance);
                        break;
                }
                holdObject.rb.MovePosition(targetTransform.position);
                holdObject.rb.MoveRotation(targetTransform.rotation);
                break;
        }
    }
    void LookHUD()
    {
        HUDManager.active.ShowPrompts(new List<ButtonPrompt>());
        switch (holdObject.canBeCarried)
        {
            case true:
            HUDManager.active.ShowPromptCommand(ButtonPrompt.Grab, "Pick " + holdObject.Name);
                break;
        }
        switch ((bool)holdObject.InDevice && holdObject.InDevice.CanBeUsed())
        {
            case true:
                HUDManager.active.ShowPromptCommand(ButtonPrompt.Use, "Use " + holdObject.InDevice.Name);
                break;
        }
        switch (holdObject.storage)
        {
            case ItemStorage.toHerbarium:
                bool isListed = IngredientListing.list.ContainsKey(holdObject.Name);
                switch (isListed)
                {
                    case true:
                        HUDManager.active.ShowPromptCommand
                                    (ButtonPrompt.toHerbarium, "Collect " + holdObject.Name);
                        break;
                }
                break;
            case ItemStorage.toBelt:
                Debug.Log("Send " + holdObject + " to belt");
                break;
        }
    }
    
    public void Grab()
    {
        if (isHolding || !holdObject || !holdObject.canBeCarried) return;

        isHolding = true;
        holdObject.rb.isKinematic = true;
        holdObject.rb.useGravity = false;
        holdObject.InDevice = null;

        List<Transform> withChildren = GetChildren(holdObject.transform);
        foreach (Transform c in withChildren) c.gameObject.layer = holdLayer;

        List<ButtonPrompt> prompts = new List<ButtonPrompt> { ButtonPrompt.Drop };
        switch (holdObject.storage)
        {
            case ItemStorage.toBelt:
                prompts.Add(ButtonPrompt.toBelt);
                break;
            case ItemStorage.toHerbarium:
                prompts.Add(ButtonPrompt.toHerbarium);
                break;
        }
        HUDManager.active.ShowPrompts(prompts);
        if (holdObject.hasUse)
            HUDManager.active.ShowPromptCommand(ButtonPrompt.Use, "Use " + holdObject.Name);
    }
    public void Release()
    {
        if (!isHolding || !holdObject) return;

        isHolding = false;
        holdObject.rb.isKinematic = false;
        holdObject.rb.useGravity = true;

        List<Transform> withChildren = GetChildren(holdObject.transform);
        foreach (Transform c in withChildren) c.gameObject.layer = grabLayer;

        //HUDManager.active.ShowPrompts(new List<ButtonPrompt>());
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

    public void UseItem()
    {
        if (holdObject)
        {
            if ((bool)holdObject.InDevice)
            {
                Debug.Log("Using " + holdObject.InDevice.name);
                holdObject.InDevice.StartUse();
            }
            else if (holdObject.hasUse)
            {
                Debug.Log("Using " + holdObject.Name);
            }
            else
                Debug.Log(holdObject.name + " can't be used");
        }
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
                    case ItemStorage.toHerbarium: //send to book's collection
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
