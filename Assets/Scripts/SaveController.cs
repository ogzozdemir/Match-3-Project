using UnityEngine;

namespace Gameplay
{
    public class SaveController : MonoBehaviour
    {
        public static SaveController instance { get; private set; }
        
        private void Awake()
        {
            instance = this;
            
            if (!PlayerPrefs.HasKey("highscore")) PlayerPrefs.SetFloat("highscore", 0);
            
            if (!PlayerPrefs.HasKey("bgVolume")) PlayerPrefs.SetFloat("bgVolume", 1);
            if (!PlayerPrefs.HasKey("sfxVolume")) PlayerPrefs.SetFloat("sfxVolume", 1);
        }

        private void Start() => Load();

        private void Load()
        {
            float highscore = PlayerPrefs.GetFloat("highscore", 0);
            UIManager.instance.highscoreText.text = "HIGHSCORE: " + highscore;
            
            UIManager.instance.musicVolumeSlider.value = PlayerPrefs.GetFloat("bgVolume", GameController.instance.bgAudioSource.volume);
            UIManager.instance.sfxVolumeSlider.value = PlayerPrefs.GetFloat("sfxVolume", GameController.instance.sfxAudioSource.volume);
        }
        
        public void Save()
        {
            PlayerPrefs.SetFloat("bgVolume", GameController.instance.bgAudioSource.volume);
            PlayerPrefs.SetFloat("sfxVolume", GameController.instance.sfxAudioSource.volume);
        }
    }
}