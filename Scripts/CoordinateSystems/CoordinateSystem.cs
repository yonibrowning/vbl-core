using UnityEngine;


public abstract class CoordinateSystem
{

    /// <summary>
    /// Convert from system coordinates back to CCF space
    /// </summary>
    /// <param name="systemCoord">System coordinate in ap/dv/lr</param>
    /// <returns></returns>
    public abstract Vector3 ToCCF(Vector3 systemCoord);

    /// <summary>
    /// Convert from CCF coordinates to system coordinates
    /// </summary>
    /// <param name="ccfCoord">CCF coordinate in ap/dv/lr</param>
    /// <returns></returns>
    public abstract Vector3 ToSystem(Vector3 ccfCoord);

}