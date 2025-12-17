using Oculus.Interaction;
using TMPro;
using UnityEngine;

public class OneGrabKnobKadriusTransformer : MonoBehaviour, ITransformer
{
    [SerializeField] protected Axis rotationAxisType = Axis.Up;
    [SerializeField] TextMeshProUGUI debugText;

    protected IGrabbable _grabbable;
    protected Transform grabbableTransform;
    protected Vector3 rotationAxis;

    protected float prevHandForwardAngle;
    protected float prevHandRightAngle;
    protected float prevHandUpAngle;

    public void Initialize(IGrabbable grabbable)
    {
        _grabbable = grabbable;
    }

    public void BeginTransform()
    {
        grabbableTransform = _grabbable.Transform;
        rotationAxis = GetRotationAxis();

        prevHandForwardAngle = _grabbable.GrabPoints[0].rotation.eulerAngles.z;
        prevHandRightAngle = _grabbable.GrabPoints[0].rotation.eulerAngles.x;
        prevHandUpAngle = _grabbable.GrabPoints[0].rotation.eulerAngles.y;
    }

    public void UpdateTransform()
    {
        Vector3 handForward = _grabbable.GrabPoints[0].forward;
        Vector3 handRight = _grabbable.GrabPoints[0].right;
        Vector3 handUp = _grabbable.GrabPoints[0].up;

        float currentHandForwardAngle = _grabbable.GrabPoints[0].rotation.eulerAngles.z;
        float currentHandRightAngle = _grabbable.GrabPoints[0].rotation.eulerAngles.x;
        float currentHandUpAngle = _grabbable.GrabPoints[0].rotation.eulerAngles.y;

        float xFactor = MathUtils.ColinearityFactor(rotationAxis, handRight, true);
        float yFactor = MathUtils.ColinearityFactor(rotationAxis, handUp, true);
        float zFactor = MathUtils.ColinearityFactor(rotationAxis, handForward, true);

        float xDeltaAngle = currentHandRightAngle - prevHandRightAngle;
        float yDeltaAngle = currentHandUpAngle - prevHandUpAngle;
        float zDeltaAngle = currentHandForwardAngle - prevHandForwardAngle;

        xDeltaAngle *= xFactor;
        yDeltaAngle *= yFactor;
        zDeltaAngle *= zFactor;

        print($"xDeltaAngle: {xDeltaAngle}");
        print($"yDeltaAngle: {yDeltaAngle}");
        print($"zDeltaAngle: {zDeltaAngle}");

        DebugPrints(xDeltaAngle, yDeltaAngle, zDeltaAngle);

        //_grabbable.Transform.Rotate(rotationAxis, xDeltaAngle + zDeltaAngle + yDeltaAngle);
        Vector3 newRotation = _grabbable.Transform.rotation.eulerAngles;
        newRotation.z += xDeltaAngle + yDeltaAngle + zDeltaAngle;
        _grabbable.Transform.rotation = Quaternion.Euler(newRotation);

        prevHandForwardAngle = currentHandForwardAngle;
        prevHandRightAngle = currentHandRightAngle;
        prevHandUpAngle = currentHandUpAngle;
    }

    public void EndTransform()
    {

    }

    #region Protected Methods

    /// <summary>
    /// Gets the rotation axis in world space from the type selected
    /// </summary>
    protected Vector3 GetRotationAxis()
    {
        Vector3 rotationAxis = Vector3.zero;
        switch (rotationAxisType)
        {
            case Axis.Forward:
                rotationAxis = grabbableTransform.TransformDirection(Vector3.forward);
                break;
            case Axis.Right:
                rotationAxis = grabbableTransform.TransformDirection(Vector3.right);
                break;
            case Axis.Up:
                rotationAxis = grabbableTransform.TransformDirection(Vector3.up);
                break;
        }
        return rotationAxis;
    }

    protected void DebugPrints(float xDeltaAngle, float yDeltaAngle, float zDeltaAngle)
    {
        if (debugText != null)
        {
            debugText.text = $"PrevHandForwardAngle: {prevHandForwardAngle}\n" +
                             $"PrevHandRightAngle: {prevHandRightAngle}\n" +
                             $"PrevHandUpAngle: {prevHandUpAngle}\n" +
                             $"xDeltaAngle: {xDeltaAngle}\n" +
                             $"yDeltaAngle: {yDeltaAngle}\n" +
                             $"zDeltaAngle: {zDeltaAngle}\n";
        }
    }

    #endregion
}

public enum Axis
{
    Right = 0,
    Up = 1,
    Forward = 2
}
