using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cosmobot.Utils;

namespace Cosmobot.ItemSystem
{
    using UnityEditor;
    using UnityEngine;

    [ExecuteAlways]
    public class ItemSpawner : MonoBehaviour
    {
        [SerializeField] private ItemInfo itemInfo;

        [SerializeField]
        private SerializableDictionary<string, string> itemData = new SerializableDictionary<string, string>();

        public ItemInfo ItemInfo => itemInfo;
        public SerializableDictionary<string, string> ItemData => itemData;

#if UNITY_EDITOR
        private ItemInfo oldItem;
        private Material material;
#endif

        private void Awake()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (gameObject.GetComponent<MeshRenderer>() == null)
                {
                    Mesh m = Resources.Load<Mesh>("billboard_cross");
                    gameObject.AddComponent<MeshFilter>().mesh = m;
                    gameObject.AddComponent<MeshRenderer>();
                    transform.localScale = Vector3.one;
                }
                material = new Material(Shader.Find("Hidden/InternalErrorShader"));
                gameObject.GetComponent<MeshRenderer>().material = material;
            }
#endif
        }


        void OnEnable()
        {
#if UNITY_EDITOR
            ReInitImage();
#endif
            if (Application.isPlaying)
            {
                transform.localScale = Vector3.one;
                Destroy(this);
                Destroy(GetComponent<MeshRenderer>());
                Destroy(GetComponent<MeshFilter>());
                ItemComponent itemComponent = gameObject.AddComponent<ItemComponent>();
                itemComponent.Init(new ItemInstance(itemInfo, itemData));
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (oldItem != itemInfo)
            {
                ReInitImage();
                oldItem = itemInfo;
            }
        }

        void ReInitImage()
        {
            SetItemIcon();
            
            if (itemInfo == null)
            {
                return;
            }

            if (Regex.IsMatch(gameObject.name,
                    $"^({Regex.Escape(oldItem?.Id ?? nameof(ItemSpawner))})( \\([0-9]+\\))?$"))
            {
                gameObject.name = itemInfo.Id;
            }

        }

        private static readonly int matFieldSurface = Shader.PropertyToID("_Surface");
        private void SetItemIcon()
        {
            if (material == null) 
            {
                return;
            }

            Shader shader = itemInfo?.Icon == null
                ? Shader.Find("Hidden/InternalErrorShader")
                : Shader.Find("Universal Render Pipeline/Unlit");

            material.name = "Mat " + shader.name;
            material.shader = shader;
            material.color = Color.white;
            if (itemInfo?.Icon != null)
            {
                material.mainTexture = itemInfo.Icon;
                const float MatSurfaceTransparent = 1;
                material.SetFloat(matFieldSurface, MatSurfaceTransparent);
            }
            
        }
#endif
    }
}
