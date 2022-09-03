using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MRILinearTransform : CoordinateTransform
{
    //ML_SCALE = 0.952
    //DV_SCALE = 0.885  # multiplicative factor on DV dimension, determined from MRI->CCF transform
    //AP_SCALE = 1.031  # multiplicative factor on AP dimension

    private Vector3 invivoConversionAPMLDV = new Vector3(-1.031f, 0.952f, -0.885f);
    private Vector3 inverseConversion = new Vector3(-1 / 1.031f, 1f / 0.952f, -1 / 0.885f);
    // rotations defined as pitch + = down, yaw + = right, spin + = clockwise
    private Vector3 invivoConversionPitchYawSpin = new Vector3(-7f, 0f, 0f);
    private Vector3 bregma = new Vector3(5.4f, 5.7f, 0.332f);

    public override string Name
    {
        get
        {
            return "MRI-Linear: linear warping on ap/ml and rotation on theta";
        }
    }
    public override string Prefix
    {
        get
        {
            return "ml";
        }
    }


    /// <summary>
    /// Convert a coordinate in MRI-Linear space back to CCF space
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
    /// Convert a coordinate in CCF space back to MRI-linear space
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
