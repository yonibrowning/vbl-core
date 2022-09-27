using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoordinateSpaces
{
    public abstract class CoordinateSpace
    {
        public abstract string Name { get; }

        public abstract Vector3 Dimensions { get; }

        public abstract Vector3 Space2World(Vector3 coord);
        public abstract Vector3 World2Space(Vector3 world);

        public abstract Vector3 Space2WorldRot(Vector3 coord);

        public abstract Vector3 World2SpaceRot(Vector3 world);

        public override string ToString()
        {
            return Name;
        }
    }
}
