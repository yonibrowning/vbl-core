using UnityEngine;

public class NeedlesAtlas : CoordinateSystem
{
    private Vector3 invivoConversionAPMLDV = new Vector3(1.087f, 1f, 0.952f);
    private Vector3 invivoConversionPhiThetaBeta = new Vector3(0f, 0f, 0f);

    public override Vector3 ToCCF(Vector3 systemCoord)
    {
        throw new System.NotImplementedException();
    }

    public override Vector3 ToSystem(Vector3 ccfCoord)
    {
        throw new System.NotImplementedException();
    }
}
