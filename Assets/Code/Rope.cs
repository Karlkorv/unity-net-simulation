using System;
using System.Collections.Generic;
using System.Linq;
using Code.Integrators;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code
{
    public class Rope : MonoBehaviour
    {
        [FormerlySerializedAs("m_ropePointPrefab")] [SerializeField]
        private RopePoint ropePointPrefab = null;

        [FormerlySerializedAs("m_rootPoint")] [SerializeField]
        public Transform rootPoint = null;

        [SerializeField] public Transform anchorPoint = null;

        [FormerlySerializedAs("m_groundPlanes")] [SerializeField]
        private Transform[] groundPlanes = null;

        [FormerlySerializedAs("m_groundStiffness")] [SerializeField]
        private float groundStiffness = 800.0f;

        [FormerlySerializedAs("m_groundDamping")] [SerializeField]
        private float groundDamping = 5.0f;

        [FormerlySerializedAs("m_sphereStiffness")] [SerializeField]
        private float sphereStiffness = 800.0f;

        [FormerlySerializedAs("m_sphereDamping")] [SerializeField]
        private float sphereDamping = 5.0f;

        [FormerlySerializedAs("m_numberOfPoints")] [SerializeField, Range(2, 200)]
        public int numberOfPoints = 10;

        [FormerlySerializedAs("m_totalLength")] [SerializeField, Range(0.1f, 10.0f)]
        public float totalLength = 2.0f;

        [FormerlySerializedAs("m_integratorType")] [SerializeField]
        private IntegratorType integratorType = IntegratorType.Euler;

        [FormerlySerializedAs("m_integratorTimeStep")] [SerializeField]
        private float integratorTimeStep = 1.0f / 60.0f;

        [FormerlySerializedAs("m_airFriction")] [SerializeField, Range(0, 5)]
        private float airFriction = 1.0f;

        [FormerlySerializedAs("m_ropeDamping")] [SerializeField]
        public float ropeDamping = 7.0f;

        [FormerlySerializedAs("m_ropeStiffness")] [SerializeField]
        public float ropeStiffness = 800.0f;

        [FormerlySerializedAs("m_showSimulationPoints")] [SerializeField]
        private bool showSimulationPoints = true;

        [SerializeField] private HashSet<Rope> linkedRopes = new HashSet<Rope>();

        [FormerlySerializedAs("m_ropeMass")] [SerializeField]
        public float ropeMass = 1.0f;

        private int m_previousNumberOfPoints;
        private List<RopePoint> m_points = null;
        private float m_accumulator = 0.0f;
        private bool m_prevShowSimulationPoints = true;
        private Dictionary<IntegratorType, INtegrator> m_integrators = new Dictionary<IntegratorType, INtegrator>();
        private RopeMesh m_meshGenerator = new RopeMesh();

        private void Start()
        {
            m_integrators.Add(IntegratorType.Euler, new EulerIntegrator());
            m_integrators.Add(IntegratorType.Leapfrog, new LeapfrogIntegrator());
            m_integrators.Add(IntegratorType.Rk4, new Rk4Integrator());

            RecreateRopePoints();
        }

        private void Update()
        {
            m_accumulator += Mathf.Min(Time.deltaTime / integratorTimeStep, 3.0f);

            if (m_previousNumberOfPoints != numberOfPoints)
            {
                RecreateRopePoints();
            }

            if (showSimulationPoints != m_prevShowSimulationPoints)
            {
                foreach (var pointRenderer in m_points.Select(x => x.GetComponent<MeshRenderer>()))
                    pointRenderer.enabled = showSimulationPoints;
                m_prevShowSimulationPoints = showSimulationPoints;
            }

            while (m_accumulator > 1.0f)
            {
                m_accumulator -= 1.0f;

                AdvanceSimulation();
            }

            //m_meshGenerator.GenerateMesh(GetComponent<MeshFilter>().mesh,
            //m_points.Select(p => p.transform.localPosition).ToList(), false);
        }

        private void ApplyForces(float timeStep)
        {
            ClearAndApplyGravity();
            ApplyGroundForces();
            ApplyAirFriction();
            ApplySpringForces();
            ApplyCollisionForces();
            ConstraintAnchorPoints();
        }

        private void ClearAndApplyGravity()
        {
            foreach (var point in m_points)
            {
                point.ClearForce();
                point.ApplyForce(Physics.gravity * point.mass);
            }
        }

        private void ApplyCollisionForces()
        {
            foreach (var point in m_points)
            {
                point.ApplyCollisionForce();
            }
        }

        private void ApplyGroundForces()
        {
            if (groundPlanes == null)
                return;

            foreach (var ground in groundPlanes)
            {
                Vector3 groundNormal = ground.rotation * Vector3.up;

                foreach (var point in m_points)
                {
                    Vector3 groundToPoint = point.State.Position - ground.position;
                    float distToGround = Vector3.Dot(groundNormal, groundToPoint);
                    float radius = point.transform.localScale.x * 0.5f;

                    if (distToGround < radius)
                    {
                        float penetrationDepth = radius - distToGround;

                        //Spring force outwards
                        point.ApplyForce(groundStiffness * penetrationDepth * groundNormal);
                        //Damping
                        point.ApplyForce(-groundDamping * point.State.Velocity);
                    }
                }
            }
        }

        private void ConstraintAnchorPoints()
        {
            if (rootPoint != null)
            {
                m_points[0].State.Velocity = Vector3.zero;
                m_points[0].State.Position = rootPoint.transform.position;
            }

            if (anchorPoint != null)
            {
                m_points[^1].State.Velocity = Vector3.zero;
                m_points[^1].State.Position = anchorPoint.transform.position;
            }
        }

        private void ApplyAirFriction()
        {
            foreach (var point in m_points)
            {
                //Air friction
                point.ApplyForce(-point.State.Velocity * airFriction);
            }
        }

        private void ApplySpringForces()
        {
            float segmentLength = totalLength / (numberOfPoints - 1);


            for (int i = 0; i < numberOfPoints - 1; i++)
            {
                var p1 = m_points[i];
                foreach (var p2 in m_points[i].Neighbors)
                {
                    float relativeDistanceDiff = (p2.State.Position - p1.State.Position).magnitude - segmentLength;
                    Vector3 springForce = ropeStiffness * relativeDistanceDiff *
                                          (p2.State.Position - p1.State.Position).normalized;
                    Vector3 dampingForce = -ropeDamping * (p2.State.Velocity - p1.State.Velocity).magnitude *
                                           (p2.State.Velocity - p1.State.Velocity).normalized;
                    Vector3 totalForce = springForce - dampingForce;
                    p1.ApplyForce(totalForce);
                    p2.ApplyForce(-totalForce);
                }
            }
        }

        public void LinkTo(Rope rope)
        {
            if (rope.numberOfPoints != numberOfPoints)
                throw new Exception("Ropes must have the same number of points to be linked");
            for (int i = 0; i < numberOfPoints; i++)
            {
                m_points[i].LinkTo(rope.m_points[i]);
            }

            linkedRopes.Add(rope);
        }

        public void RecreateRopePoints()
        {
            if (m_points != null)
            {
                foreach (var point in m_points)
                {
                    Destroy(point.gameObject);
                }
            }

            m_points = new List<RopePoint>();
            float segmentLength = totalLength / (numberOfPoints - 1);

            List<Vector3> points = new List<Vector3>();
            Vector3 dirBetweenPoints = (rootPoint.position - anchorPoint.position).normalized;
            for (int i = 0; i < numberOfPoints; i++)
            {
                points.Add(rootPoint.position + dirBetweenPoints * (i * segmentLength));
            }

            for (int i = 0; i < numberOfPoints; i++)
            {
                RopePoint point = (RopePoint)Instantiate(ropePointPrefab,
                    points[i], Quaternion.identity);
                point.transform.parent = transform;
                point.mass = ropeMass / numberOfPoints;
                m_points.Add(point);
                if (i > 0)
                    m_points[i - 1].LinkTo(m_points[i]);
            }

            foreach (var rope in linkedRopes)
            {
                LinkTo(rope);
            }

            m_previousNumberOfPoints = numberOfPoints;
            // if (GetComponent<MeshFilter>().mesh == null)
            // GetComponent<MeshFilter>().mesh = new Mesh();
            // m_meshGenerator.GenerateMesh(GetComponent<MeshFilter>().mesh,
            // m_points.Select(p => p.transform.localPosition).ToList(), true);
            m_prevShowSimulationPoints = !showSimulationPoints; //Make sure points are enabled/disabled
        }

        public void SetDragHook(Transform hook)
        {
            foreach (var ropePoint in m_points)
            {
                ropePoint.GetComponent<Draggable>().SetDragHook(hook);
            }
        }

        private void AdvanceSimulation()
        {
            m_integrators[integratorType].Advance(m_points, ApplyForces, integratorTimeStep);
        }
    }
}