using UnityEngine;
using System.Collections;

public class test : MonoBehaviour {
    public bool maju = false;

	// Update is called once per frame
	void Update () {
        //if(maju)transform.Translate( * 1f * Time.deltaTime);
        GameObject.Find("DebugText").GetComponent<UnityEngine.UI.Text>().text = transform.position.normalized.ToString();
    }
}
