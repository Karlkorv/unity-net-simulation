using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Code
{
    public class NetPoint : MonoBehaviour
    {

        [SerializeField] private List<NetPoint> neighbors = null;

        public List<NetPoint> Neighbors => neighbors;

        private bool isAnchorPoint = false;
        
        public void ApplyForce(Vector3 force)
        {
            if (isAnchorPoint) return;
            GetComponent<Rigidbody>().AddForce(force);
        }
        
        public void SetAnchorPoint(bool anchor)
        {
            isAnchorPoint = anchor;
            GetComponent<Rigidbody>().isKinematic = anchor;
        }

        public Vector3 GetVelocity()
        {
            if (isAnchorPoint) return Vector3.zero;
            return GetComponent<Rigidbody>().velocity;
        }
        
        private void Awake()
        {
            neighbors = new List<NetPoint>();
        }

        public void LinkTo(NetPoint neighbor)
        {
            if (neighbors.Contains(neighbor)) return;
            neighbor.neighbors.Add(this);
            neighbors.Add(neighbor);
        }
    }
}