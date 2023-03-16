using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code
{
    /// <summary>
    /// A net script for tying multiple ropes together.
    /// The net connections are not rendered but behave exactly the same as the rope connections.
    /// Author: Janne Schyffert
    /// </summary>
    public class Net : MonoBehaviour
    {
        [SerializeField] public int numberOfRopesPerSide = 10;

        [SerializeField] public int numberOfPointsPerRope = 10;

        [SerializeField] public Transform rootPoint = null;

        [SerializeField] public List<Rope> ropes = null;

        [SerializeField] public Rope ropePrefab = null;

        [SerializeField] public float ropeMass = 1.0f;

        [SerializeField] public float ropeStiffness = 800.0f;

        [SerializeField] public float ropeDamping = 7.0f;

        [SerializeField] public float sideLength = 5.0f;

        [SerializeField] public float collisionDamping = 0.5f;

        private int prevRopesPerSide;
        private int prevPointsPerRope;
        private float prevRopeMass;
        private float prevRopeStiffness;
        private float prevRopeDamping;
        private float prevSideLength;
        private Transform prevRootPoint;
        private float prevDamping;

        private void Awake()
        {
            InitializeRopes();
        }

        private void Update()
        {
            if (RopeValuesHasChanged())
            {
                InitializeRopes();
            }
        }

        private void InitializeRopes()
        {
            SaveCurrentValues();
            if (ropes != null)
            {
                foreach (var rope in ropes)
                {
                    Destroy(rope.gameObject);
                }
            }

            ropes = new List<Rope>();

            for (var i = 0; i < numberOfRopesPerSide; i++)
            {
                var rope = CreateNetRope(i);
                ropes.Add(rope);
                rope.RecreateRopePoints();
                rope.SetDragHook(rootPoint);
                if (i > 0)
                    ropes[i].LinkTo(ropes[i - 1]);
            }
        }

        private void SaveCurrentValues()
        {
            prevRootPoint = rootPoint;
            prevRopesPerSide = numberOfRopesPerSide;
            prevPointsPerRope = numberOfPointsPerRope;
            prevRopeMass = ropeMass;
            prevRopeStiffness = ropeStiffness;
            prevRopeDamping = ropeDamping;
            prevSideLength = sideLength;
            prevDamping = collisionDamping;
        }

        private bool RopeValuesHasChanged()
        {
            var returnBool = false;
            returnBool |= prevRopesPerSide != numberOfRopesPerSide;
            returnBool |= prevPointsPerRope != numberOfPointsPerRope;
            returnBool |= prevRopeMass != ropeMass;
            returnBool |= prevRopeStiffness != ropeStiffness;
            returnBool |= prevRopeDamping != ropeDamping;
            returnBool |= prevSideLength != sideLength;
            returnBool |= prevRootPoint != rootPoint;
            returnBool |= prevDamping != collisionDamping;
            return returnBool;
        }

        private Rope CreateNetRope(int index)
        {
            var segmentLength = sideLength / numberOfRopesPerSide;
            var position = rootPoint.position;
            var rope = Instantiate(ropePrefab, transform);
            rope.rootPoint.position = position + new Vector3(position.x - sideLength / 2, position.y,
                position.z + segmentLength * index);
            rope.anchorPoint.position = position + new Vector3(position.x + sideLength / 2, position.y,
                position.z + segmentLength * index);
            rope.transform.parent = transform;
            rope.ropeStiffness = ropeStiffness;
            rope.ropeDamping = ropeDamping;
            rope.ropeMass = ropeMass;
            rope.numberOfPoints = numberOfPointsPerRope;
            rope.totalLength = sideLength;
            rope.collisionDamping = collisionDamping;

            return rope;
        }
    }
}