// DO NOT INCLUDE THIS FILE IN YOUR BUILD
// ONLY FOR FORMAT-CHECKING PURPOSES WITH Formatting.csproj (github actions)

// This exists only for Formatting.csproj to hide compiler errors about missing Unity types.
// Mostly for CS1503 (cannot convert from 'type' to 'type')
namespace UnityEngine
{
    public class MonoBehaviour : Behaviour { }
    public class Behaviour : Component { }
    public class Component : Object { }
    public class Object { }
}