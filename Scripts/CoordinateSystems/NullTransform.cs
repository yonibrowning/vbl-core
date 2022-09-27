using UnityEngine;


namespace CoordinateTransforms
{
    public class NullTransform : CoordinateTransform
    {
        public override string Name
        {
            get
            {
                return "Null";
            }
        }

        public override string Prefix
        {
            get
            {
                return "null";
            }
        }

        public override Vector3 Transform2Space(Vector3 coord)
        {
            return coord;
        }

        public override Vector3 Space2Transform(Vector3 coord)
        {
            return coord;
        }

        public override Vector3 Transform2SpaceRot(Vector3 coordTransformed)
        {
            return coordTransformed;
        }

        public override Vector3 Space2TransformRot(Vector3 coordSpace)
        {
            return coordSpace;
        }
    }
}