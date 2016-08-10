using UnityEngine;
using System.Collections;

namespace JojoCrowdAi
{
    public class Member : MonoBehaviour
    {
        private GroupInfo group = null;
        private bool isInit = false;

        [SerializeField]
        public Vector3 position = new Vector3(0.6f, 0.6f, 0f);

        [SerializeField]
        public float maxSpeed = 1f;

        [SerializeField]
        public float radius = 1f;

        public Vector3 curVelocity = Vector3.zero;
        private float lastTime = 0f;
        private float curTime = 0f;

        private bool isStart = false;

        
        public void Initialize(GroupInfo g)
        {
            group = g;
            isInit = true;
        }

        void Start()
        {
            lastTime = Time.time;
            isStart = true;
        }

        // Update is called once per frame
        void Update()
        {
            curTime = Time.time;
            DoUpdate(curTime - lastTime);
            lastTime = curTime;
        }

        public void DoUpdate(float deltaTime)
        {
            position += curVelocity*deltaTime;

            if (isStart)
                transform.position = position;
        }
    }
}

