using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Notifications : MonoBehaviour {

    List<Notification> notifications = new List<Notification>();
    Canvas canvas;

    public float textBrightness = 1.2f;
    public float stripBrightness = 1.1f;
    public float tabDarkness = 0.2f;
    public float yOffset = 40;
    public float positionCatchupSpeed = 0.04f;
    public GameObject example;

    public class Notification
    {
        public Color mainColor;
        public string[] values;
        public string locId;
        public float duration = 5f;
        public bool isEternal = false;
        public GameObject gameObject;

        public Notification(string _l, Color _c, params string[] _v)
        {
            mainColor = _c;
            values = _v;
            locId = _l;
        }
    }

    private void Start()
    {        
        canvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {

        if (GameManager.instance.IsPaused()) {
            return;
        }

        List<Notification> newList = new List<Notification>();
        int index = 0;
        float factor = canvas.scaleFactor;
        foreach (Notification not in notifications) {

            bool destroy = false;

            // Lifespan calculation
            if (!not.isEternal) {
                not.duration -= Time.deltaTime;
                if (not.duration <= 0f) {
                    Destroy(not.gameObject);
                    destroy = true;
                }
                else {
                    if (not.duration <= 1f) {
                        foreach(Image image in not.gameObject.GetComponentsInChildren<Image>()) {
                            image.color = new Color(image.color.r, image.color.g, image.color.b, not.duration);
                        }
                        Text txt = not.gameObject.GetComponentInChildren<Text>();
                        txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, not.duration);
                    }
                }
            }
            
            if (!destroy) {
                newList.Add(not);
            }

            // Position catch up
            float yShould = -yOffset * index;
            Transform notTransform = not.gameObject.transform;
            if (notTransform.localPosition.y != yShould)
                notTransform.localPosition = new Vector3(
                      notTransform.localPosition.x,
                      Mathf.Lerp(notTransform.localPosition.y, yShould, 0.05f),
                      notTransform.localPosition.z
             );

            index++;
        }
        notifications = newList;
    }

    public void Notify(Notification notification)
    {
        float factor = canvas.scaleFactor;

        GameObject nO = Instantiate(example, transform);
        nO.GetComponent<Image>().color = GetTabColor(notification.mainColor);
        nO.GetComponentsInChildren<Image>()[1].color = GetStripColor(notification.mainColor);

        Localization loc = GameManager.instance.localization;
        loc.SetCategory("notification");

        Text text = nO.GetComponentInChildren<Text>();
        text.color = GetTextColor(notification.mainColor);
        text.text = string.Format(loc.GetLine(notification.locId.ToString()), notification.values);

        nO.transform.position = new Vector3(
              nO.transform.position.x,
              nO.transform.position.y - yOffset* factor*notifications.Count,
              nO.transform.position.z
         );

        notification.gameObject = nO;

        notifications.Add(notification);
    }

    Color GetStripColor(Color color)
    {
        Color newColor = Color.Lerp(color, Color.white, stripBrightness);
        newColor.a = 1f;
        return newColor;
    }

    Color GetTextColor(Color color)
    {
        Color newColor = Color.Lerp(color, Color.white, textBrightness);
        newColor.a = 1f;
        return newColor;
    }

    Color GetTabColor(Color color)
    {
        Color newColor = Color.Lerp(color, Color.black, tabDarkness);
        newColor.a = 1f;
        return newColor;
    }
}
