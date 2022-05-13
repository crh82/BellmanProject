using UnityEngine;

namespace TestScripts
{
    public class CubeScript : MonoBehaviour
    {
        public void UpdateHeight(float val)
        {
            var transform1 = transform;
            float value = transform1.localScale.y + val;
            transform1.localScale = new Vector3(1f, value, 1f);
        }
    }
}
