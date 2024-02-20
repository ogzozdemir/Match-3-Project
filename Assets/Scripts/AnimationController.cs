using UnityEngine;

namespace Gameplay
{
    public class AnimationController : MonoBehaviour
    {
        private void Hide() => gameObject.SetActive(false);
        private void StartGame() => GameController.instance.StartGame();
    } 
}