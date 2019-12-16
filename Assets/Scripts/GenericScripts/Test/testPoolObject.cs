using UnityEngine;
using System.Collections;

public class testPoolObject : PoolObject {
    public TrailRenderer trail;
    void Update () {
        //transform.localScale += Vector3.one * Time.deltaTime * 2;
        //transform.Translate(Vector3.forward * Time.deltaTime * 25);
    }

    public override void OnObjectReuse() {
        CancelInvoke();
        trail.Clear();
        transform.localScale = Vector3.one;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().AddForce(transform.forward * 100,ForceMode.Impulse);
        Invoke("Destroy", 5f);
    }
}
