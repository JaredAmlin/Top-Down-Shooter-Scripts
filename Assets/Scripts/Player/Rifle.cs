using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rifle : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private BoxCollider _collider;
        
    // Start is called before the first frame update
    void Start()
    {
        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            _rb = rb;
        }
        else print("The Rigidbody on the Rifle is NULL");

        if (TryGetComponent<BoxCollider>(out BoxCollider collider))
        {
            _collider = collider;
        }
        else print("The Collider on the Rifle is NULL");
    }

    public void DropRifle()
    {
        transform.parent = null;
        _collider.enabled = true;
        _rb.isKinematic = false;
        _rb.useGravity = true;
        _rb.AddTorque(Vector3.back, ForceMode.Impulse);
        _rb.AddTorque(Vector3.right, ForceMode.Impulse);
        _rb.AddForce(Vector3.back, ForceMode.Impulse);
        _rb.AddForce(Vector3.up, ForceMode.Impulse);
    }
}
