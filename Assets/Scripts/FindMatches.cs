using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Gameplay
{
    public class FindMatches : MonoBehaviour
    {
        private Board board;
        [HideInInspector] public List<Cat> currentMatches = new List<Cat>();

        private void Awake() => board = FindObjectOfType<Board>();
        
        public void FindAllMatches()
        {
            currentMatches.Clear();
            
            for (int x = 0; x < board.boardWidth; x++)
            {
                for (int y = 0; y < board.boardHeight; y++)
                {
                    Cat currentCat = board.allCats[x, y];

                    if (currentCat != null)
                    {
                        if (x > 0 && x < board.boardWidth - 1)
                        {
                            Cat leftCat = board.allCats[x - 1, y];
                            Cat rightCat = board.allCats[x + 1, y];

                            if (leftCat != null && rightCat != null)
                            {
                                if (leftCat.type == currentCat.type && rightCat.type == currentCat.type)
                                {
                                    currentCat.isMatched = true;
                                    leftCat.isMatched = true;
                                    rightCat.isMatched = true;
                                    
                                    currentMatches.Add(currentCat);
                                    currentMatches.Add(leftCat);
                                    currentMatches.Add(rightCat);
                                }
                            }
                        }
                        
                        if (y > 0 && y < board.boardHeight - 1)
                        {
                            Cat aboveCat = board.allCats[x, y + 1];
                            Cat belowCat = board.allCats[x, y - 1];

                            if (aboveCat != null && belowCat != null)
                            {
                                if (aboveCat.type == currentCat.type && belowCat.type == currentCat.type)
                                {
                                    currentCat.isMatched = true;
                                    aboveCat.isMatched = true;
                                    belowCat.isMatched = true;
                                    
                                    currentMatches.Add(currentCat);
                                    currentMatches.Add(aboveCat);
                                    currentMatches.Add(belowCat);
                                }
                            }
                        }
                    }
                }
            }

            if (currentMatches.Count > 0)
                currentMatches = currentMatches.Distinct().ToList();
            
            CheckForBombs();
        }

        public void CheckForBombs()
        {
            for (int i = 0; i < currentMatches.Count; i++)
            {
                Cat cat = currentMatches[i];

                int x = cat.posIndex.x;
                int y = cat.posIndex.y;

                if(cat.posIndex.x > 0)
                {
                    if(board.allCats[x - 1, y] != null)
                    {
                        if (board.allCats[x - 1, y].type == Cat.CatType.Bomb)
                            MarkBombArea(new Vector2Int(x - 1, y), board.allCats[x - 1, y]);
                    }
                }

                if (cat.posIndex.x < board.boardWidth - 1)
                {
                    if (board.allCats[x + 1, y] != null)
                    {
                        if (board.allCats[x + 1, y].type == Cat.CatType.Bomb)
                            MarkBombArea(new Vector2Int(x + 1, y), board.allCats[x + 1, y]);
                    }
                }

                if (cat.posIndex.y > 0)
                {
                    if (board.allCats[x, y - 1] != null)
                    {
                        if (board.allCats[x, y - 1].type == Cat.CatType.Bomb)
                            MarkBombArea(new Vector2Int(x, y - 1), board.allCats[x, y - 1]);
                    }
                }

                if (cat.posIndex.y < board.boardHeight - 1)
                {
                    if (board.allCats[x, y + 1] != null)
                    {
                        if (board.allCats[x, y + 1].type == Cat.CatType.Bomb)
                            MarkBombArea(new Vector2Int(x, y + 1), board.allCats[x, y + 1]);
                    }
                }
            }
        }

        public void MarkBombArea(Vector2Int bombPos, Cat theBomb)
        {
            for (int x = bombPos.x - theBomb.blastSize; x <= bombPos.x + theBomb.blastSize; x++)
            {
                for (int y = bombPos.y - theBomb.blastSize; y <= bombPos.y + theBomb.blastSize; y++)
                {
                    if (x >= 0 && x < board.boardWidth && y >= 0 && y < board.boardHeight)
                    {
                        if (board.allCats[x, y] != null)
                        {
                            board.allCats[x, y].isMatched = true;
                            currentMatches.Add(board.allCats[x, y]);
                        }
                    }
                }
            }

            currentMatches = currentMatches.Distinct().ToList();
        }
    }
}