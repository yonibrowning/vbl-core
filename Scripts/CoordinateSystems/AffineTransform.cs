using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoordinateTransforms
{
    public abstract class AffineTransform : CoordinateTransform
    {
        private Vector3 _translation;
        private Vector3 _scaling;
        private Vector3 _inverseScaling;
        private Quaternion _rotation;
        private Quaternion _inverseRotation;

        /// <summary>
        /// Define an AffineTransform by passing the translation, scaling, and rotation that go from origin space to this space
        /// </summary>
        /// <param name="translation">translation on x/y/z</param>
        /// <param name="scaling">scaling on x/y/z</param>
        /// <param name="rotation">rotation around z, y, x in that order (or on xy plane, then xz plane, then yz plane)</param>
        public AffineTransform(Vector3 translation, Vector3 scaling, Vector3 rotation)
        {
            _translation = translation;
            _scaling = scaling;
            _inverseScaling = new Vector3(1f / _scaling.x, 1f / _scaling.y, 1f / _scaling.z);
            _rotation = Quaternion.Euler(rotation);
            _inverseRotation = Quaternion.Inverse(_rotation);
        }

        public override Vector3 Space2Transform(Vector3 ccfCoord)
        {
            return _rotation * Vector3.Scale(ccfCoord + _translation, _scaling);
        }

        public override Vector3 Transform2Space(Vector3 systemCoord)
        {
            return Vector3.Scale(_inverseRotation * systemCoord, _inverseScaling) - _translation;
        }
    }
}
