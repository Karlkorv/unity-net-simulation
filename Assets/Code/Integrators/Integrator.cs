using System;
using System.Collections.Generic;

namespace Code.Integrators
{
    public enum IntegratorType
    { 
        Euler,
        Leapfrog,
        Rk4
    }

    public interface INtegrator
    {
        void Advance(List<RopePoint> points, Action<float> updateForcesFunc, float timeStep);
    }
}