using UnityEngine;

namespace EpicLoot.BaseEL.LootBeams
{
    public class AlwaysPointUp : MonoBehaviour
    {
        public void Update()
        {
            transform.rotation = Quaternion.identity;
        }
    }
}
