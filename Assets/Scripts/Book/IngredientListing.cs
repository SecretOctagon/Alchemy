using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngredientListing : MonoBehaviour
{
    public static Dictionary<string,IngredientListing> list = new Dictionary<string, IngredientListing>();

    public string Name;
    public int count;
    [SerializeField] GameObject prefab;
    [SerializeField] TMPro.TMP_Text countUI;

    void Start()
    {
        if (list == null)
            list = new Dictionary<string, IngredientListing>();

        list.Add(Name, this);
    }
    void OnDestroy()
    {
        list.Remove(Name);
    }

    private void OnEnable()
    {
        countUI.text = count.ToString();//("00");
    }

    public Vector2Int GetPage()
    {
        Transform PageT = transform.parent;
        int Page = PageT.GetSiblingIndex();
        Transform ChapterT = PageT.parent;
        int Chapter = ChapterT.GetSiblingIndex();
        return new Vector2Int(Chapter, Page);
    }
}
