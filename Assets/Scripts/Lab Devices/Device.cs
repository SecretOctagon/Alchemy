using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Device : MonoBehaviour
{
    public Dictionary<string, LabTransmutation> recipes = new Dictionary<string, LabTransmutation>();
    public string Name;
    [SerializeField] Device chainInto;

    [Header("Ingredient")]
    [SerializeField] float ingredientSnapTime;
    [SerializeField] int ingredientSnapLayer;
    protected PortableItem ingredient;
    protected LabTransmutation currentRecipe;

    [Header("Snap player")]
    [SerializeField] float playerSnapTime;
    [SerializeField] Transform standHere;
    [SerializeField] Transform lookHere;

    protected virtual void Start()
    {
        LabTransmutation[] ingredients = GetComponentsInChildren<LabTransmutation>();
        foreach (LabTransmutation ing in ingredients)
        {
            recipes.Add(ing.IngredientIn, ing);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log(name + " collided with " + other);
        if (ingredient)
        {
            Debug.Log(name + " is already holding an ingredient");
            return;
        }

        PortableItem newIngredient = other.attachedRigidbody.GetComponent<PortableItem>();
        if (newIngredient && CanSnap(newIngredient.Name))
        {
            PlayerGrab.active.Release();
            ingredient = newIngredient;
            ingredient.InDevice = this;
            recipes.TryGetValue(ingredient.Name, out currentRecipe);
            ingredient.SnapTo(currentRecipe.transform.position, transform.rotation, ingredientSnapTime, ingredientSnapLayer, true);
        }
    }
    public virtual void StartUse()
    {
        HUDManager.active.ShowCrosshair(false);
        StartCoroutine(SnapPlayer());
    }
    IEnumerator SnapPlayer()
    {
        Transform playerTransform = PlayerMovement.active.transform;
        Vector3 startPosition = playerTransform.position;
        Vector3 endPosition = standHere.position;

        Quaternion startRotation = playerTransform.rotation;
        Vector3 lookVector = lookHere.position - standHere.position;
        lookVector.y = 0;
        Quaternion endRotation = Quaternion.LookRotation(lookVector);

        Transform CameraTransform = PlayerMovement.active.FPCamera.transform;
        Quaternion CamStartRotation = CameraTransform.rotation;
        Quaternion CamEndRotation = Quaternion.LookRotation(lookHere.position - (endPosition + CameraTransform.localPosition));

        PlayerMovement.active.enabled = false;
        float timeElapsed = 0;
        float t = 0;

        while (t <= 1)
        {
            timeElapsed += Time.deltaTime;
            t = timeElapsed / playerSnapTime;

            playerTransform.position = Vector3.Lerp(startPosition, endPosition, t);
            playerTransform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            CameraTransform.rotation = Quaternion.Lerp(CamStartRotation, CamEndRotation, t);

            yield return null;
        }
        OnSnapEnd();
    }
    protected virtual void OnSnapEnd()
    {
        Debug.Log("Player snapped to " + name);
        currentRecipe.StartTransmutation();
    }
    protected virtual void EndUse()
    {
        currentRecipe.EndTransmutation();
        currentRecipe = null;

        switch ((bool)chainInto)
        {
            case true:
                chainInto.StartUse();
                break;
            case false:
                PlayerMovement.active.enabled = true;
                PlayerMovement.active.InverseCameraT();
                PlayerGrab.active.enabled = true;
                HUDManager.active.ShowCrosshair(true);
                break;
        }
    }

    public virtual bool CanBeUsed()
    {
        return (bool)ingredient;
    }
    bool CanSnap(string ingredient)
    {
        return recipes.ContainsKey(ingredient);
    }
}
