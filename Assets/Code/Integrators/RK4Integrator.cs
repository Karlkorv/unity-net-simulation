using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Integrators
{
    public class Rk4Integrator : INtegrator
    {    
        private struct Derivative
        {
            public void Reset()
            {
                DeltaPosition = Vector3.zero;
                DeltaVelocity = Vector3.zero;
            }

            public Vector3 DeltaPosition;
            public Vector3 DeltaVelocity;
        }

        private class CalcData
        {
            public Derivative EvalResult = new Derivative();
            public Derivative A = new Derivative();
            public Derivative B = new Derivative();
            public Derivative C = new Derivative();
            public Derivative D = new Derivative();
        }

        private List<CalcData> m_calcData = new List<CalcData>();
        private Derivative m_zero = new Derivative();

        public void Advance(List<RopePoint> points, Action<float> updateForcesFunc, float timeStep)
        {
            PrepareCalcData(points.Count);

            Evaluate(points, updateForcesFunc, 0.0f);
            for (int i = 0; i < points.Count; i++)
                m_calcData[i].A = m_calcData[i].EvalResult;

            Evaluate(points, updateForcesFunc, timeStep * 0.5f);
            for (int i = 0; i < points.Count; i++)
                m_calcData[i].B = m_calcData[i].EvalResult;

            Evaluate(points, updateForcesFunc, timeStep * 0.5f);
            for (int i = 0; i < points.Count; i++)
                m_calcData[i].C = m_calcData[i].EvalResult;

            Evaluate(points, updateForcesFunc, timeStep);
            for (int i = 0; i < points.Count; i++)
                m_calcData[i].D = m_calcData[i].EvalResult;

            for (int i = 0; i < points.Count; i++)
            {
                CalcData p = m_calcData[i];
                Vector3 deltaPos = (1.0f / 6.0f) * (p.A.DeltaPosition + 2.0f * (p.B.DeltaPosition + p.C.DeltaPosition) + p.D.DeltaPosition);
                Vector3 deltaVel = (1.0f / 6.0f) * (p.A.DeltaVelocity + 2.0f * (p.B.DeltaVelocity + p.C.DeltaVelocity) + p.D.DeltaVelocity);

                points[i].State.Position += deltaPos * timeStep;
                points[i].State.Velocity += deltaVel * timeStep;
            }
        }

        private void Evaluate(List<RopePoint> points, Action<float> updateForcesFunc, float timeStep)
        {
            for (int i = 0; i < points.Count; i++)
            {
                RopePoint point = points[i];
                Derivative derivative = m_calcData[i].EvalResult;

                point.SaveState();
                point.State.Position += derivative.DeltaPosition * timeStep;
                point.State.Velocity += derivative.DeltaVelocity * timeStep;
            }

            updateForcesFunc(timeStep);

            for (int i = 0; i < points.Count; i++)
            {
                RopePoint point = points[i];

                m_calcData[i].EvalResult.DeltaPosition = point.State.Velocity;
                m_calcData[i].EvalResult.DeltaVelocity = point.Force / point.mass;
            
                point.LoadState();
            }
        }

        private void PrepareCalcData(int numPoints)
        {
            int diff = numPoints - m_calcData.Count;

            //Do we need more? Create more! Never shrink
            if (diff > 0)
            {
                for (int i = 0; i < diff; i++)
                {
                    m_calcData.Add(new CalcData());
                }
            }

            for (int i = 0; i < numPoints; i++)
            {
                m_calcData[i].EvalResult.Reset();
            }
        }
    }
}