using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code
{
    /// <summary>
    /// A net script for tying multiple ropes together.
    /// The net connections are not rendered but behave exactly the same as the rope connections.
    /// Author: Janne Schyffert
    /// </summary>
    public class Net : MonoBehaviour
    {
        [SerializeField] public Transform rootPoint = null;

        [SerializeField] public NetPoint netPointPrefab = null;

        [FormerlySerializedAs("ropeMass")] [SerializeField]
        public float netMass = 1.0f;

        [FormerlySerializedAs("ropeStiffness")] [SerializeField]
        public float netStiffness = 800.0f;

        [FormerlySerializedAs("ropeDamping")] [SerializeField]
        public float netDamping = 7.0f;

        [SerializeField] public float sideLength = 5.0f;

        [FormerlySerializedAs("numberOfPointsSide")] [SerializeField]
        public int numberOfPointsPerSide = 5;

        // Columns are "ropes" from anchor to anchor
        private NetPoint[,] points;
        private int prevPointsPerSide;
        private float prevRopeMass;
        private float prevSideLength;
        private Transform prevRootPoint;

        private void Awake()
        {
            InitializePoints();
        }

        private void FixedUpdate()
        {
            if (ValuesHasChanged())
            {
                InitializePoints();
            }

            ApplySpringForces();
        }

        private void ApplySpringForces()
        {
            var segmentLength = sideLength / numberOfPointsPerSide;
            foreach (var point in points)
            {
                foreach (var neighbor in point.Neighbors)
                {
                    var neighborPos = neighbor.GetComponent<Rigidbody>().position;
                    var pointPos = point.GetComponent<Rigidbody>().position;
                    float relativeDistanceDiff = (neighborPos - pointPos).magnitude - segmentLength;
                    Vector3 springForce = netStiffness * relativeDistanceDiff *
                                          (neighborPos - pointPos).normalized;
                    Vector3 dampingForce = netDamping * (neighbor.GetVelocity() - point.GetVelocity());
                    Vector3 totalForce = springForce + dampingForce;
                    point.ApplyForce(totalForce);
                    neighbor.ApplyForce(-totalForce);
                }
            }
        }

        private void InitializePoints()
        {
            SaveCurrentValues();
            if (points != null)
            {
                foreach (var point in points)
                {
                    Destroy(point.gameObject);
                }
            }

            points = new NetPoint[numberOfPointsPerSide, numberOfPointsPerSide];
            var segmentLength = sideLength / numberOfPointsPerSide;

            // Initialize
            for (int i = 0; i < numberOfPointsPerSide; i++)
            {
                for (int j = 0; j < numberOfPointsPerSide; j++)
                {
                    Vector3 position = rootPoint.position + new Vector3(segmentLength * i, 0, segmentLength * j);
                    var point = Instantiate(netPointPrefab, position, Quaternion.identity);
                    point.transform.parent = transform;
                    point.GetComponent<Rigidbody>().mass = netMass / (numberOfPointsPerSide ^ 2);

                    if (IsCorner(i, j))
                        point.SetAnchorPoint(true);

                    points[i, j] = point;
                }
            }

            // Link
            for (int i = 0; i < numberOfPointsPerSide; i++)
            {
                for (int j = 0; j < numberOfPointsPerSide; j++)
                {
                    if (i > 0)
                    {
                        points[i, j].LinkTo(points[i - 1, j]);
                    }

                    if (j > 0)
                    {
                        points[i, j].LinkTo(points[i, j - 1]);
                    }
                }
            }
        }

        private bool IsCorner(int i, int j)
        {
            var returnValue = false;
            returnValue |= i == 0 && j == 0;
            returnValue |= i == 0 && j == numberOfPointsPerSide - 1;
            returnValue |= i == numberOfPointsPerSide - 1 && j == 0;
            returnValue |= i == numberOfPointsPerSide - 1 && j == numberOfPointsPerSide - 1;
            return returnValue;
        }


        private void SaveCurrentValues()
        {
            prevRootPoint = rootPoint;
            prevSideLength = sideLength;
            prevPointsPerSide = numberOfPointsPerSide;
            prevSideLength = sideLength;
            prevRopeMass = netMass;
        }

        private bool ValuesHasChanged()
        {
            var returnBool = prevSideLength != sideLength;
            returnBool |= prevRootPoint != rootPoint;
            returnBool |= prevPointsPerSide != numberOfPointsPerSide;
            returnBool |= prevRopeMass != netMass;
            return returnBool;
        }
    }
}