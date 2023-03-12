using System.Collections.Generic;
using UnityEngine;

namespace Code
{
    public class Net : MonoBehaviour
    {
        [SerializeField] public int numberOfRopesPerSide = 10;

        [SerializeField] public int numberOfPointsPerRope = 10;

        [SerializeField] public Transform rootPoint = null;

        [SerializeField] public Transform anchorPoint = null;

        [SerializeField] public List<Rope> ropes = null;

        [SerializeField] public Rope ropePrefab = null;

        [SerializeField] public float ropeStiffness = 800.0f;

        [SerializeField] public float ropeDamping = 7.0f;

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

            for (int i = 0; i < numberOfRopesPerSide; i++)
            {
                var rope = Instantiate(ropePrefab, transform);
                var position = rootPoint.position;
                rope.rootPoint.position = position + new Vector3(0, 0, i);
                rope.anchorPoint.position = position + new Vector3(5, 0, i);
                rope.transform.parent = transform;
                rope.ropeStiffness = ropeStiffness;
                rope.ropeDamping = ropeDamping;
                rope.numberOfPoints = numberOfPointsPerRope;
                rope.anchorPoint = anchorPoint;
                ropes.Add(rope);
                rope.RecreateRopePoints();
                if (i > 0)
                    ropes[i].LinkTo(ropes[i - 1]);
            }
        }
    }
}