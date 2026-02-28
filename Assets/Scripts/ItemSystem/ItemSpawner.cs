using System.Collections.Generic;
using Cosmobot.Utils;

namespace Cosmobot.ItemSystem
{
    using UnityEditor;
    using UnityEngine;

    [ExecuteAlways]
    public class ItemSpawner : MonoBehaviour
    {
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
        [SerializeField] private ItemInfo itemInfo;

        [SerializeField]
        private SerializableDictionary<string, string> itemData = new SerializableDictionary<string, string>();

        public ItemInfo ItemInfo => itemInfo;
        public SerializableDictionary<string, string> ItemData => itemData;

        private void Awake()
        {
            Debug.Log("ItemSpawner awake");
            if (Application.isPlaying)
            {
                ItemComponent itemComponent = gameObject.AddComponent<ItemComponent>();
                itemComponent.Init(new ItemInstance(itemInfo, itemData));
                Destroy(this);
                Destroy(GetComponent<MeshRenderer>());
                Destroy(GetComponent<MeshFilter>());
                transform.localScale = Vector3.one;
            }
            else if (gameObject.GetComponent<MeshRenderer>() == null)
            {
                Mesh m = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");

                Material material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                material.color = new Color(1, 1, 1, 0.0f);
                material.SetFloat("_Surface", 1.0f);
                EditorUtility.SetDirty(gameObject.GetComponent<MeshRenderer>());
                AssetDatabase.SaveAssets(); AssetDatabase.Refresh();

                gameObject.AddComponent<MeshFilter>().mesh = m;
                gameObject.AddComponent<MeshRenderer>().material = material;
                transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            }
        }

        public Mesh Mesh { get; private set; }
        public Material Material { get; private set; }

        ItemInfo oldItem;

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
                oldItem = itemInfo;
                ReInitImage();
            }
        }

        void ReInitImage()
        {
            if (itemInfo == null)
            {
                Material = null;
                return;
            }

            Mesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");

            Shader shader = itemInfo.Icon == null
                ? Shader.Find("Hidden/InternalErrorShader")
                : Shader.Find("Universal Render Pipeline/Unlit");

            Material = new Material(shader) { mainTexture = itemInfo.Icon };

            Material.hideFlags = HideFlags.DontSave;
        }
    }
}
