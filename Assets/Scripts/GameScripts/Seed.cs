using UnityEngine;
using System.Collections;

public class PriceProperties {
    public int buy;
    public int sell;
}
public class Seed : MonoBehaviour {

    public string seedName{ get; set; }
    //public Mesh mesh;
    public PriceProperties price;
    public Crop crop;
    
}
