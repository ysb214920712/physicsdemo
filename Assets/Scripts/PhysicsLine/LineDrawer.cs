using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
    public GameObject linePrefab;
    public LayerMask cantDrawOverLayer;
    private int _cantDrawOverLayerIndex;

    [Space(30)] public Gradient lineColor;
    public float linePointsMinDistance;
    public float lineWidth;

    private Line _currentLine;
    private Camera _camera;

    private void Start()
    {
        _camera = Camera.main;
        _cantDrawOverLayerIndex = LayerMask.NameToLayer("CantDrawOver");
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            BeginDraw();
        }

        if (_currentLine != null)
        {
            Draw();
        }

        if (Input.GetMouseButtonUp(0))
        {
            EndDraw();
        }
    }

    private void BeginDraw()
    {
        _currentLine = Instantiate(linePrefab, transform).GetComponent<Line>();
        _currentLine.UsePhysics(false);
        _currentLine.SetLineColor(lineColor);
        _currentLine.SetPointMinDistance(linePointsMinDistance);
        _currentLine.SetLineWidth(lineWidth);
    }

    private void Draw()
    {
        var pos = _camera.ScreenToWorldPoint(Input.mousePosition);
        //防止线与线交叉
        RaycastHit2D hit = Physics2D.CircleCast(pos, lineWidth / 3f, Vector2.zero, 1f, cantDrawOverLayer);
        if (hit)
        {
            EndDraw();
        }
        else
        {
            _currentLine.AddPoint(pos);
        }
    }

    private void EndDraw()
    {
        if (_currentLine == null)
        {
            return;
        }

        if (_currentLine.pointCount < 2)
        {
            Destroy(_currentLine.gameObject);
        }
        else
        {
            _currentLine.gameObject.layer = _cantDrawOverLayerIndex;
            _currentLine.UsePhysics(true);
            _currentLine = null;
        }
    }
}
