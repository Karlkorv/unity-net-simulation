using System;
using System.Collections.Generic;

namespace Code.Integrators
{
    public class LeapfrogIntegrator : INtegrator
    {
        public void Advance(List<RopePoint> points, Action<float> updateForcesFunc, float timeStep)
        {
            //This is not implemented yet, it is part of an optional task.
        }
    }
}