using UnityEngine;
using CoordinateSpaces;

public class AnnotationDataset : VolumetricDataset
{
    private CoordinateSpace volumeCoordinateSpace;
    private bool[,,] areaBorders;
    
    /// <summary>
    /// Create a new annotation dataset
    /// </summary>
    /// <param name="size"></param>
    /// <param name="data"></param>
    /// <param name="map"></param>
    /// <param name="ccfIndexMap"></param>
    public AnnotationDataset((int ap, int dv, int lr) size, ushort[] data, uint[] map, byte[] ccfIndexMap) : base(size, ccfIndexMap, map, data)
    {
        volumeCoordinateSpace = new CCFSpace25();
    }

    public void ComputeBorders()
    {
        if (areaBorders != null)
        {
            Debug.LogWarning("(AnnotationDataset) Borders were going to be re-computed unnecessarily. Skipping");
            return;
        }
        areaBorders = new bool[size.x, size.y, size.z];

        for (int ap = 0; ap < size.x; ap++)
        {
            // We go through coronal slices, going down each DV depth, anytime the *next* annotation point changes, we mark this as a border
            for (int lr = 0; lr < (size.z-1); lr++)
            {
                for (int dv = 0; dv < (size.y-1); dv ++)
                {
                    if ((data[ap, dv, lr] != data[ap, dv + 1, lr]) || data[ap,dv,lr] != data[ap, dv, lr+1])
                        areaBorders[ap, dv, lr] = true;
                }
            }
        }

    }

    public bool BorderAtIndex(int ap, int dv, int lr)
    {
        if ((ap >= 0 && ap < size.x) && (dv >= 0 && dv < size.y) && (lr >= 0 && lr < size.z))
            return areaBorders[ap, dv, lr];
        else
            return false;
    }

    public bool BorderAtIndex(Vector3 apdvlr)
    {
        return BorderAtIndex(Mathf.RoundToInt(apdvlr.x), Mathf.RoundToInt(apdvlr.y), Mathf.RoundToInt(apdvlr.z));
    }

    /// <summary>
    /// Convert a Unity world coordinate into the annotation dataset coordinate space
    /// </summary>
    /// <param name="world">Coordinate in Unity x/y/z mm space</param>
    /// <returns>Coordinate in ap/dv/lr 25 um units</returns>
    public Vector3 World2Annotation(Vector3 world)
    {
        return volumeCoordinateSpace.World2Space(world);
    }

    /// <summary>
    /// Convert an annotation dataset coordinate into Unity world coordinates
    /// </summary>
    /// <param name="apdvlr">Coordinate in ap/dv/lr 25 um units</param>
    /// <returns>Coordinate in Unity x/y/z mm space</returns>
    public Vector3 Annotation2World(Vector3 apdvlr)
    {
        return volumeCoordinateSpace.Space2World(apdvlr);
    }
}
