using System;
using System.Collections;
using System.Collections.Generic;
using PaintIn3D;
using UnityEngine;
using Object = UnityEngine.Object;

public class PaintTest : MonoBehaviour
{
    public Mesh mesh;
    public Material mat;
    private Camera _camera;
    private Material _material;
    private bool _canDraw = false;
    private Vector3 _hitPos;
    private Quaternion _hitRotation;
    public void OnPostRender() 
    {
        //
        if (_canDraw)
        {
            _material = mat;
            _material.SetPass(0);
            // draw mesh at the origin
            //Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity);
            Graphics.DrawMeshNow(mesh, _hitPos, _hitRotation);
        }
        
    }

    private void Start()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var hit = default(RaycastHit);

        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("hit");
                var position = hit.point;
                var rotation = Quaternion.LookRotation(-hit.normal); // Get the rotation of the paint. This should point TOWARD the surface we want to paint, so we use the inverse normal.

                _hitPos = position;
                _hitRotation = rotation;
                _canDraw = true;
            }
        }
    }

    void Draw()
    {
        //Debug.Log("Draw");
        // set first shader pass of the material
        _material = mat;
        _material.SetPass(0);
        // draw mesh at the origin
        //Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity);
        Graphics.DrawMeshNow(mesh, Vector3.zero, Quaternion.identity);
    }
    
    
    private static Mesh quadMesh;
    private static bool quadMeshSet;
    public static Mesh GetQuadMesh()
    {
        if (quadMeshSet == false)
        {
            var gameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
				
            quadMeshSet = true;
            quadMesh    = gameObject.GetComponent<MeshFilter>().sharedMesh;

            Object.DestroyImmediate(gameObject);
        }

        return quadMesh;
    }
    
    public static Vector4 IndexToVector(int index)
    {
        switch (index)
        {
            case 0: return new Vector4(1.0f, 0.0f, 0.0f, 0.0f);
            case 1: return new Vector4(0.0f, 1.0f, 0.0f, 0.0f);
            case 2: return new Vector4(0.0f, 0.0f, 1.0f, 0.0f);
            case 3: return new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
        }

        return default(Vector4);
    }
}
