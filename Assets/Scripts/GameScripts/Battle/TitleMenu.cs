using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TitleMenu : MonoBehaviour
{
	void Update ()
	{
		if(Input.GetButtonDown("Attack_1"))
		{
			//Application.LoadLevel(Application.loadedLevel+1);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
		}
	}
}
