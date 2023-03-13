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

        [SerializeField] public float ropeStiffness = 800.0f;

        [SerializeField] public float ropeDamping = 7.0f;

        [SerializeField] public float ropeLength = 2.0f;

        private void Awake()
        {
            InitializeRopes();
        }

        private void InitializeRopes()
        {
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

        private Rope CreateNetRope(int index)
        {
            var rope = Instantiate(ropePrefab, transform);
            var position = rootPoint.position;
            rope.rootPoint.position = position + new Vector3(position.x - 2.5f, position.y, position.z + index);
            rope.anchorPoint.position = position + new Vector3(position.x + 2.5f, position.y, position.z + index);
            rope.transform.parent = transform;
            rope.ropeStiffness = ropeStiffness;
            rope.ropeDamping = ropeDamping;
            rope.numberOfPoints = numberOfPointsPerRope;
            rope.totalLength = ropeLength;
            return rope;
        }
    }
}