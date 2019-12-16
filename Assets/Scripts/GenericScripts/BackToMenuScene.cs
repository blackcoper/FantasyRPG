using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BackToMenuScene : MonoBehaviour {

	void Start () {
        if (GameObject.Find("NetworkManager") != null) {
            Destroy(this);
        } else {
            SceneManager.LoadScene("Menu");// Application.LoadLevel("Menu");
        }
	}
	
}
