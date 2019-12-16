using System;
using UnityEngine;


namespace UnityStandardAssets.Utility
{
    public class FollowTarget : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0f, 56f, -55f);
        private Vector3 m_pos;

        private void LateUpdate()
        {
            if(target) {
                m_pos = target.position;
                //m_pos.y = 0;
                transform.position = m_pos + offset;
            }
        }
    }
}
