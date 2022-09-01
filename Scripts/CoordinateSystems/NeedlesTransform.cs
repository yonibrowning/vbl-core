using UnityEngine;


public class NeedlesTransform : CoordinateTransform
{
    private Vector3 invivoConversionAPMLDV = new Vector3(-1.087f, 1f, -0.952f);
    private Vector3 inverseConversion = new Vector3(-1 / 1.087f, 1f, -1 / 0.952f);
    private Vector3 bregma = new Vector3(5.4f, 5.7f, 0.332f);

    public override string Name
    {
        get
        {
            return "Needles: linear warping on ap/ml";
        }
    }

    public override string Prefix
    {
        get
        {
            return "ne";
        }
    }

    /// <summary>
    /// Convert a coordinate in Needles space back to CCF space
    /// </summary>
    /// <param name="needlesCoord">Coordinate should be relative to bregma (ap,ml,dv)</param>
    /// <returns></returns>
    /// <exception cref="System.NotImplementedException"></exception>
    public override Vector3 ToCCF(Vector3 needlesCoord)
    {
        Vector3 ccfCoord = Vector3.Scale(needlesCoord, inverseConversion) + bregma;
        return ccfCoord;
    }

    /// <summary>
    /// Convert a coordinate in CCF space back to needles space
    /// </summary>
    /// <param name="ccfCoord">Coordinate should be relative to CCF (ap=0, ml=0, dv=0)</param>
    /// <returns></returns>
    public override Vector3 FromCCF(Vector3 ccfCoord)
    {
        Vector3 needlesCoord = Vector3.Scale(ccfCoord - bregma, invivoConversionAPMLDV);
        // Apply rotation
        return needlesCoord;
    }

    //public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    //{
    //    Vector3 dir = point - pivot; // get point direction relative to pivot
    //    dir = Quaternion.Euler(angles) * dir; // rotate it
    //    point = dir + pivot; // calculate rotated point
    //    return point; // return it
    //}
}
