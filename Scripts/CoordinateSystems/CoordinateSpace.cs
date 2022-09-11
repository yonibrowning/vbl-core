using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoordinateSpace
{
    public abstract class CoordinateSpace
    {
        public abstract Vector3 Dimensions { get; }

        public abstract Vector3 Space2World(Vector3 coord);
        public abstract Vector3 World2Space(Vector3 world);

    }
}
