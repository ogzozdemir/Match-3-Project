using UnityEngine;

namespace Gameplay
{
    public class CameraController : MonoBehaviour
    {
        private Board board;
        private Camera cam;
        [SerializeField] private float cameraOffset;
        //[SerializeField] private float aspectRatio;
        [SerializeField] private float padding;

        private void Awake()
        {
            board = FindObjectOfType<Board>();
            cam = Camera.main;

            if (board != null)
                RepositionCamera(board.boardWidth - 1, board.boardHeight - 1);
        }

        private void RepositionCamera(float x, float y)
        {
            padding = board.boardWidth / 1.25f;
            
            Vector3 tempPos = new Vector3(x/2, x/2, cameraOffset);
            transform.position = tempPos;

            /*
            if (board.boardWidth >= board.boardHeight)
                cam.orthographicSize = (board.boardWidth / 2 + padding) / aspectRatio;  
            else
            */
            
            cam.orthographicSize = board.boardHeight / 2 + padding;
        }
    }
}