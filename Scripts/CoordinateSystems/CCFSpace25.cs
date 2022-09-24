using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoordinateSpaces
{
    /// <summary>
    /// AllenCCF CoordinateSpace defined in (AP,DV,LR) coordinates with 25 um units
    /// </summary>
    public sealed class CCFSpace25 : CoordinateSpace
    {
        private Vector3 _dimensions = new Vector3(528, 456, 320);
        private Vector3 _zeroOffset = new Vector3(-5.7f, -4.0f, +6.6f);


        public override Vector3 Dimensions
        {
            get
            {
                return _dimensions;
            }
        }

        public override Vector3 Space2World(Vector3 coord)
        {
            return CCF2WorldRotation(coord/40f) - _zeroOffset;
        }

        public override Vector3 World2Space(Vector3 world)
        {
            Vector3 coord = World2CCFRotation(world + _zeroOffset) * 40f;
            return new Vector3(Mathf.RoundToInt(coord.x), Mathf.RoundToInt(coord.y), Mathf.RoundToInt(coord.z));
        }

        public Vector3 World2CCFRotation(Vector3 world)
        {
            return new Vector3(world.z, -world.y, -world.x);
        }

        public Vector3 CCF2WorldRotation(Vector3 coord)
        {
            return new Vector3(-coord.z, -coord.y, coord.x);
        }
    }
}
