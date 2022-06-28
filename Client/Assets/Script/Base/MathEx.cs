using UnityEngine;


namespace Utilities.Math
{
    public class QuaternionEx
    {
        public static Quaternion Add(Quaternion q1, Quaternion q2)
        {
            return new Quaternion(q1.x + q2.x, q1.y + q2.y, q1.z + q2.z, q1.w + q2.w);
        }

        public static Quaternion Scale(Quaternion q, float t)
        {
            return new Quaternion(q.x * t, q.y * t, q.z * t, q.w * t);
        }

        public static Quaternion Exp(Quaternion q)
        {
            float a = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z);
            float sina = Mathf.Sin(a);
            float cosa = Mathf.Cos(a);
            float coeff = Mathf.Abs(sina) < 0.001f ? 1f : sina / a;
            return new Quaternion(coeff * q.x, coeff * q.y, coeff * q.z, cosa);
        }

        public static Quaternion Log(Quaternion q)
        {
            if (Mathf.Abs(q.w) < 1f)
            {
                float a = Mathf.Acos(q.w);
                float sina = Mathf.Sin(a);

                if (Mathf.Abs(sina) > 0.001f)
                {
                    float coeff = a / sina;
                    return new Quaternion(coeff * q.x, coeff * q.y, coeff * q.z, 0f);
                }
            }

            return new Quaternion(q.x, q.y, q.z, 0f);
        }

        public static Quaternion InnerQuadrangle(Quaternion q0, Quaternion q1, Quaternion q2)
        {
            Quaternion q1_1 = Quaternion.Inverse(q1);
            Quaternion qln = Add(Log(q1_1 * q2), Log(q1_1 * q0));
            Quaternion qexp = Exp(Scale(qln, -0.25f));
            return q1 * qexp;
        }

        public static Quaternion Squad(Quaternion q1, Quaternion a1, Quaternion a2, Quaternion q2, float t)
        {
            t = Mathf.Clamp01(t);
            Quaternion p1 = Quaternion.Slerp(q1, q2, t);
            Quaternion p2 = Quaternion.Slerp(a1, a2, t);
            return Quaternion.Slerp(p1, p2, 2 * t * (1 - t));
        }
    }


    public class BezierCurve
    {
        public Vector3 from { get; private set; }
        public Vector3 to { get; private set; }
        public Vector3 p1 { get; private set; }
        public Vector3 p2 { get; private set; }

        public BezierCurve(Vector3 from, Vector3 p1, Vector3 p2, Vector3 to)
        {
            this.from = from;          
            this.p1 = p1;
            this.p2 = p2;
            this.to = to;
        }

        public Vector3 this[float t]
        {
            get
            {
                return from * (1f - t) * (1f - t) * (1f - t)
                    + p1 * 3f * t * (1f - t) * (1f - t)
                    + p2 * 3f * t * t * (1f - t)
                    + to * t * t * t;
            }
        }

        /// <summary>
        /// 输出采样点，包括起点和终点
        /// </summary>
        /// <param name="pointCount"> 采样点数量，必须大于等于2（含起点和终点） </param>
        /// <returns> 采样数组 </returns>
        public Vector3[] SampleToArray(int pointCount)
        {
            if (pointCount <= 2)
            {
                return new Vector3[2] { from, to };
            }

            Vector3[] sampline = new Vector3[pointCount];
            sampline[0] = from;
            sampline[pointCount - 1] = to;

            for (int i = 1; i < pointCount - 1; ++i)
            {
                sampline[i] = this[(float)i / (pointCount - 1)];
            }

            return sampline;
        }

        public SamplineCurve Sample(int pointCount)
        {
            SamplineCurve curve;

            if (pointCount <= 2)
            {
                curve = new SamplineCurve(2);
                curve[0] = from;
                curve[1] = to;
            }
            else
            {
                curve = new SamplineCurve(pointCount);
                curve[0] = from;
                curve[pointCount - 1] = to;

                for (int i = 1; i < pointCount - 1; ++i)
                {
                    curve[i] = this[(float)i / (pointCount - 1)];
                }
            }

            curve.Refresh();
            return curve;
        }

        public static void CalculateControlPoint(Vector3 left, Vector3 from, Vector3 to, Vector3 right, out Vector3 pt1, out Vector3 pt2)
        {
            Vector3 dir = to - from;
            Vector3 dir1 = to - left;
            Vector3 dir2 = from - right;
            float len = dir.magnitude;

            if ((left - from).sqrMagnitude < 0.0001f)
            {
                pt1 = from + dir / 3f;
            }
            else
            {
                pt1 = from + Vector3.ClampMagnitude(dir1 / 6f, len / 2f);
            }

            if ((right - to).sqrMagnitude < 0.0001f)
            {
                pt2 = to - dir / 3f;
            }
            else
            {
                pt2 = to + Vector3.ClampMagnitude(dir2 / 6f, len / 2f);
            }
        }
    }


    public class SamplineCurve
    {
        public Vector3[] corners { get; private set; }
        public int cornersCount { get; private set; }
        public float curveLength { get; private set; }

        private float[] lengths;

        public SamplineCurve(int cornersCount)
        {
            if (cornersCount < 2)
            {
                DEBUG.LogError("Can't construct a sampline curve with less then 2 corners!");
                return;
            }

            this.cornersCount = cornersCount;
            this.corners = new Vector3[cornersCount];
        }

        public Vector3 this[int index]
        {
            get { return corners[index]; }
            set { corners[index] = value; }
        }

        public void Refresh()
        {
            lengths = new float[corners.Length];
            lengths[0] = 0f;

            for (int i = 1; i < corners.Length; ++i)
            {
                lengths[i] = lengths[i - 1] + (corners[i] - corners[i - 1]).magnitude;
            }

            curveLength = lengths[lengths.Length - 1];
        }

        public Vector3 Locate(float len)
        {
            if (len <= 0f)
            {
                return corners[0];
            }
            else if (len >= curveLength)
            {
                return corners[corners.Length - 1];
            }
            else
            {
                return Locate(0, corners.Length - 1, len);
            }
        }

        private Vector3 Locate(int start, int end, float len)
        {
            if (start >= end - 1)
            {
                return Vector3.Lerp(corners[start], corners[end], Mathf.InverseLerp(lengths[start], lengths[end], len));
            }
            else
            {
                int center = (start + end) / 2;

                if (len < lengths[center])
                {
                    return Locate(start, center, len);
                }
                else
                {
                    return Locate(center, end, len);
                }
            }
        }
    }
}