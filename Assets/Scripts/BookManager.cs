using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookManager : MonoBehaviour
{
    public static BookManager active;
    int currentChapter;
    int currentPage;
    int chapterCount { get => transform.childCount; }
    int pageCount { get => transform.GetChild(currentChapter).childCount; }
    [SerializeField] Camera cam;

    private void Start()
    {
        if (!cam)
            cam = GetComponentInParent<Camera>();
    }

    public void OpenBook()
    {
        cam.enabled = true;
    }
    public void CloseBook()
    {
        cam.enabled = false;
    }
    public void DialogueHighlight()
    {

    }

    public void OpenChapter(int chapter)
    {
        if (chapter < 0 || chapter > chapterCount - 1)
        {
            Debug.Log("chapter outside of range");
            return;
        }

        if (chapter == currentChapter)
        {
            CloseBook();
            return;
        }

        currentChapter = chapter;
        OpenPage(0);
    }
    public void ChangeChapter(int forward)
    {
        currentChapter += forward;
        if (currentChapter > chapterCount - 1 || currentChapter < 0)
        {
            CloseBook();
            return;
        }
        OpenChapter(currentChapter);
    }
    void OpenPage(int page)
    {
        currentPage = page;

        for (int c = 0; c < chapterCount; c++)
        {
            Transform chapter = transform.GetChild(c);
            for (int p = 0; p < chapter.childCount; p++)
            {
                chapter.GetChild(p).gameObject.SetActive(c == currentChapter && p == currentPage);
            }
        }
    }
    public void ChangePage(int forward)
    {
        currentPage += forward;
        while (currentPage > pageCount - 1) 
        {
            currentPage %= pageCount;
            currentChapter++;
            if(currentChapter > chapterCount - 1)
            {
                CloseBook();
                return;
            }
        }
        while (currentPage < 0)
        {
            currentChapter--;
            if (currentChapter < 0)
            {
                CloseBook();
                return;
            }
            currentPage -= pageCount;
        }
        OpenPage(currentPage);
    }
}
