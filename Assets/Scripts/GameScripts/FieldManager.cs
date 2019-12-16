using UnityEngine;
using System.Collections;
using System;

public class FieldManager : MonoBehaviour {
    public GameObject FieldUntilled;
    public GameObject FieldTilled;
    public int SizeX = 3;
    public int SizeY = 3;
    public float margin = 1f;
    public float tileWidth = 2f;
    public float tileHeight = 2f;
    private Crop[,] m_Crop;
    private bool[,] m_Tilled ;

    private void init_field() {
        m_Tilled = new Boolean[SizeX, SizeY];
        for (int i = 0; i < SizeX; i++) {
            for (int j = 0; j < SizeY; j++) {
                m_Tilled[i, j] = false;
            }
        }
    }

    private GameObject m_GameObject;
    private void draw_field() {
        for (int i = 0; i < SizeX; i++) {
            for (int j = 0; j < SizeY; j++) {
                if(m_Tilled[i, j] == false) {
                    m_GameObject = (GameObject)Instantiate(FieldUntilled, new Vector3(i * (tileWidth+margin), 0, j * (tileHeight + margin)), Quaternion.identity);
                } else {
                    m_GameObject = (GameObject)Instantiate(FieldTilled, new Vector3(i * (tileWidth + margin), 0, j * (tileHeight + margin)), Quaternion.identity);
                }
                m_GameObject.transform.parent = transform;
            }
        }
    }

    // Use this for initialization
    void Start () {
        init_field();
        draw_field();
    }

    


    // Update is called once per frame
    void Update () {
	
	}
}
