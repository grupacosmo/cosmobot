using UnityEngine;

namespace Cosmobot
{
    // TODO: find a way to make SerializeReference work with UnityEngine.Object
    //   suggestion: Make field as UnityEngine.Object, use property drawer to check if the assigned object is of the
    //   desired type and use code generator to generate property with desired type (by casting or something).
    public class AnyUnityObjectUIAttribute : PropertyAttribute
    {
    }
}