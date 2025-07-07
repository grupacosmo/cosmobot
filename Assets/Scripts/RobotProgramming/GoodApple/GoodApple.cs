using UnityEngine;
using UnityEngine.Video;

namespace Cosmobot
{
    public class GoodApple : MonoBehaviour
    {
        public RenderTexture renderTexture;
        public bool start;
        private Texture2D texture;

        private void Start()
        {
            texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        }

        private void Update()
        {
            if (start)
            {
                gameObject.GetComponent<VideoPlayer>().Play();
                start = false;
            }
            RenderTexture.active = renderTexture;

            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();
        }

        public bool CheckColor(float x, float y)
        {
            Color pixelColor = texture.GetPixel((int)(x * renderTexture.width), (int)(y * renderTexture.height));
            
            if (pixelColor.grayscale <= 0.5) return false;
            return true;
        }
    }
}
