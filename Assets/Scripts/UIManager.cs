using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Gameplay
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager instance { get; private set; }
        
        [Space(5), Header("UI References"), Space(15)]
        public Slider timeSlider;
        public TMP_Text scoreText;
        public TMP_Text shuffleCount;
        public TMP_Text addScoreText;
        public Button shuffleButton;
        
        [HideInInspector] public int addScore;
        
        [Space(5), Header("Pause Game References"), Space(15)]
        public GameObject pauseGameLayer;
        public Slider musicVolumeSlider;
        public Slider sfxVolumeSlider;
        
        [Space(5), Header("End Game References"), Space(15)]
        public GameObject endGameLayer;
        public TMP_Text endGameScore;
        public TMP_Text highscoreText;

        private void Awake() => instance = this;

        private void Start()
        {
            pauseGameLayer.SetActive(false);
            endGameLayer.SetActive(false);
            
            shuffleButton.enabled = false;
            shuffleCount.text = GameController.instance.shuffleCounter.ToString();
            timeSlider.maxValue = GameController.instance.gameTime;
            timeSlider.value = timeSlider.maxValue;
        }
        
        public void DrawScore(int score)
        {
            addScoreText.gameObject.SetActive(false);
            addScoreText.gameObject.SetActive(true);
            
            addScore += score;
            addScoreText.text = "+" + addScore;
        }

        public void MusicVolumeChange() => GameController.instance.bgAudioSource.volume = musicVolumeSlider.value;
        public void SFXVolumeChange() => GameController.instance.sfxAudioSource.volume = sfxVolumeSlider.value;
    }
}