using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code
{
    public class RopePoint : MonoBehaviour
    {
        public class PointState
        {
            public PointState()
            {
                Position = Vector3.zero;
                Velocity = Vector3.zero;
            }

            public PointState(Vector3 pos, Vector3 vel)
            {
                Position = pos;
                Velocity = vel;
            }

            public Vector3 Position;
            public Vector3 Velocity;

            public PointState Clone()
            {
                return new PointState(Position, Velocity);
            }
        }

        [SerializeField] private List<RopePoint> neighbors = null;

        public List<RopePoint> Neighbors => neighbors;

        public PointState State { get; set; }

        private Vector3 collisionForceToApply = Vector3.zero;
        public Vector3 Force { get; private set; }
        [FormerlySerializedAs("Mass")] public float mass = 1.0f;

        public void ClearForce()
        {
            Force = Vector3.zero;
        }

        public void ApplyForce(Vector3 force)
        {
            Force += force;
        }

        public void ApplyCollisionForce()
        {
            Force += collisionForceToApply;
            collisionForceToApply = Vector3.zero;
        }

        public void SaveState()
        {
            m_savedState = State.Clone();
        }

        public void LoadState()
        {
            State = m_savedState.Clone();
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (collision.rigidbody == null)
                return;
            collisionForceToApply = collision.relativeVelocity * collision.rigidbody.mass;
            collision.rigidbody.AddForce(-collisionForceToApply);
        }

        private PointState m_savedState = new PointState();

        //A note about this way of setting the position in Unity:
        //  This in not how you really would use Unity in most
        //  cases. Normally you use their physics engine, which 
        //  would handle this in a better way with interpolation 
        //  and other fancy features. This is only to get clean 
        //  access to Pos/Vel/Force in a consistent way.

        private void Awake()
        {
            neighbors = new List<RopePoint>();
            State = new RopePoint.PointState();
            //Get initial position
            State.Position = transform.position;
        }

        private void Update()
        {
            //Update graphical representation
            transform.position = State.Position;
        }

        public void LinkTo(RopePoint neighbor)
        {
            neighbor.neighbors.Add(this);
            neighbors.Add(neighbor);
        }
    }
}