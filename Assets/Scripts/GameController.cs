using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Gameplay
{
    public class GameController : MonoBehaviour
    {
        public static GameController instance { get; private set; }

        [Space(5), Header("Game References"), Space(15)]
        private Board board;
        public AudioSource sfxAudioSource;
        public AudioSource bgAudioSource;
        [SerializeField] private AudioClip[] meowSFX;

        [Space(5), Header("Game Settings"), Space(15)]
        [HideInInspector] public int gameLevel;
        [HideInInspector] public bool isGameStarted;
        
        public float gameTime;
        private float maxGameTime;
        
        [Space(5), Header("Score Variables"), Space(15)]
        public int scorePoints;
        [SerializeField] private float displayScore;
        [SerializeField] private float scoreSpeed;
        public int shuffleCounter;
    
        private void Awake()
        {
            Application.targetFrameRate = -1;
            
            instance = this;
            board = FindObjectOfType<Board>();
        }

        private void Start() => maxGameTime = gameTime;

        private void Update()
        {
            if (!isGameStarted) return;
            
            if (gameTime > 0)
            {
                gameTime -= Time.deltaTime;

                if (gameTime <= 0)
                    gameTime = 0;
            }
            else
            {
                if (board.currentState == Board.BoardState.Move)
                    RoundOver();
            }

            UIManager.instance.timeSlider.value = gameTime;
            
            displayScore = Mathf.Lerp(displayScore, scorePoints, scoreSpeed * Time.deltaTime);
            UIManager.instance.scoreText.text = Mathf.Round(displayScore).ToString("0");
        }

        public void Shuffle()
        {
            if (shuffleCounter > 0 && board.currentState == Board.BoardState.Move && gameTime > 0)
            {
                board.ShuffleBoard();
                shuffleCounter--;
                UIManager.instance.shuffleCount.text = shuffleCounter.ToString();
            }
        }
        
        public void IncreaseGameTime(int time)
        {
            if (gameTime < maxGameTime)
                gameTime += time;
        }

        public void PlayMeowSound() => sfxAudioSource.PlayOneShot(meowSFX[Random.Range(0,meowSFX.Length)]);

        public void StartGame()
        {
            isGameStarted = true;
            board.Setup();
            UIManager.instance.shuffleButton.enabled = true;
        }

        private void RoundOver()
        {
            UIManager.instance.endGameLayer.SetActive(true);
            UIManager.instance.endGameScore.text = scorePoints.ToString();

            if (scorePoints > PlayerPrefs.GetFloat("highscore"))
                PlayerPrefs.SetFloat("highscore", scorePoints);
        }

        public void TryAgain() => StartCoroutine(LoadGame());
        
        public void PauseGame()
        {
            bgAudioSource.Pause();
            
            UIManager.instance.pauseGameLayer.SetActive(true);
            Time.timeScale = 0;
        }
        
        public void ResumeGame()
        {
            bgAudioSource.UnPause();

            SaveController.instance.Save();
            
            UIManager.instance.pauseGameLayer.SetActive(false);
            Time.timeScale = 1;
        }
        
        private IEnumerator LoadGame()
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync("Level01");
        
            while (!operation.isDone)
                yield return null;
        
            SceneManager.LoadScene("Level01");
        }
    }
   
}