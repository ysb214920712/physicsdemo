using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EzySlice;
using UnityEngine.Serialization;

[RequireComponent(typeof(Camera))]
    public class MouseSlice : MonoBehaviour
    {
        [Range(0.01f, 0.4f)]
        public float raySpace = 0.2f; //射线的检测间隔,用于将画出的线分段进行检测

        public Material lineMaterial;
        public bool showDebugPlane = false; //是否显示plane
        public GameObject slicePlane; //可用作切割的平面（切割方式可使用该平面检测collider的方式，此脚本中使用射线检测，只是用平面的位置和法线向量)
        public Material sliceMaterial; //切割后平面的材质
        
        private Vector3 _startPoint, _endPoint, _screenSize;
        private Camera _camera;
        //private UnityEngine.Plane _slicePlane = new UnityEngine.Plane();
        private bool _isDragging;
        private List<Collider> hitColliderList = new List<Collider>(); //临时存储碰撞点信息
        
        void Start()
        {
            if (!_camera)
                _camera = GetComponent<Camera>();
            slicePlane.SetActive(false);
        }
        
        /// <summary>
        /// 根据画出线的位置设置plane的位置信息
        /// </summary>
        void SetPlane()
        {
            GetSlicePlaneInfo(out var planePos, out var normalVec);
            //将平面原本的法线向量转成normalVec
            Quaternion rotate = Quaternion.FromToRotation(Vector3.up, normalVec);
            slicePlane.transform.localRotation = rotate;
            slicePlane.transform.position = planePos;
            slicePlane.SetActive(showDebugPlane);
        }
        
        
        /// <summary>
        /// 根据起点和终点算出画线平面的位置和法向量
        /// </summary>
        /// <param name="planePos"></param>
        /// <param name="normalVec"></param>
        void GetSlicePlaneInfo(out Vector3 planePos, out Vector3 normalVec)
        {
            //ray to point
            var startRay = _camera.ViewportPointToRay(_camera.ScreenToViewportPoint(_startPoint));
            var endRay = _camera.ViewportPointToRay(_camera.ScreenToViewportPoint(_endPoint));
            var start = startRay.GetPoint(_camera.nearClipPlane);
            var end = endRay.GetPoint(_camera.nearClipPlane);
            var depth = endRay.direction.normalized;
            
            //画出线的方向矢量
            var planeTangent = (end - start).normalized;
            //如果没有画线，默认沿X方向切割
            if (planeTangent == Vector3.zero)
                planeTangent = Vector3.right;
            
            //切割平面的法线向量
            normalVec = Vector3.Cross(depth, planeTangent);
            planePos = (end + start) / 2;
        }
        
        void Update()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR || UNITY_WEBGL
            if (Input.GetMouseButtonDown(0))
            {
                _isDragging = true;
                _startPoint = Input.mousePosition;
            }
            if (Input.GetMouseButton(0))
            {
                _endPoint = Input.mousePosition;
            }
            if (Input.GetMouseButtonUp(0))
            {
                SetPlane();
                Slice(_startPoint, _endPoint);
                _isDragging = false;
            }
#elif UNITY_ANDROID || UNITY_IPHONE
            for (int i = 0; i < Input.touchCount; i++)
            {
                if(Input.GetTouch(i).phase== TouchPhase.Began)
                   {
                        _isDragging = true;
                        _startPoint = Input.mousePosition;
                    }
                if(Input.GetTouch(i).phase == TouchPhase.Moved)
                    {
                        _endPoint = Input.mousePosition;
                    }
                    
                if (Input.GetTouch(i).phase == TouchPhase.Ended)
                    {   
                        SetPlane();
                        Slice(_startPoint, _endPoint);
                        _isDragging = false;
                    }
                
            }
#endif
        }

        /// <summary>
        /// 实现切割物体功能
        /// </summary>
        /// <param name="start">画线的起点（接触位置）</param>
        /// <param name="end">画线的终点（接触位置）</param>
        void Slice(Vector3 start, Vector3 end)
        {
            float near = _camera.nearClipPlane;
            Vector3 line = start - end;
            
            float rayLength = new Vector2(line.x / _screenSize.x, line.y / _screenSize.y).magnitude;
            line = _camera.ScreenToWorldPoint(new Vector3(_endPoint.x, _endPoint.y, near)) - _camera.ScreenToWorldPoint(new Vector3(_startPoint.x, _startPoint.y, near));
            if (raySpace > rayLength)
                return;
            
            //将画出的线分为射线发射点，检测是否有处在切割平面的物体
            for (float i = 0; i <= rayLength; i += raySpace)
            {
                Ray ray = _camera.ScreenPointToRay(Vector3.Lerp(start, end, i / rayLength));
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 1000, ~LayerMask.GetMask("CanNotSlice")))
                {
                    var item = hit.collider;
                    if (!hitColliderList.Contains(item))
                    {
                        hitColliderList.Add(item);
                        //Debug.Log("ADD item name: " + item.name);
                    }
                }
            }
            
            //对所有碰撞物体进行切割
            foreach (var collider in hitColliderList)
            {
                SlicedHull hull = SliceObject(collider.gameObject, sliceMaterial);
                if (hull != null)
                {
                    //以切割平面的正反面为标准分为上下两块
                    GameObject lower = hull.CreateLowerHull(collider.gameObject, sliceMaterial);
                    GameObject upper = hull.CreateUpperHull(collider.gameObject, sliceMaterial);
                    //增加对应的模块
                    AddHullComponents(lower);
                    AddHullComponents(upper);
                    //销毁原来物体
                    Destroy(collider.gameObject);
                }
            }
            //每次检测过后清空List
            hitColliderList.Clear();
        }

        /// <summary>
        /// 对单个物体进行切割
        /// </summary>
        /// <param name="obj">切割的物体</param>
        /// <param name="crossSectionMaterial">切割后赋予的材质</param>
        /// <returns></returns>
        private SlicedHull SliceObject(GameObject obj, Material crossSectionMaterial = null)
        {
            // slice the provided object using the transforms of this object
            if (obj.GetComponent<MeshFilter>() == null)
                return null;
            
            GetSlicePlaneInfo(out var planePos, out var normalVec);
            //EzySlice的核心功能,传入切割平面的位置,平面的法向量以及切割后切割面的材质
            return obj.Slice(planePos, normalVec, crossSectionMaterial);
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
        
        //OnPostRender is called after the camera renders all its objects
        void OnPostRender()
        {
            GLLine(_startPoint, _endPoint);
        }
        
        void GLLine(Vector2 start, Vector2 end)
        {
            //在屏幕上画线
            if (_isDragging && lineMaterial)
            {
                _screenSize = new Vector2(Screen.width, Screen.height);
                GL.PushMatrix();
                lineMaterial.SetPass(0);
                GL.LoadOrtho();
                GL.Begin(GL.LINES);
                GL.Color(Color.red);
                GL.Vertex3(start.x / _screenSize.x, start.y / _screenSize.y, _camera.nearClipPlane);
                GL.Vertex3(end.x / _screenSize.x, end.y / _screenSize.y, _camera.nearClipPlane);
                GL.End();
                GL.PopMatrix();
            }
        }
        
    }