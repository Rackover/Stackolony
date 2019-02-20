using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreditsDisplay : MonoBehaviour {

    [System.Serializable]
    public class CreditStaff
    {
        public string name;
        public string function;
        public Sprite logo;
    }

    public List<CreditStaff> staff = new List<CreditStaff>();
    public GameObject container;
    public GameObject creditStaffPrefab;

	// Use this for initialization
	void Start () {
        foreach(CreditStaff person in staff) {
            CreditsItem member = Instantiate(creditStaffPrefab, container.transform).GetComponent<CreditsItem>();
            member.SetName(person.name);
            member.SetFunction(person.function);
            member.SetLogo(person.logo);
        }
	}

    public void DestoryWindow()
    {
        Destroy(gameObject);
    }
	
}
