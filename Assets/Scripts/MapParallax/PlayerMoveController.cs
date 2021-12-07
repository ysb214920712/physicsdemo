using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveController : MonoBehaviour
{
    public enum SIDE { Left, Right }


        Vector3 initialPosition;
        Vector3 newPos;
        SIDE side;

        public KeyCode moveLeftKey = KeyCode.A;
        public KeyCode moveRightKey = KeyCode.D;

        public float translateOffset = 3.5f;


        void Start()
        {
            initialPosition = transform.position;

            side = SIDE.Left;
            newPos = transform.localPosition + new Vector3(translateOffset, 0, 0);
        }
        
        void Update()
        {
            if (Input.GetKeyDown(moveLeftKey))
            {
                if (side == SIDE.Right)
                {
                    newPos = initialPosition + new Vector3(-translateOffset, 0, 0);
                    side = SIDE.Left;
                }
            }
            else if (Input.GetKeyDown(moveRightKey))
            {
                if (side == SIDE.Left)
                {
                    newPos = initialPosition + new Vector3(translateOffset, 0, 0);
                    side = SIDE.Right;
                }
            }

            transform.localPosition = Vector3.Lerp(transform.localPosition, newPos, Time.deltaTime * 10);
        }
}
