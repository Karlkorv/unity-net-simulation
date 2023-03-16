//
//Filename: maxCamera.cs
//
// original: http://www.unifycommunity.com/wiki/index.php?title=MouseOrbitZoom
//
// --01-18-2010 - create temporary target, if none supplied at start

using UnityEngine;

namespace Code
{
    [AddComponentMenu("Camera-Control/3dsMax Camera Style")]
    public class MaxCamera : MonoBehaviour
    {
        public Transform target;
        public Vector3 targetOffset;
        public float distance = 5.0f;
        public float maxDistance = 20;
        public float minDistance = .6f;
        public float xSpeed = 200.0f;
        public float ySpeed = 200.0f;
        public int yMinLimit = -80;
        public int yMaxLimit = 80;
        public int zoomRate = 40;
        public float panSpeed = 0.3f;
        public float zoomDampening = 5.0f;

        private float m_xDeg = 0.0f;
        private float m_yDeg = 0.0f;
        private float m_currentDistance;
        private float m_desiredDistance;
        private Quaternion m_currentRotation;
        private Quaternion m_desiredRotation;
        private Quaternion m_rotation;
        private Vector3 m_position;

        private void Start() { Init(); }
        private void OnEnable() { Init(); }

        public void Init()
        {
            //If there is no target, create a temporary target at 'distance' from the cameras current viewpoint
            if (!target)
            {
                GameObject go = new GameObject("Cam Target");
                go.transform.position = transform.position + (transform.forward * distance);
                target = go.transform;
            }

            distance = Vector3.Distance(transform.position, target.position);
            m_currentDistance = distance;
            m_desiredDistance = distance;

            //be sure to grab the current rotations as starting points.
            m_position = transform.position;
            m_rotation = transform.rotation;
            m_currentRotation = transform.rotation;
            m_desiredRotation = transform.rotation;

            m_xDeg = Vector3.Angle(Vector3.right, transform.right);
            m_yDeg = Vector3.Angle(Vector3.up, transform.up);
        }

        /*
     * Camera logic on LateUpdate to only update after all character movement logic has been handled. 
     */
        private void LateUpdate()
        {
            // Scroll
            if (Input.GetMouseButton(1) && (Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)))
            {
                m_desiredDistance -= Input.GetAxis("Mouse Y") * Time.deltaTime * zoomRate * 0.125f * Mathf.Abs(m_desiredDistance);
            }
            else if (Input.GetMouseButton(1)) // If left-click => ORBIT
            {
                m_xDeg += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                m_yDeg -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

                ////////OrbitAngle

                //Clamp the vertical axis for the orbit
                m_yDeg = ClampAngle(m_yDeg, yMinLimit, yMaxLimit);
                // set camera rotation 
                m_desiredRotation = Quaternion.Euler(m_yDeg, m_xDeg, 0);
                m_currentRotation = transform.rotation;

                m_rotation = Quaternion.Lerp(m_currentRotation, m_desiredRotation, Time.deltaTime * zoomDampening);
                transform.rotation = m_rotation;
            }
            else if (Input.GetMouseButton(2)) // otherwise if middle mouse is selected, we pan by way of transforming the target in screenspace
            {
                //grab the rotation of the camera so we can move in a psuedo local XY space
                target.rotation = transform.rotation;
                target.Translate(Vector3.right * -Input.GetAxis("Mouse X") * panSpeed);
                target.Translate(transform.up * -Input.GetAxis("Mouse Y") * panSpeed, Space.World);
            }

            ////////Orbit Position

            // affect the desired Zoom distance if we roll the scrollwheel
            m_desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * zoomRate * Mathf.Abs(m_desiredDistance);
            //clamp the zoom min/max
            m_desiredDistance = Mathf.Clamp(m_desiredDistance, minDistance, maxDistance);
            // For smoothing of the zoom, lerp distance
            m_currentDistance = Mathf.Lerp(m_currentDistance, m_desiredDistance, Time.deltaTime * zoomDampening);

            // calculate position based on the new currentDistance 
            m_position = target.position - (m_rotation * Vector3.forward * m_currentDistance + targetOffset);
            transform.position = m_position;
        }

        private static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360)
                angle += 360;
            if (angle > 360)
                angle -= 360;
            return Mathf.Clamp(angle, min, max);
        }
    }
}