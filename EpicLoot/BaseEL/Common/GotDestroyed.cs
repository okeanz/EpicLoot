using System;
using UnityEngine;

namespace EpicLoot.BaseEL.Common
{
    public class GotDestroyed : MonoBehaviour
    {
        public void OnDisable()
        {
            Debug.LogError($"I got destroyed! ({gameObject.name})");
            Debug.Log(Environment.StackTrace);
        }
    }
}
