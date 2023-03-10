using System;
using System.Collections.Generic;

namespace Code.Integrators
{
    public class EulerIntegrator : INtegrator
    {
        public void Advance(List<RopePoint> points, Action<float> updateForcesFunc, float timeStep)
        {
            updateForcesFunc(timeStep);

            foreach (var point in points)
            {
                point.State.Velocity += (timeStep / point.mass) * point.Force;
                point.State.Position += timeStep * point.State.Velocity;
            }
        }
    }
}
