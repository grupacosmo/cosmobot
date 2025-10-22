using System.Collections.Generic;
using UnityEngine;

namespace Cosmobot.Editor.Standalone
{
    [CreateAssetMenu(fileName = "BuildBundle", menuName = "Cosmobot/BuildSettings/BuildBundle")]
    public class BuildBundle : ScriptableObject
    {
        public bool includeInBuild = false;

        [SerializeField]
        private List<Object> files;
        public IReadOnlyList<Object> Files => files.AsReadOnly();
    }
}
