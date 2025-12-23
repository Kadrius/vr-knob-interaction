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

    protected Vector3 prevHandForward;
    protected Vector3 prevHandRight;
    protected Vector3 prevHandUp;

    protected float xFactor, yFactor, zFactor;

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
        Vector3 currentHandForward = _grabbable.GrabPoints[0].forward;
        Vector3 currentHandRight = _grabbable.GrabPoints[0].right;
        Vector3 currentHandUp = _grabbable.GrabPoints[0].up;

        xFactor = MathUtils.ColinearityFactor(rotationAxis, currentHandRight, true);
        yFactor = MathUtils.ColinearityFactor(rotationAxis, currentHandUp, true);
        zFactor = MathUtils.ColinearityFactor(rotationAxis, currentHandForward, true);

        //Chose to use SignedAngle because it gives an absolute angle difference. Using eulerAngles difference can lead to issues when crossing 0/360 boundary.
        //It isn't perfect because the axis of rotation is not always perfectly aligned, but the difference is minimal because the calculation is frame by frame
        //and the factors help to minimize the error.
        float xDeltaAngle = Vector3.SignedAngle(prevHandForward, currentHandForward, currentHandRight);
        float yDeltaAngle = Vector3.SignedAngle(prevHandForward, currentHandForward, currentHandUp);
        float zDeltaAngle = Vector3.SignedAngle(prevHandRight, currentHandRight, currentHandForward);

        xDeltaAngle *= xFactor;
        yDeltaAngle *= yFactor;
        zDeltaAngle *= zFactor;

        DebugPrints(xDeltaAngle, yDeltaAngle, zDeltaAngle);

        Vector3 newRotation = _grabbable.Transform.rotation.eulerAngles;
        newRotation.z += xDeltaAngle + yDeltaAngle + zDeltaAngle;
        _grabbable.Transform.rotation = Quaternion.Euler(newRotation);

        prevHandForward = currentHandForward;
        prevHandRight = currentHandRight;
        prevHandUp = currentHandUp;
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
                             $"xDeltaAngle: {xDeltaAngle} - xFactor: {xFactor}\n" +
                             $"yDeltaAngle: {yDeltaAngle} - yFactor: {yFactor}\n" +
                             $"zDeltaAngle: {zDeltaAngle} - zFactor: {zFactor}\n";
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
