using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DrawLineByBox : MonoBehaviour
    {
        public Material line;
        public bool gravity;
        public Color lineColor = Color.white;
        public Camera mainCamera;
        
        private readonly List<GameObject> _lineList = new List<GameObject>();
        private readonly List<Vector2> _pointList = new List<Vector2>();
        private GameObject _currentLine;
        private LineRenderer _currentLineRenderer;

        private bool _stopHolding;
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _pointList.Clear();
                CreateLine();
            }

            if (Input.GetMouseButton(0))
            {
                Vector2 item = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                if (!_pointList.Contains(item))
                {
                    _pointList.Add(item);
                    _currentLineRenderer.positionCount = _pointList.Count;
                    _currentLineRenderer.SetPosition(_pointList.Count - 1, _pointList.Last());
                    if (_pointList.Count >= 2)
                    {
                        Vector2 vector1 = _pointList[_pointList.Count - 2];
                        Vector2 vector2 = _pointList[_pointList.Count - 1];

                        GameObject currentCollierObject = new GameObject("Collider");
                        currentCollierObject.transform.position = (vector1 + vector2) / 2f;
                        currentCollierObject.transform.right = (vector2 - vector1).normalized;
                        currentCollierObject.transform.parent = _currentLine.transform;
                        BoxCollider2D currentBoxCollider2D = currentCollierObject.AddComponent<BoxCollider2D>();
                        currentBoxCollider2D.size = new Vector3((vector2 - vector1).magnitude, 0.1f, 0.1f);
                        currentBoxCollider2D.enabled = false;
                    }
                }
            }
            
            if (Input.GetMouseButtonUp(0))
            {
                if (_currentLine.transform.childCount > 0)
                {
                    for (int i = 0; i < _currentLine.transform.childCount; i++)
                    {
                        _currentLine.transform.GetChild(i).GetComponent<BoxCollider2D>().enabled = true;
                    }

                    _lineList.Add(_currentLine);

                    if (gravity)
                    {
                        _currentLine.AddComponent<Rigidbody2D>().useAutoMass = true;
                    }
                }
                else
                {
                    Destroy(_currentLine);
                }
            }
        }

        private void CreateLine()
        {
            _currentLine = new GameObject("Line");
            _currentLineRenderer = _currentLine.AddComponent<LineRenderer>();
            _currentLineRenderer.material = line;
            _currentLineRenderer.material.EnableKeyword("_EMISSION");
            _currentLineRenderer.material.SetColor(EmissionColor, this.lineColor);
            _currentLineRenderer.positionCount = 0;
            _currentLineRenderer.startWidth = 0.1f;
            _currentLineRenderer.endWidth = 0.1f;
            _currentLineRenderer.startColor = lineColor;
            _currentLineRenderer.endColor = lineColor;
            _currentLineRenderer.useWorldSpace = false;
        }

        // public void ClearAll()
        // {
        //     foreach (var obj in _lineList)
        //     {
        //         Destroy(obj);
        //     }
        //     _lineList.Clear();
        // }
    }