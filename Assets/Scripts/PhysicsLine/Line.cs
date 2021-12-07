using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class Line : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public EdgeCollider2D edgeCollider;
    public Rigidbody2D rigidBody;
    
    [HideInInspector] public List<Vector2> points = new List<Vector2>();
    [HideInInspector] public int pointCount = 0;

    //点与点之间的最小距离
    private float _pointsMinDistance = 0.1f;
    private float _circleColliderRadius;
    
    public void AddPoint(Vector2 newPoint)
    {
        if (pointCount >= 1 && Vector2.Distance(newPoint, GetLineRendererLastPoint()) < _pointsMinDistance)
        {
            return;
        }
        
        points.Add(newPoint);
        ++pointCount;
        
        //add circle collider
        var circleCollider = gameObject.AddComponent<CircleCollider2D>();
        circleCollider.offset = newPoint;
        circleCollider.radius = _circleColliderRadius;
        
        //set LineRenderer
        lineRenderer.positionCount = pointCount;
        //设置新添加点的位置
        lineRenderer.SetPosition(pointCount - 1, newPoint);
        
        //set EdgeCollider
        if (pointCount > 1)
        {
            edgeCollider.points = points.ToArray();
        }
        
    }

    private Vector3 GetLineRendererLastPoint()
    {
        return lineRenderer.GetPosition(pointCount - 1);
    }

    public void UsePhysics(bool usePhysics)
    {
        rigidBody.isKinematic = !usePhysics;
    }

    public void SetLineColor(Gradient color)
    {
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.colorGradient = color;
        // lineRenderer.startColor = c1; 
        // lineRenderer.endColor = c2;
    }

    public void SetPointMinDistance(float distance)
    {
        _pointsMinDistance = distance;
    }

    public void SetLineWidth(float width)
    {
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;

        _circleColliderRadius = width / 2f;
        edgeCollider.edgeRadius = _circleColliderRadius;
    }
}
