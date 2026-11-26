using UnityEngine;


namespace Slafurry.Utils.GameFeel
{

    public class GameFeel : MonoBehaviour
    {
        private IGameFeelEffect[] gameFeelEffects;

        // Collect every effect on this GameObject (was never filled, so PlayEffect threw).
        private void Awake() => gameFeelEffects = GetComponents<IGameFeelEffect>();

        public void PlayEffect()
        {
            foreach (var gameFeel in gameFeelEffects)
            {
                gameFeel.PlayEffect();
            }
        }

        public void StopEffect()
        {
            foreach (var gameFeel in gameFeelEffects)
            {
                gameFeel.StopEffect();
            }
        }
    }
}