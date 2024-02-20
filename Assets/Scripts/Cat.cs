using System.Collections;
using UnityEngine;

namespace Gameplay
{
    public class Cat : MonoBehaviour
    {
        [HideInInspector] public Board board;
        [HideInInspector] public Vector2Int posIndex;
        private Vector2 firstTouchPosition;
        private Vector2 finalTouchPosition;
        private bool isMousePressed;
        private float swipeAngle;
        private Cat otherCat;
        private SpriteRenderer spriteRenderer;
        
        [HideInInspector] public bool isMatched;
        [HideInInspector] public Vector2Int previousPos;
        
        public enum CatType
        {
            Cat01,
            Cat02,
            Cat03,
            Cat04,
            Cat05,
            Cat06,
            Bomb
        }

        [Space(5), Header("Cat Variables"), Space(15)]
        public CatType type;
        public int blastSize;
        public int pointValue;
        public GameObject destroyEffect;

        private void Awake() => spriteRenderer = GetComponent<SpriteRenderer>();
        
        private void Update()
        {
            if (Vector2.Distance(transform.position, posIndex) > .01f)
                transform.position = Vector2.Lerp(transform.position, posIndex, board.catSpeed * Time.deltaTime);
            else
            {
                transform.position = new Vector3(posIndex.x, posIndex.y, 0f);
                board.allCats[posIndex.x, posIndex.y] = this;
            }
                
            if (isMousePressed && Input.GetMouseButtonUp(0))
            {
                if (board.allCats[posIndex.x, posIndex.y].type != CatType.Bomb)
                    spriteRenderer.color = Color.white;
                else
                    spriteRenderer.color = Color.red;
                
                isMousePressed = false;
                
                if (board.currentState == Board.BoardState.Move && GameController.instance.gameTime > 0)
                {
                    finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    CalculateAngle();
                }
            }
        }

        public void SetupCat(Vector2Int pos, Board thisBoard)
        {
            posIndex = pos;
            board = thisBoard;
        }

        private void OnMouseDown()
        {
            if (board.currentState == Board.BoardState.Move && GameController.instance.gameTime > 0)
            {
                firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                isMousePressed = true;

                if (board.allCats[posIndex.x, posIndex.y].type != CatType.Bomb)
                    spriteRenderer.color = Color.gray;
                else
                    spriteRenderer.color = new Color32(150, 0, 0, 255);
            }
        }

        private void CalculateAngle()
        {
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y,
                finalTouchPosition.x - firstTouchPosition.x);
            swipeAngle = swipeAngle * 180 / Mathf.PI;

            if (Vector3.Distance(firstTouchPosition, finalTouchPosition) > .5f)
                MovePieces();
        }

        private void MovePieces()
        {
            previousPos = posIndex;
            
            otherCat = null;
            
            if (swipeAngle < 45 && swipeAngle > -45 && posIndex.x < board.boardWidth - 1)
            {
                otherCat = board.allCats[posIndex.x + 1, posIndex.y];
                otherCat.posIndex.x--;
                posIndex.x++;
                
                UpdateCatArray();
            }
            else if (swipeAngle > 45 && swipeAngle <= 135 && posIndex.y < board.boardHeight - 1)
            {
                otherCat = board.allCats[posIndex.x, posIndex.y + 1];
                otherCat.posIndex.y--;
                posIndex.y++;
                
                UpdateCatArray();
            }
            else if (swipeAngle < -45 && swipeAngle >= -135 && posIndex.y > 0)
            {
                otherCat = board.allCats[posIndex.x, posIndex.y - 1];
                otherCat.posIndex.y++;
                posIndex.y--;
                
                UpdateCatArray();
            }
            else if (swipeAngle > 135 || swipeAngle < -135 && posIndex.x > 0)
            {
                if (posIndex.x <= 0) return;
                
                otherCat = board.allCats[posIndex.x - 1, posIndex.y];
                otherCat.posIndex.x++;
                posIndex.x--;

                UpdateCatArray();
            }

            if(otherCat == null) return;
            
            StartCoroutine(CheckMove());
        }

        private void UpdateCatArray()
        {
            board.allCats[posIndex.x, posIndex.y] = this;
            board.allCats[otherCat.posIndex.x, otherCat.posIndex.y] = otherCat;
        }

        private IEnumerator CheckMove()
        {
            board.currentState = Board.BoardState.Wait;
            
            yield return new WaitForSeconds(.25f);
            board.matchFinder.FindAllMatches();

            if (otherCat != null)
            {
                if (!isMatched && !otherCat.isMatched)
                {
                    otherCat.posIndex = posIndex;
                    posIndex = previousPos;

                    UpdateCatArray();

                    yield return new WaitForSeconds(.25f);
                    board.currentState = Board.BoardState.Move;
                }
                else
                    board.DestroyMatches();
            }
        }
    }
}