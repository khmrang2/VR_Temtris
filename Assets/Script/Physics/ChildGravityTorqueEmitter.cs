using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ChildGravityTorqueEmitter : MonoBehaviour
{
    public Vector3 ComputeTorque(Vector3 centerOfMass, float individualMass)
    {
        Vector3 r = transform.position - centerOfMass;
        Vector3 F = Physics.gravity * individualMass;
        return Vector3.Cross(r, F);
    }
}