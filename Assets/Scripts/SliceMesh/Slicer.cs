using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;

public class Slicer : MonoBehaviour
{
    public Material sliceMaterial;
    public float rotateSpeed = 5f;
    
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X");
        transform.Rotate(0, 0, -mouseX * 5);

        if (Input.GetMouseButtonDown(0))
        {
            //以transform为范围检测所有碰撞体
            Collider[] colliders = Physics.OverlapBox(transform.position, new Vector3(5, 0.005f, 5), transform.rotation,
                ~LayerMask.GetMask("CanNotSlice"));

            foreach (var collider in colliders)
            {
                //GameObject[] objects = collider.gameObject.SliceInstantiate(transform.position, transform.up);
                SlicedHull hull = collider.gameObject.Slice(transform.position, transform.up);
                if (hull != null)
                {
                    GameObject lower = hull.CreateLowerHull(collider.gameObject, sliceMaterial);
                    GameObject upper = hull.CreateUpperHull(collider.gameObject, sliceMaterial);
                    AddHullComponents(lower);
                    AddHullComponents(upper);
                    //销毁原来物体
                    Destroy(collider.gameObject);
                }
            }
        }
    }
    
    /// <summary>
    /// 添加刚体组件以及设置碰撞所需的信息
    /// </summary>
    /// <param name="go"></param>
    private void AddHullComponents(GameObject go)
    {
        Rigidbody rb = go.AddComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        MeshCollider meshCollider = go.AddComponent<MeshCollider>();
        //设为凸多面体,只有凸多面体才能为刚体
        meshCollider.convex = true;

        //添加一个爆炸力用于分开利于观察
        rb.AddExplosionForce(100, go.transform.position, 20);
    }
}
