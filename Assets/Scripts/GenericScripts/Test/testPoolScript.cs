using UnityEngine;
using System.Collections;

public class testPoolScript : MonoBehaviour {

    public GameObject prefab;
    void Start() {
        PoolManager.Instance.CreatePool(prefab, 5);
    }
    void Update () {
        if (Input.GetKeyDown(KeyCode.Space)) {
            PoolManager.Instance.ReuseObject(prefab, transform.position + (transform.forward * 1) + new Vector3(0,1,0), transform.rotation);
        }
	    
	}
}
