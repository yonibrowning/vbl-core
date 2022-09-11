using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoordinateSpace
{
    /// <summary>
    /// CCF CoordinateSpace defined in (AP,ML,DV) coordinates
    /// </summary>
    public sealed class CCFSpace : CoordinateSpace
    {
        private Vector3 _dimensions = new Vector3(13.2f, 11.4f, 8f);
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
            return CCF2WorldRotation(coord) - _zeroOffset;
        }

        public override Vector3 World2Space(Vector3 world)
        {
            return World2CCFRotation(world + _zeroOffset);
        }

        public Vector3 World2CCFRotation(Vector3 world)
        {
            return new Vector3(world.z, -world.x, -world.y);
        }

        public Vector3 CCF2WorldRotation(Vector3 coord)
        {
            return new Vector3(-coord.y, -coord.z, coord.x);
        }
    }
}
