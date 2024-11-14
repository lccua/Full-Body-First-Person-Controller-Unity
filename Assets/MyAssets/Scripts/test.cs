using UnityEngine;
namespace StarterAssets {
public class NewEmptyCSharpScript : MonoBehaviour
{
   [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _lOffset;

    // Update is called once per frame
    void Update()
    {
        Vector3 localTarget = transform.parent.InverseTransformPoint(_target.position);

        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, _lOffset.z + localTarget.z);
    } 
}

}

