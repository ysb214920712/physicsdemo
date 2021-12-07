using System;
using System.Collections;
using System.Collections.Generic;
using PaintIn3D;
using UnityEngine;

public class GeneratePaintableMesh : MonoBehaviour
{
    public Material Material
    {
        get => material;

        set => material = value;
    }
    
    [SerializeField] private Material material;
    
    public float Size
    {
        get => size;

        set => size = value;
    }
    
    [SerializeField] private float size = 1.5f;

    private Mesh _mesh;

    private void Awake()
    {
        UpdateMesh();

        gameObject.AddComponent<MeshFilter>().sharedMesh = _mesh;
        gameObject.AddComponent<MeshCollider>().sharedMesh = _mesh;
        gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
        
        //添加paint所需要的组件
        gameObject.AddComponent<P3dPaintable>();
        //复制第一个材质
        var materialCloner = gameObject.AddComponent<P3dMaterialCloner>();
        materialCloner.Index = 0;

        //将第一个材质的"_MainTex"设置为可喷涂的
        var paintableTexture = gameObject.AddComponent<P3dPaintableTexture>();
        paintableTexture.Slot = new P3dSlot(0, "_MainTex");
    }

    private void OnDestroy()
    {
        Destroy(_mesh);
    }

    void UpdateMesh()
    {
        if (_mesh == null)
        {
            _mesh = new Mesh();
        }
        else
        {
            _mesh.Clear();
        }
        
        //设置顶点,uv,三角面片信息
        _mesh.vertices = new Vector3[]
        {
            new Vector3(-size, -size), 
            new Vector3(+size, -size), 
            new Vector3(-size, +size), 
            new Vector3(+size, +size)
        };
        
        _mesh.uv = new Vector2[]
        {
            new Vector2(0.0f, 0.0f), 
            new Vector2(1.0f, 0.0f),
            new Vector2(0.0f, 1.0f), 
            new Vector2(1.0f, 1.0f)
        };
        
        _mesh.triangles = new int[] {0, 2, 1, 1, 2, 3};
        
        //recalculate data
        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();
        _mesh.RecalculateTangents();

    }
}
