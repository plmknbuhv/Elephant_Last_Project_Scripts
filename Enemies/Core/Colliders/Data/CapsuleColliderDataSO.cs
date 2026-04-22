using UnityEngine;

namespace Code.Enemies.Core.Colliders.Data
{
    [CreateAssetMenu(fileName = "CapsuleDataSO", menuName = "SO/Enemy/Collider/CapsuleData", order = 0)]
    public class CapsuleColliderDataSO : AbstractColliderDataSO
    {
        public float height;
        public float centerPosY;
        public float radius;
    }
}
