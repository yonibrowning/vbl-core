using UnityEngine;

namespace CoordinateTransforms
{
    //public class NeedlesTransform : CoordinateTransform
    //{
    //    private Vector3 invivoConversionAPMLDV = new Vector3(-1.087f, 1f, -0.952f);
    //    private Vector3 inverseConversion = new Vector3(-1 / 1.087f, 1f, -1 / 0.952f);
    //    private Vector3 bregma = new Vector3(5.4f, 5.7f, 0.332f);

    //    public override string Name { get { return "Needles"; } }

    //    public override string Prefix {  get { return "ne"; } }

    //    /// <summary>
    //    /// Convert a coordinate in Needles space back to CCF space
    //    /// </summary>
    //    /// <param name="needlesCoord">Coordinate should be relative to bregma (ap,ml,dv)</param>
    //    /// <returns></returns>
    //    /// <exception cref="System.NotImplementedException"></exception>
    //    public override Vector3 Transform2Space(Vector3 needlesCoord)
    //    {
    //        Vector3 ccfCoord = Vector3.Scale(needlesCoord, inverseConversion) + bregma;
    //        return ccfCoord;
    //    }

    //    /// <summary>
    //    /// Convert a coordinate in CCF space back to needles space
    //    /// </summary>
    //    /// <param name="ccfCoord">Coordinate should be relative to CCF (ap=0, ml=0, dv=0)</param>
    //    /// <returns></returns>
    //    public override Vector3 Space2Transform(Vector3 ccfCoord)
    //    {
    //        // Apply transform
    //        Vector3 needlesCoord = Vector3.Scale(ccfCoord - bregma, invivoConversionAPMLDV);
    //        return needlesCoord;
    //    }

    //    public override Vector3 Transform2SpaceRot(Vector3 coordTransformed)
    //    {
    //        return new Vector3(-coordTransformed.x, coordTransformed.y, -coordTransformed.z);
    //    }

    //    public override Vector3 Space2TransformRot(Vector3 coordSpace)
    //    {
    //        return new Vector3(-coordSpace.x, coordSpace.y, -coordSpace.z);
    //    }
    //}

    public class NeedlesTransform : AffineTransform
    {
        public override string Name { get { return "Needles"; } }

        public override string Prefix { get { return "ne"; } }


        //private Vector3 invivoConversionAPMLDV = new Vector3(-1.087f, 1f, -0.952f);
        //private Vector3 inverseConversion = new Vector3(-1 / 1.087f, 1f, -1 / 0.952f);
        //private Vector3 bregma = new Vector3(5.4f, 5.7f, 0.332f);

        /// <summary>
        /// Angles are (yaw, pitch, spin)
        /// </summary>
        public NeedlesTransform() : base(new Vector3(-1.087f, 1f, -0.952f), new Vector3(0f, 0f, 0f))
        {

        }
    }
}