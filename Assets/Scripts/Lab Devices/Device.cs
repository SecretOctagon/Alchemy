using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Device : MonoBehaviour
{
    [SerializeField] public Dictionary<string, GameObject> recipes; //[raw ingredient name] , [animated mesh]
    
    [SerializeField] float snapTime;
    [SerializeField] int ingredientLayer;
    PortableItem ingredient;

    [SerializeField] Transform standHere;
    [SerializeField] Transform lookHere;

    private void OnTriggerEnter(Collider other)
    {
        if (ingredient) return;

        PortableItem newIngredient = other.GetComponent<PortableItem>();
        if (newIngredient && CanSnap(newIngredient.Name))
        {
            ingredient = newIngredient;
        }
    }

    bool CanSnap(string ingredient)
    {
        return recipes.ContainsKey(ingredient);
    }
}
