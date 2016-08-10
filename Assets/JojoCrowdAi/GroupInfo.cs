using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace JojoCrowdAi
{
    public class GroupInfo
    {
        public Vector3 startPoint = Vector3.zero;
        public Vector3 endPoint = Vector3.zero;
        //public Dictionary<int, float> costDict = new Dictionary<int, float>();
        public List<Member> members = new List<Member>(); 

        public Color color = Color.red;
    }
}

