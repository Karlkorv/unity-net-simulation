using UnityEngine;

namespace Code
{
    public class InputControl : MonoBehaviour 
    {
        private const int MouseButton = 0;
        private const int TargetGizmoRadius = 1;
        private const float DragSpeed = 0.015f;

        private Transform m_dragHook;
        private bool m_dragging;
        private Vector2 m_mouseStart;
        private Vector3 m_dragOrigin;
        private Vector3 m_dragClickHit;

        private void Start () 
        {
	    
        }

        private void Update () 
        {
            if(m_dragging)
            {
                if(Input.GetMouseButton(MouseButton) && m_dragHook != null)
                {
                    //drag along xz-plane
                    //Vector2 move = (Vector2)Input.mousePosition - m_mouseStart;
                    //move *= m_dragSpeed;
                    //m_dragHook.position = new Vector3(m_dragOrigin.x + move.x, m_dragOrigin.y, m_dragOrigin.z + move.y);

                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit[] hits;
                    hits = Physics.RaycastAll(ray, 100.0f);
                    foreach(RaycastHit hit in hits)
                    {
                        if(hit.transform.gameObject.tag == "Ground")
                        {
                            Vector3 delta = hit.point - m_dragClickHit;
                            Vector3 position = new Vector3(m_dragOrigin.x + delta.x, m_dragOrigin.y, m_dragOrigin.z + delta.z);
                            m_dragHook.position = position;
                            break;
                        }
                    }

                }
                if(Input.GetMouseButtonUp(MouseButton))
                {
                    //Debug.Log("end drag");
                    m_dragging = false;
                    m_dragHook = null;
                    Cursor.visible = true;
                }
            }
            else if (Input.GetMouseButtonDown(MouseButton))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                bool foundDraggable = false;
                RaycastHit[] hits;
                hits = Physics.RaycastAll(ray, 100.0f);
                foreach(RaycastHit hit in hits)
                {
                    if(hit.transform.gameObject.tag == "Ground")
                    {
                        m_dragClickHit = hit.point;
                    }
                    else
                    {
                        if (foundDraggable) continue;
                        GameObject target = hit.transform.gameObject;
                        if (target.HasComponent<Draggable>())
                        {
                            //Debug.Log("begin drag");
                            m_dragging = true;
                            m_dragHook = target.GetComponent<Draggable>().GetDragHook();
                            m_mouseStart = Input.mousePosition;
                            m_dragOrigin = m_dragHook.position;
                            Cursor.visible = false;
                            foundDraggable = true;
                        }
                    }
                }
            }
        }
    }
}
