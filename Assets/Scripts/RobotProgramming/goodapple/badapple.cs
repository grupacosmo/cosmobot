using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace Cosmobot
{
    public class badapple : MonoBehaviour
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

        public bool checkColor(float x, float y)
        {
            Color pixelColor = texture.GetPixel((int)(x * renderTexture.width), (int)(y * renderTexture.height));
            
            if (pixelColor.grayscale <= 0.5) return false;
            else return true;
        }
    }
}
