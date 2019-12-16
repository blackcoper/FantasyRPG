using UnityEngine;
using System.Collections;

public class GrowProperties {
    public Mesh mesh;
    public float time;
}
public class Crop : Seed {
    public GrowProperties[] grow;
    private float m_StartDay;
    private float m_RemainDsay;
    private bool m_Watered;
    
}
