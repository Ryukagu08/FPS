using UnityEngine;
using KinematicCharacterController;

public struct CharacterInput
{
    public Quaternion Rotation;
    public Vector2 Move;
    public bool Jump;
}

public class PlayerCharacter : MonoBehaviour, ICharacterController
{
    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private Transform cameraTarget;
    [Space]
    [SerializeField] private float walkSpeed = 20f;
    [Space]

    private Quaternion _requestRotation;
    private Vector3 _requestedMovement;

    public void Initialize()
    {
        motor.CharacterController = this;
    }

    public void UpdateInput(CharacterInput input)
    {
        _requestRotation = input.Rotation;
        // 2D input vecto to 3D movement on XZ plane.
        _requestedMovement = new Vector3(input.Move.x, 0f, input.Move.y);
        // Clamp lenght to 1 to prevent faster diagonal movement.
        _requestedMovement = Vector3.ClampMagnitude(_requestedMovement, 1f);
        // Orient input to camera.
        _requestedMovement = input.Rotation * _requestedMovement;
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) 
    {
        var groundedMovement = motor.GetDirectionTangentToSurface
        (
            direction: _requestedMovement,
            surfaceNormal: motor.GroundingStatus.GroundNormal
        )* _requestedMovement.magnitude;

        currentVelocity = groundedMovement * walkSpeed;
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) 
    {
        var forward = Vector3.ProjectOnPlane(_requestRotation * Vector3.forward, motor.CharacterUp);
        currentRotation = Quaternion.LookRotation(forward, motor.CharacterUp);

        if (forward != Vector3.zero)
        {
            currentRotation = Quaternion.LookRotation(forward, motor.CharacterUp);
        }
    }

    public void BeforeCharacterUpdate(float deltaTime) { }
    public void PostGroundingUpdate(float deltaTime) { }
    public void AfterCharacterUpdate(float deltaTime) { }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
    public bool IsColliderValidForCollisions(Collider coll) => true;
    public void OnDiscreteCollisionDetected(Collider hitCollider) { }
    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }

    public Transform GetCameraTarget() => cameraTarget;
}
