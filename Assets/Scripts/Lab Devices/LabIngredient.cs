using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabIngredient : MonoBehaviour
{
    public string IngredientIn;
    public GameObject IngredientOut;
    Animator anim;
    [SerializeField] bool continuousProgress;
    [SerializeField] int[] advanceOn;
    int a_Progress = Animator.StringToHash("Progress");

    private void Start()
    {
        anim = GetComponent<Animator>();
        if (!anim)
            anim = GetComponentInChildren<Animator>();

        ChildrenSetActive(false);
    }

    public void StartTransmutation()
    {
        ChildrenSetActive(true);
        switch(continuousProgress)
        {
            case true:
                anim.SetFloat(a_Progress, 0);
                break;
            case false:
                anim.SetInteger(a_Progress, 0);
                break;
        }
    }
    public void EndTransmutation()
    {
        if (IngredientOut)
            Instantiate(IngredientOut, transform.position, transform.rotation);
        ChildrenSetActive(false);
    }
    public void AdvanceTransmutation(float progress)
    {
        switch (continuousProgress)
        {
            case true:
                anim.SetFloat(a_Progress, progress);
                break;
            case false:
                int progInt = 0;
                foreach (int i in advanceOn)
                {
                    if (progress > i)
                        progInt++;
                }
                anim.SetInteger(a_Progress, progInt);
                break;
        }
    }
    void ChildrenSetActive(bool value)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(value);
        }
    }
}
