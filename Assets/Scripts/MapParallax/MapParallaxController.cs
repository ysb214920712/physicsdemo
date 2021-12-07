using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapParallaxController : MonoBehaviour
{
    public static MapParallaxController instance;
    public MeshRenderer[] meshes;
    public float SwerveX = 0.0f;
    public float SwerveY = 0.0f;

    private void Awake() {
        instance = this;
    }

    void Start()
    {
        
    }

    void Update()
    {
        this.UpdateShaderParameter();
    }

    void UpdateShaderParameter()
    {
        foreach(var mesh in meshes)
        {
            mesh.material.SetFloat("_SwerveX", SwerveX / 300);//由于shader中参数范围为-0.003 ~ 0.003
            mesh.material.SetFloat("_SwerveY", SwerveY / 300);//为方便在Inspector面板上调整,将面板上的值扩大300倍,在实际赋值时再除以300
        }
    }
}
