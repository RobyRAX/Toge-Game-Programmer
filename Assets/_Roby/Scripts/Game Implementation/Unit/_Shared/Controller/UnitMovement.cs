using RAXY.Movement;
using UnityEngine;

public class UnitMovement : MovementController
{
    [SerializeField]
    float runSpeed = 5f;

    [SerializeField]
    float sprintSpeed = 8f;

    public float RunSpeed => runSpeed;
    public float SprintSpeed => sprintSpeed;

    protected override void Update()
    {
        base.Update();

        if (enableRotation)
            RotateTowardsMovement();
    }
}
