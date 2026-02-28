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
#if UNITY_EDITOR
        [InitializeOnLoad]
        private static class ItemSpawnerDrawer
        {
            public static readonly HashSet<ItemSpawner> Instances = new HashSet<ItemSpawner>();

            static ItemSpawnerDrawer()
            {
                SceneView.duringSceneGui += Draw;
            }

            [InitializeOnLoadMethod]
            static void Clear()
            {
                Instances.Clear();
            }

            static void Draw(SceneView sceneView)
            {
                if (Event.current.type != EventType.Repaint)
                    return;

                Camera cam = sceneView.camera;
                if (!cam) return;

                foreach (ItemSpawner ex in Instances)
                {
                    if (!ex || ex.Mesh == null || ex.Material == null)
                        continue;

                    Matrix4x4 matrix = Matrix4x4.TRS(
                        ex.transform.position,
                        Quaternion.LookRotation(cam.transform.forward),
                        Vector3.one * 0.5f
                    );

                    Graphics.DrawMesh(
                        ex.Mesh,
                        matrix,
                        ex.Material,
                        0,
                        cam
                    );
                }
            }
        }
#endif

        [SerializeField] private ItemInfo itemInfo;

        [SerializeField]
        private SerializableDictionary<string, string> itemData = new SerializableDictionary<string, string>();

        public ItemInfo ItemInfo => itemInfo;
        public SerializableDictionary<string, string> ItemData => itemData;

#if UNITY_EDITOR
        public Mesh Mesh { get; private set; }
        public Material Material { get; private set; }
        private ItemInfo oldItem;
#endif

        private void Awake()
        {
            if (Application.isPlaying)
            {
                ItemComponent itemComponent = gameObject.AddComponent<ItemComponent>();
                itemComponent.Init(new ItemInstance(itemInfo, itemData));
                Destroy(this);
                Destroy(GetComponent<MeshRenderer>());
                Destroy(GetComponent<MeshFilter>());
                transform.localScale = Vector3.one;
            }
#if UNITY_EDITOR
            else if (gameObject.GetComponent<MeshRenderer>() == null)
            {
                gameObject.name = nameof(ItemSpawner);
                Mesh m = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");

                Material material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                material.color = new Color(1, 1, 1, 0.0f);
                material.SetFloat("_Surface", 1.0f);
                EditorUtility.SetDirty(material);
                AssetDatabase.SaveAssets();

                gameObject.AddComponent<MeshFilter>().mesh = m;
                gameObject.AddComponent<MeshRenderer>().material = material;
                transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            }
#endif
        }

#if UNITY_EDITOR
        void OnEnable()
        {
            ItemSpawnerDrawer.Instances.Add(this);
            ReInitImage();
        }

        void OnDisable()
        {
            ItemSpawnerDrawer.Instances.Remove(this);
        }

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
            if (itemInfo == null)
            {
                Material = null;
                return;
            }

            if (Regex.IsMatch(gameObject.name, $"^({Regex.Escape(oldItem?.Id ?? nameof(ItemSpawner))})( \\([0-9]+\\))?$"))
            {
                gameObject.name = itemInfo.Id;
            }

            Mesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");

            Shader shader = itemInfo.Icon == null
                ? Shader.Find("Hidden/InternalErrorShader")
                : Shader.Find("Universal Render Pipeline/Unlit");

            Material = new Material(shader) { mainTexture = itemInfo.Icon };

            Material.hideFlags = HideFlags.DontSave;
        }
#endif
    }
}
