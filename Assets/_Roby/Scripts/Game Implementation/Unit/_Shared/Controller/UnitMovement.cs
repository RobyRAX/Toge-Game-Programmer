using RAXY.Movement;
using UnityEngine;
using Sirenix.OdinInspector;

public class UnitMovement : MovementController
{
    [TitleGroup("Unit")]
    [SerializeField]
    float runSpeed = 5f;

    [TitleGroup("Unit")]
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
