using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

namespace Cosmobot
{
    public class OptionsMenu : MonoBehaviour
    {
        public AudioMixer audioMixer;

        public TMP_Dropdown resolutionDropdown;

        private Resolution[] resolutions;
        
        private void Start()
        {
            resolutions = Screen.resolutions;
            resolutionDropdown.ClearOptions();
            List<string> options = new List<string>();
            int currentResolutionIndex = 0;
            for (int i = 0; i < resolutions.Length; i++)
            {
                options.Add(resolutions[i].width + " x " + resolutions[i].height + " @ " + resolutions[i].refreshRateRatio + "hz");
                if (resolutions[i].width == Screen.currentResolution.width
                    && resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                }
            }
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
        }

        public void SetResolution(int resolutionIndex)
        {
            Resolution resolution = resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }

        public void SetVolume(float volume)
        {
            audioMixer.SetFloat("volume", volume);
        }

        public void SetQuality(int qualityIndex)
        {
            QualitySettings.SetQualityLevel(qualityIndex);
        }

        public void SetFullScreen(bool fullScreen)
        {
            Screen.fullScreen = fullScreen;
        }
    }
}
