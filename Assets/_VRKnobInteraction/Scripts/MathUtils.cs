using UnityEngine;

public static class MathUtils 
{
    /// <summary>
    /// Returns a float between -1 and 1 depeding in the colinearity of the provided vectors.
    /// If 1 both vector are colinear and point in the same direction, -1 opposite direction, 0 they are perpendicular
    /// </summary>
    /// <param name="vectorA"></param>
    /// <param name="vectorB"></param>
    /// <param name="normalized">If true the vectors are considered already normalized</param>
    /// <returns></returns>
    public static float ColinearityFactor(Vector3 vectorA, Vector3 vectorB, bool normalized = false) 
    {
        return normalized ? Vector3.Dot(vectorA, vectorB) : Vector3.Dot(vectorA.normalized, vectorB.normalized);
    }
}
