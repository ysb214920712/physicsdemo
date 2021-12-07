using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using DG.Tweening;

public class BattleCamera : MonoBehaviour
{
    Camera cam;
    Vector3 velocity = Vector3.zero;
    public Vector3 cameraPos;
    public bool follow = false;
    public Transform target;
    public float smoothTime = 0.5f;
    public float maxSpeed = Mathf.Infinity;
    public float angleSpeed = 30.0f;

    float followElapaseTime_ = .0f;
    public float followDelay = 0.0f;

    public Vector3 playerOffset = new Vector3(0, 1.8f, -2.6f);
    public Vector3 lookatOffset = new Vector3(0, 0f, 17f);

    void Awake()
    {
        cam = gameObject.GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if( !(target != null && follow) )
            return;

        followElapaseTime_ += Time.deltaTime;

        if( followElapaseTime_ < followDelay )
            return;

        Vector3 destPos = target.TransformPoint(playerOffset);
        Quaternion destRot = Quaternion.LookRotation(target.TransformPoint(lookatOffset)-destPos);

        // Smoothly move the camera towards that target position
        transform.position = Vector3.SmoothDamp(transform.position, destPos, ref velocity, smoothTime);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, destRot, angleSpeed*Time.deltaTime);
        cameraPos = transform.position;
    }

    public void SetTarget(Transform trans)
    {
        target = trans;
    }

    // public void Set(MapPoint.SetCamera data)
    // {
    //     transform.position = data.cameraPos;
    //     transform.rotation =  data.cameraRot;
    //     cameraPos = transform.position;
    // }

    // public void Move(MapPoint.SetCamera data, float time)
    // {
    //     transform.DOMove(data.cameraPos, time).OnComplete(()=>{
    //         cameraPos = transform.position;
    //     });
    //     transform.DORotate(data.cameraRot.eulerAngles, time);
    // }

    public void Follow(bool enable)
    {
        follow = enable;
        velocity = Vector3.zero;
        followElapaseTime_ = .0f;
    }
}
