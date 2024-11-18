using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmobot.BuildingSystem
{
     [CreateAssetMenu(fileName = "BuildingInfo", menuName = "Cosmobot/BuildingSystem/BuildingInfo", order = 1)]
    public class BuildingInfo : ScriptableObject, IEquatable<BuildingInfo>
    {

        [SerializeField]
        private string id;
        [SerializeField]
        private string displayName;
        [SerializeField]
        private Texture2D icon;
        [SerializeField]
        private GameObject prefab;
        [SerializeField]
        private SerializableDictionary<string, string> additionalData = new();
        [SerializeField]
        private Vector2Int gridDimensions = new(1,1);

        public string Id => id;
        public string DisplayName => displayName;
        public Texture2D Icon => icon;
        public GameObject Prefab => prefab;
        public IReadOnlyDictionary<string, string> AdditionalData => additionalData;
        public Vector2Int GridDimensions => gridDimensions;

        public override bool Equals(object obj) => Equals(obj as BuildingInfo);

        public bool Equals(BuildingInfo other) => other != null && id == other.id;

        public override int GetHashCode() => id.GetHashCode();

        public static bool operator ==(BuildingInfo left, BuildingInfo right)
        {
            if (ReferenceEquals(left, right)) return true;
            return left is not null && right is not null && left.Equals(right);
        }

        public static bool operator !=(BuildingInfo left, BuildingInfo right) => !(left == right);
    }
}
