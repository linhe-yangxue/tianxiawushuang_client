using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Utilities.Routines;
using Utilities.Math;


public class MovementCorner
{
    public Vector3 position;
    public Quaternion rotation;
    public float speed;
}


public class MovementPath
{
    public MovementCorner[] corners;

    public void Refresh()
    {
        if (corners != null && corners.Length >= 2)
        {
            for (int i = 1; i < corners.Length; ++i)
            {
                if (Quaternion.Dot(corners[i - 1].rotation, corners[i].rotation) < 0f)
                {
                    corners[i].rotation = QuaternionEx.Scale(corners[i].rotation, -1f);
                }
            }
        }
    }
}


public class MovementRoutine : Routine
{
    public Transform target { get; private set; }
    public MovementPath path { get; private set; }
    public int currentFromCornerIndex { get; private set; }

    public event Action<int, float> onMoving;
    public event Action<int> onCorner;
    
    private BezierMoveRoutine currentMoveRoutine;

    public float currentSegmentFactor
    {
        get 
        {
            return currentMoveRoutine == null ? 0f : currentMoveRoutine.factor;
        }
    }

    public MovementRoutine(Transform target, MovementPath path)
    {
        this.target = target;
        this.path = path;
        this.currentFromCornerIndex = 0;
        Bind(DoMovement());
    }

    private IEnumerator DoMovement()
    {
        if (target == null || path == null || path.corners == null || path.corners.Length < 2)
        {
            yield break;
        }

        RoutineMonitor.KeepExclusively(this, target.gameObject);

        Append(OnMoving());

        float marginLength = 0f;

        while (currentFromCornerIndex < path.corners.Length - 1)
        {
            if (onCorner != null)
            {
                onCorner(currentFromCornerIndex);
            }

            MovementCorner from = path.corners[currentFromCornerIndex];
            MovementCorner to = path.corners[currentFromCornerIndex + 1];
            MovementCorner left = currentFromCornerIndex == 0 ? from : path.corners[currentFromCornerIndex - 1];
            MovementCorner right = currentFromCornerIndex == path.corners.Length - 2 ? to : path.corners[currentFromCornerIndex + 2];
            currentMoveRoutine = new BezierMoveRoutine(target, left, from, to, right);
            currentMoveRoutine.startLength = marginLength;
            yield return currentMoveRoutine;
            marginLength = currentMoveRoutine.marginLength;
            ++currentFromCornerIndex;
        }

        currentFromCornerIndex = path.corners.Length - 1;
        currentMoveRoutine = null;

        if (onMoving != null)
        {
            onMoving(currentFromCornerIndex, 0f);
        }

        if (onCorner != null)
        {
            onCorner(currentFromCornerIndex);
        }
    }

    private IEnumerator OnMoving()
    {
        while (true)
        {
            if (onMoving != null)
            {
                onMoving(currentFromCornerIndex, currentSegmentFactor);
            }

            yield return null;
        }
    }
}


public class BezierMoveRoutine : Routine
{
    public Transform target { get; private set; }
    public MovementCorner from { get; private set; }
    public MovementCorner to { get; private set; }
    public float factor { get; private set; }
    public float startLength { get; set; }
    public float marginLength { get; private set; }

    private MovementCorner left;
    private MovementCorner right;

    public BezierMoveRoutine(Transform target, MovementCorner left, MovementCorner from, MovementCorner to, MovementCorner right)
    {
        this.target = target;
        this.from = from;
        this.to = to;
        this.left = left;
        this.right = right;
        this.factor = 0f;
        Bind(DoBezierMove());
    }

    private IEnumerator DoBezierMove()
    {
        Vector3 pt1, pt2;
        BezierCurve.CalculateControlPoint(left.position, from.position, to.position, right.position, out pt1, out pt2);
        BezierCurve bezier = new BezierCurve(from.position, pt1, pt2, to.position);
        float len = (to.position - from.position).magnitude;
        int count = Mathf.Clamp((int)len * 5, 10, 100);
        SamplineCurve sampline = bezier.Sample(count);       
        Quaternion a1 = QuaternionEx.InnerQuadrangle(left.rotation, from.rotation, to.rotation);
        Quaternion a2 = QuaternionEx.InnerQuadrangle(from.rotation, to.rotation, right.rotation);
        float length = 0f;
        float speed = from.speed;

        if (startLength < 0.001f)
        {
            target.position = from.position;
            target.rotation = from.rotation;
            factor = 0f;
        }
        else 
        {
            length = startLength;
            factor = length / sampline.curveLength;
            target.position = sampline.Locate(length);
            target.rotation = QuaternionEx.Squad(from.rotation, a1, a2, to.rotation, factor);
            speed = Mathf.Lerp(from.speed, to.speed, factor);
        }

        while (length < sampline.curveLength)
        {
            yield return null;         
            length += speed * Time.deltaTime;
            factor = length / sampline.curveLength;
            target.position = sampline.Locate(length);
            target.rotation = QuaternionEx.Squad(from.rotation, a1, a2, to.rotation, factor);
            speed = Mathf.Lerp(from.speed, to.speed, factor);
        }

        target.position = to.position;
        target.rotation = to.rotation;
        factor = 1f;
        marginLength = length - sampline.curveLength;
    }
}