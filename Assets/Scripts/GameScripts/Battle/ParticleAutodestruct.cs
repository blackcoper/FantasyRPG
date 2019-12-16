using UnityEngine;
using System.Collections;

public class ParticleAutodestruct : MonoBehaviour
{
	void Start ()
	{
		if(!GetComponent<ParticleSystem>().loop)
		{
			Destroy(gameObject, GetComponent<ParticleSystem>().duration);
		}
	}
	
	public void DestroyGracefully()
	{
		DestroyGracefully(gameObject);
	}
	
	static public void DestroyGracefully(GameObject go)
	{
		go.transform.parent = null;
		go.GetComponent<ParticleSystem>().loop = false;
        ParticleSystem.EmissionModule em = go.GetComponent<ParticleSystem>().emission;
        em.enabled = false;
        Destroy(go, go.GetComponent<ParticleSystem>().duration);
	}
}
