using UnityEngine;
using System.Collections;

public class ClearSight : MonoBehaviour {
    // Use this for initialization
    public float offset = 0f;

    private Shader m_OldShader = null;
    private Color m_OldColor = Color.black;
    private float m_Transparency = 1f;
    private const float m_TargetTransparancy = 0.3f;
    private const float m_FallIn = 0.2f;
    private const float m_FallOut = 1f;
    private Renderer m_Renderer;
    private bool isHidden = false;

	// Update is called once per frame
	void FixedUpdate () {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward,out hit, Vector3.Distance(Camera.main.transform.position, transform.position)+offset)) {
            Renderer R = hit.collider.GetComponent<Renderer>();
            
            if (R) {
                if (Vector3.Distance(transform.position, R.transform.position) > offset) {
                    isHidden = true;
                    if (m_OldShader == null) {
                        m_Transparency = 1;
                        m_Renderer = R;
                        m_OldShader = R.material.shader;
                        m_OldColor = R.material.color;
                        R.material.shader = Shader.Find("Transparent/Diffuse");
                    }
                } else {
                    isHidden = false;
                }
                
            }
        }
    }
    void Update() {
        
        if (m_Renderer == null) return;
        if (isHidden) {
            if(m_Transparency > m_TargetTransparancy) {
                m_Transparency -= ((1.0f - m_TargetTransparancy) * Time.deltaTime) / m_FallIn;
            }
        } else {
            if (m_Transparency >= 1.0f) {
                m_Renderer.material.shader = m_OldShader;
                m_Renderer.material.color = m_OldColor;
                m_OldShader = null;
                isHidden = false;
                Color _c = m_Renderer.material.color;
                _c.a = m_Transparency;
                m_Renderer.material.color = _c;
                m_Renderer = null;
                return;
            } else {
                m_Transparency += ((1.0f - m_TargetTransparancy) * Time.deltaTime) / m_FallOut;
            }
        }
        Color c = m_Renderer.material.color;
        c.a = m_Transparency;
        m_Renderer.material.color = c;
        
    }
}
