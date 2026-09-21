using UnityEngine;
namespace EduAI.Court
{
    public sealed class BillboardLabel : MonoBehaviour
    {
        private Camera target;
        private void LateUpdate()
        {
            if (!target) target = Camera.main;
            if (target) transform.rotation = target.transform.rotation;
        }
    }
}
