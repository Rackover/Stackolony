using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BulletinDisplay : MonoBehaviour {

    public bool hasUnread = false;
    public GameObject notification;
    public GameObject bulletinWindow;

    public Text title;
    public Text subTitle;
    public Text text;

    public float buttonDisableTimespan = 1f;

    bool opened = false;
    BulletinsManager.Bulletin currentBulletin;
    Animator animator;
    Button button;

    private void Start()
    {
        animator = bulletinWindow.GetComponent<Animator>();
        button = GetComponent<Button>();
    }

    public IEnumerator RefreshBulletin()
    {
        if (
            currentBulletin == null ||
            GameManager.instance.bulletinsManager.GetBulletin().id != currentBulletin.id
        ) {
            currentBulletin = GameManager.instance.bulletinsManager.GetBulletin();
            Localization loc = GameManager.instance.localization;

            title.text = loc.GetLine("bulletin" + currentBulletin.id.ToString(), "bulletinTitle").ToUpper();
            subTitle.text = loc.GetLine("bulletin" + currentBulletin.id.ToString(), "bulletinSubtitle");
            text.text = loc.GetLine("bulletin" + currentBulletin.id.ToString(), "bulletinText");

            SetUnread(!opened);
        }

        yield return new WaitForSeconds(FindObjectOfType<Interface>().refreshRate);
        yield return StartCoroutine(RefreshBulletin());
    }

    public void ToggleBulletinDisplay()
    {
        SetUnread(false);
        animator.Play(opened ? "Fold" : "Unfold");
        StartCoroutine(AnimateFor(buttonDisableTimespan));
        opened = !opened;
    }

    public void SetUnread(bool unread)
    {
        hasUnread = unread;
        notification.SetActive(unread);
    }

    IEnumerator AnimateFor(float seconds)
    {
        button.interactable = false;
        yield return new WaitForSeconds(seconds);
        button.interactable = true;
    }
}
