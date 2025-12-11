using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Oculus.Interaction
{
    /// <summary>
    /// A Transformer that rotates the target about the up axis.
    /// Updates apply relative rotational changes of a GrabPoint about the up vector
    /// of the pivot.
    /// </summary>
    public class OneGrabRedditKnobTransformer : MonoBehaviour, ITransformer
    {
        [SerializeField, Optional]
        private Transform _pivotTransform = null;

        [SerializeField]
        private Vector3 referencePoint;

        [Serializable]
        public class OneGrabRotateConstraints
        {
            public FloatConstraint MinAngle;
            public FloatConstraint MaxAngle;
        }

        [SerializeField]
        private OneGrabRotateConstraints _constraints;

        public OneGrabRotateConstraints Constraints
        {
            get
            {
                return _constraints;
            }

            set
            {
                _constraints = value;
            }
        }

        private float _relativeAngle = 0.0f;
        private float _constrainedRelativeAngle = 0.0f;

        private IGrabbable _grabbable;

        private Pose _initialPose;
        private Pose _previousGrabPose;

        public void Initialize(IGrabbable grabbable)
        {
            _grabbable = grabbable;
        }

        public void BeginTransform()
        {
            var grabPoint = _grabbable.GrabPoints[0];
            Transform pivot = _pivotTransform != null ? _pivotTransform : _grabbable.Transform; ;
            Vector3 rotationAxis = pivot.TransformDirection(Vector3.up);

            _initialPose = new Pose(pivot.position, Quaternion.LookRotation(pivot.position - grabPoint.position, rotationAxis));
            _previousGrabPose = grabPoint;
        }

        public void UpdateTransform()
        {
            var grabPoint = _grabbable.GrabPoints[0];
            var targetTransform = _grabbable.Transform;

            Transform pivot = _pivotTransform != null ? _pivotTransform : targetTransform;
            Vector3 rotationAxis = pivot.TransformDirection(Vector3.up);

            var targetPose = grabPoint.GetTransformedBy(_initialPose);
            var previousPose = _previousGrabPose.GetTransformedBy(_initialPose);

            var angleDelta = Vector3.SignedAngle(targetPose.up, previousPose.up, _initialPose.forward);

            float previousAngle = _constrainedRelativeAngle;

            _relativeAngle += angleDelta;
            _constrainedRelativeAngle = _relativeAngle;
            if (_constraints.MinAngle.Constrain)
            {
                _constrainedRelativeAngle = Mathf.Max(_constrainedRelativeAngle, _constraints.MinAngle.Value);
            }

            if (_constraints.MaxAngle.Constrain)
            {
                _constrainedRelativeAngle = Mathf.Min(_constrainedRelativeAngle, _constraints.MaxAngle.Value);
            }

            angleDelta = _constrainedRelativeAngle - previousAngle;

            // Apply this angle rotation about the axis to our transform
            targetTransform.RotateAround(pivot.position, rotationAxis, angleDelta);

            _previousGrabPose = grabPoint;
        }

        public void EndTransform()
        {
            // Clamps relative angle to constraints to remove windup
            if (_constraints.MinAngle.Constrain)
            {
                _relativeAngle = Mathf.Max(_constrainedRelativeAngle, _constraints.MinAngle.Value);
            }

            if (_constraints.MaxAngle.Constrain)
            {
                _relativeAngle = Mathf.Min(_constrainedRelativeAngle, _constraints.MaxAngle.Value);
            }
        }

        #region Inject

        public void InjectOptionalPivotTransform(Transform pivotTransform)
        {
            _pivotTransform = pivotTransform;
        }

        public void InjectOptionalConstraints(OneGrabRotateConstraints constraints)
        {
            _constraints = constraints;
        }

        #endregion
    }
}

