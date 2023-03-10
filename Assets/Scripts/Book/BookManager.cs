using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BookManager : MonoBehaviour
{
    public static BookManager active;    
    Vector2Int currentPage; //x is chapter; y is page in chapter;
    int chapterCount { get => transform.childCount; }
    int pageCount { get => transform.GetChild(currentPage.x).childCount; }
    [SerializeField] Camera cam;
    
    [SerializeField] Animator anim;
    int a_Open = Animator.StringToHash("Open");
    int a_Flip = Animator.StringToHash("Flip");
    Vector2Int holdTo;
    public bool isOpen { get => anim.GetBool(a_Open); }

    //startup
    private void Awake()
    {
        active = this;
    }
    private void Start()
    {
        if (!cam)
            cam = GetComponentInParent<Camera>();

        currentPage = Vector2Int.zero;
        for (int x = 0; x < chapterCount; x++) //hide other pages
        {
            Transform ChapterT = transform.GetChild(x);
            for (int y = 0; y < ChapterT.childCount; y++) 
            {
                Transform PageT = ChapterT.GetChild(y);
                PageT.gameObject.SetActive(currentPage.y == y);
            }
            ChapterT.gameObject.SetActive(currentPage.x == x);
        }
    }

    //public functions
    public void OpenBook(Vector2Int page)
    {
        PlayerMovement.active.enabled = false;
        PlayerGrab.active.enabled = false;
        DeviceInput.a.enabled = false;

        cam.enabled = true;
        anim.gameObject.SetActive(true);
        anim.SetBool(a_Open, true);
        anim.SetInteger(a_Flip, 0);

        page = ClampPage(page);
        SetPage(page);
    }
    public void CloseBook()
    {
        cam.enabled = false;
        anim.SetBool(a_Open, false);

        PlayerMovement.active.enabled = true;
        PlayerGrab.active.enabled = true;
        DeviceInput.a.enabled = true;
    }
    public void DialogueHighlight()
    {
        Debug.Log("The book wants to speak");
    }
    public void OpenOnPage(Vector2Int toPage)
    {
        switch (isOpen)
        {
            case true: //book is already open
                toPage = ClampPage(toPage);
                PageFlip(toPage, 0);
                break;
            case false: //book is closed
                OpenBook(toPage);
                break;
        }
    }
    public void OpenChapter(int chapter)
    {
        switch (currentPage.x == chapter)
        {
            case true: //chapter is already open
                switch (anim.GetBool(a_Open))
                {
                    case true: //book is open
                        CloseBook();
                        break;
                    case false: //book is closed
                        OpenBook(currentPage);
                        break;
                }
                break;
            case false: //change chapter
                OpenOnPage(new Vector2Int(chapter, 0));
                break;
        }
    }
    public void CloseOrOpen()
    {
        switch (anim.GetBool(a_Open))
        {
            case true:
                CloseBook();
                break;
            case false:
                OpenBook(currentPage);
                break;
        }
    }
    //input functions
    public void ChangePage(InputAction.CallbackContext context)
    {
        if (isOpen && context.performed)
        {
            int direction = 0;
            switch (context.ReadValue<float>() > 0)
            {
                case true: //next page
                    direction = 1;
                    break;
                case false: //previous page
                    direction = -1;
                    break;
            }
            Vector2Int toPage = currentPage + new Vector2Int(0, direction);
            toPage = ClampPage(toPage);
            switch (toPage == currentPage)
            {
                case true: //reached either end of the book
                    CloseBook();
                    break;
                case false: //recieved next page number properly
                    PageFlip(toPage, direction);
                    break;
            }            
        }
    }
    public void ChangeChapter(InputAction.CallbackContext context)
    {
        if (isOpen && context.performed)
        {
            int direction = 0;
            switch (context.ReadValue<float>() > 0)
            {
                case true: //next page
                    direction = 1;
                    break;
                case false: //previous page
                    direction = -1;
                    break;
            }
            Vector2Int toPage = currentPage + new Vector2Int(direction, 0);
            toPage = ClampPage(toPage);
            switch (toPage == currentPage)
            {
                case true: //reached either end of the book
                    CloseBook();
                    break;
                case false: //recieved next page number properly
                    PageFlip(toPage, direction);
                    break;
            }
        }
    }
    
    //internal functions
    Vector2Int ClampPage(Vector2Int requestedPage)
    {
        int chapter = Mathf.Clamp(requestedPage.x, 0, chapterCount);
        int lastPage = transform.GetChild(chapter).childCount - 1;
        int page = requestedPage.y;
        while (page < 0)
        {
            chapter = Mathf.Max(chapter - 1, 0);
            page += transform.GetChild(chapter).childCount;
        }
        while (page > transform.GetChild(chapter).childCount)
        {
            page -= transform.GetChild(chapter).childCount;
            chapter = Mathf.Min(chapter + 1, chapterCount);
        }
        return new Vector2Int(chapter, page);
    }
    public void SetPage(Vector2Int page)
    {
        Transform oldPage = GetPageTransform(currentPage);
        oldPage.parent.gameObject.SetActive(false);
        oldPage.gameObject.SetActive(false);
        currentPage = page;
        Transform newPage = GetPageTransform(page);
        newPage.parent.gameObject.SetActive(true);
        newPage.gameObject.SetActive(true);
    }
    void PageFlip(Vector2Int to, int direction)
    {
        Transform toT = GetPageTransform(to);
        toT.gameObject.SetActive(true);
        holdTo = to;

        if (direction == 0) 
        {   // calculate direction
            switch (currentPage.x == to.x)
            {
                case true: //both pages in the same chapter
                    direction = to.y - currentPage.y;
                    break;
                case false: //chapter change
                    direction = to.x - currentPage.x;
                    break;
            }
        }

        anim.SetInteger(a_Flip, direction);
    }
    public void EndFlip()
    {
        anim.SetInteger(a_Flip, 0);
        if (holdTo == Vector2Int.one * (-1))
            return;

        SetPage(holdTo);
        holdTo = Vector2Int.one * (-1);
    }
    Transform GetPageTransform(Vector2Int page)
    {
        Transform Chapter = transform.GetChild(page.x);
        Transform Page = Chapter.GetChild(page.y);
        return Page;
    }
}
