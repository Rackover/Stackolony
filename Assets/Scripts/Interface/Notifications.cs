using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Notifications : MonoBehaviour {

    public class Notification
    {
        public Color mainColor;
        public string[] values;
        public int locId;

        Notification(int _l, Color _c, params string[] _v)
        {
            mainColor = _c;
            values = _v;
            locId = _l;
        }
    }

	public void Notify(Notification notification)
    {

    }
}
