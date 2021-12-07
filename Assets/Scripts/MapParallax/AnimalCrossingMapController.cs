using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalCrossingMapController : MonoBehaviour
{
    public static AnimalCrossingMapController instance;
    public float BendY = 0.0f;
    private MeshRenderer[] meshes = null;
    public Transform[] plans = null;
    public int singlePlaneWidth = 50;

    public KeyCode moveLeftKey = KeyCode.A; // X++
    public KeyCode moveRightKey = KeyCode.D; // X--
    public KeyCode moveUpKey = KeyCode.W; // Z--
    public KeyCode moveDownKey = KeyCode.S; // Z++

    public float speed = 10;
    private List<Vector3> planeInitialPositions = new List<Vector3>();


    private void Awake() {
        instance = this;
    }

    void Start()
    {
        for (int i = 0; i < this.plans.Length; i++)
        {
            this.planeInitialPositions.Add(this.plans[i].localPosition);
        }

        meshes = transform.GetComponentsInChildren<MeshRenderer>();
    }

    void Update()
    {
        if (Input.GetKey(this.moveLeftKey))
        {
            this.Move(speed, 0);
        }   
        if (Input.GetKey(this.moveRightKey))
        {
            this.Move(-speed, 0);
        }
        if (Input.GetKey(this.moveUpKey))
        {
            this.Move(0, -speed);
        }
        if (Input.GetKey(this.moveDownKey))
        {
            this.Move(0, speed);
        }

        foreach(var mesh in this.meshes)
        {
            mesh.material.SetFloat("_BendY", BendY / 300);
        }
    }

    void Move(float x, float z)
    {
        Vector3 newPos;
        for (int i = 0; i < this.plans.Length; i++)
        {
            newPos = this.plans[i].localPosition + new Vector3(x, 0, z);
            this.plans[i].localPosition = Vector3.Lerp(this.plans[i].localPosition, newPos, Time.deltaTime * 10);

            Vector3 initialPos = this.planeInitialPositions[i];
            Vector3 curPos = this.plans[i].localPosition;
            Vector3 tempPos = Vector3.zero;

            if (Mathf.Abs(curPos.x) > singlePlaneWidth * 1.5f)
            {
                if (curPos.x > initialPos.x)
                {
                    tempPos = new Vector3(-(singlePlaneWidth * 1.5f), initialPos.y, curPos.z);
                }
                else if (curPos.x < initialPos.x)
                {
                    tempPos = new Vector3(singlePlaneWidth * 1.5f, initialPos.y, curPos.z);
                }
                else
                {
                    tempPos = initialPos;
                }
                this.plans[i].localPosition = tempPos;
            }
            else if(Mathf.Abs(curPos.z) > singlePlaneWidth * 1.5f)
            {
                if (curPos.z > initialPos.z)
                {
                    tempPos = new Vector3(curPos.x, initialPos.y, -(singlePlaneWidth * 1.5f));
                }
                else if (curPos.z < initialPos.z)
                {
                    tempPos = new Vector3(curPos.x, initialPos.y, singlePlaneWidth * 1.5f);
                }
                else
                {
                    tempPos = initialPos;
                }
                this.plans[i].localPosition = tempPos;
            }
        }
    }
}
