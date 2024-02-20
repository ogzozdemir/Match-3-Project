using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay
{
    public class Board : MonoBehaviour
    {
        [Space(5), Header("Board"), Space(15)]
        [Range(4,6)] public int boardWidth;
        [HideInInspector] public int boardHeight;
        public enum BoardState { Wait, Move }
        public BoardState currentState = BoardState.Move;
        
        [Space(5), Header("Prefabs"), Space(15)]
        [SerializeField] private GameObject backgroundTilePrefab;
        [SerializeField] private Cat[] catPrefabs;
        [SerializeField] private Cat bombPrefab;
        
        [Space(5), Header("Cats"), Space(15)]
        public float catSpeed;
        public float bombChance;
        public Cat[,] allCats;

        [Space(5), Header("Matching"), Space(15)]
        [HideInInspector] public FindMatches matchFinder;
        private float bonusMultiplier;
        [SerializeField] private float bonusAmount;

        private void Awake() => matchFinder = GetComponent<FindMatches>();

        private void Start()
        {
            boardHeight = boardWidth;
            
            allCats = new Cat[boardWidth, boardHeight];
        }

        public void Setup()
        {
            for (int x = 0; x < boardWidth; x++)
            {
                for (int y = 0; y < boardHeight; y++)
                {
                    Vector2 pos = new Vector2(x, y);
                    GameObject tile = Instantiate(backgroundTilePrefab, pos, Quaternion.identity);
                    tile.transform.parent = transform;
                    tile.name = "BG Tile : " + x + "," + y;

                    int catToUse = Random.Range(0, boardWidth);

                    int iterations = 0;
                    while (MatchesAt(new Vector2Int(x,y), catPrefabs[catToUse]) && iterations < 100)
                    {
                        catToUse = Random.Range(0, boardWidth);
                        iterations++;
                    }
                    
                    SpawnCat(new Vector2Int(x, y), catPrefabs[catToUse]);
                }
            }
        }

        private void SpawnCat(Vector2Int pos, Cat catToSpawn)
        {
            if (Random.Range(0, 100) < bombChance + GameController.instance.gameLevel)
                catToSpawn = bombPrefab;
            
            Cat cat = Instantiate(catToSpawn, new Vector3(pos.x, pos.y + boardHeight, 0f), Quaternion.identity);
            cat.transform.parent = this.transform;
            cat.name = "Cat : " + pos.x + "," + pos.y;
            allCats[pos.x, pos.y] = cat;
            
            cat.SetupCat(pos, this);
        }

        private bool MatchesAt(Vector2Int posToCheck, Cat catToCheck)
        {
            if (posToCheck.x > 1)
            {
                if (allCats[posToCheck.x - 1, posToCheck.y].type == catToCheck.type && allCats[posToCheck.x - 2, posToCheck.y].type == catToCheck.type)
                    return true;
            }
            
            if (posToCheck.y > 1)
            {
                if (allCats[posToCheck.x, posToCheck.y - 1].type == catToCheck.type && allCats[posToCheck.x, posToCheck.y - 2].type == catToCheck.type)
                    return true;
            }

            return false;
        }

        private void DestroyMatchedCatAt(Vector2Int pos)
        {
            if (allCats[pos.x, pos.y] != null)
            {
                if (allCats[pos.x, pos.y].isMatched)
                {
                    if (allCats[pos.x, pos.y].type == Cat.CatType.Bomb)
                        GameController.instance.IncreaseGameTime(2);

                    Instantiate(allCats[pos.x, pos.y].destroyEffect, new Vector2(pos.x, pos.y), Quaternion.identity);
                    
                    Destroy(allCats[pos.x, pos.y].gameObject);
                    allCats[pos.x, pos.y] = null;
                }
            }
        }

        public void DestroyMatches()
        {
            for (int i = 0; i < matchFinder.currentMatches.Count; i++)
            {
                if (matchFinder.currentMatches[i] != null)
                {
                    ScoreCheck(matchFinder.currentMatches[i]);
                    DestroyMatchedCatAt(matchFinder.currentMatches[i].posIndex);
                }
            }

            StartCoroutine(DecreaseRow());
            
            UIManager.instance.addScore = 0;
            GameController.instance.PlayMeowSound();
        }

        private IEnumerator DecreaseRow()
        {
            yield return new WaitForSeconds(.1f);

            int nullCounter = 0;

            for (int x = 0; x < boardWidth; x++)
            {
                for (int y = 0; y < boardHeight; y++)
                {
                    if (allCats[x, y] == null)
                        nullCounter++;
                    else if (nullCounter > 0)
                    {
                        allCats[x, y].posIndex.y -= nullCounter;
                        allCats[x, y - nullCounter] = allCats[x, y];
                        allCats[x, y] = null;
                    }
                }

                nullCounter = 0;
            }

            StartCoroutine(FillBoard());
        }

        private IEnumerator FillBoard()
        {
            yield return new WaitForSeconds(.25f);
            RefillBoard();

            yield return new WaitForSeconds(.25f);
            
            matchFinder.FindAllMatches();

            if (matchFinder.currentMatches.Count > 0)
            {
                bonusMultiplier++;
                
                yield return new WaitForSeconds(.25f);
                DestroyMatches();
            }
            else
            {
                yield return new WaitForSeconds(.25f);
                currentState = BoardState.Move;

                bonusMultiplier = 0;
            }
        }

        private void RefillBoard()
        {
            for (int x = 0; x < boardWidth; x++)
            {
                for (int y = 0; y < boardHeight; y++)
                {
                    if (allCats[x, y] == null)
                    {
                        int catToUse = Random.Range(0, boardWidth);
                        SpawnCat(new Vector2Int(x,y), catPrefabs[catToUse]);
                    }
                }
            }
            
            CheckMisplacedCats();
        }

        private void CheckMisplacedCats()
        {
            List<Cat> foundCats = new List<Cat>();
            foundCats.AddRange(FindObjectsOfType<Cat>());
            
            for (int x = 0; x < boardWidth; x++)
            {
                for (int y = 0; y < boardHeight; y++)
                {
                    if (foundCats.Contains(allCats[x, y]))
                        foundCats.Remove(allCats[x, y]);
                }
            }

            foreach (Cat cat in foundCats)
                Destroy(cat.gameObject);
        }
        
        public void ShuffleBoard()
        {
            if (currentState != BoardState.Wait)
            {
                currentState = BoardState.Wait;

                List<Cat> catsFromBoard = new List<Cat>();

                for (int x = 0; x < boardWidth; x++)
                {
                    for (int y = 0; y < boardHeight; y++)
                    {
                        catsFromBoard.Add(allCats[x, y]);
                        allCats[x, y] = null;
                    }
                }

                for (int x = 0; x < boardWidth; x++)
                {
                    for (int y = 0; y < boardHeight; y++)
                    {
                        int catToUse = Random.Range(0, catsFromBoard.Count);

                        int iterations = 0;
                        while (MatchesAt(new Vector2Int(x, y), catsFromBoard[catToUse]) && iterations < 100)
                        {
                            catToUse = Random.Range(0, catsFromBoard.Count);
                            iterations++;
                        }
                        
                        catsFromBoard[catToUse].SetupCat(new Vector2Int(x, y), this);
                        allCats[x, y] = catsFromBoard[catToUse];
                        catsFromBoard.RemoveAt(catToUse);
                    }
                }
            }

            StartCoroutine(FillBoard());
        }

        private void ScoreCheck(Cat catToCheck)
        {
            if (bonusMultiplier > 0)
            {
                float bonusToAdd;
                bonusToAdd = catToCheck.pointValue * bonusMultiplier * bonusAmount;
                GameController.instance.scorePoints += Mathf.RoundToInt(bonusToAdd);
                
                UIManager.instance.DrawScore((int)bonusToAdd);
            }
            else
            {
                GameController.instance.scorePoints += catToCheck.pointValue;
                UIManager.instance.DrawScore(catToCheck.pointValue);
            }
        }
    }
}