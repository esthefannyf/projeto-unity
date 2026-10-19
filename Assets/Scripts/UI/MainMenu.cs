using CrystalRuins.Presentation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CrystalRuins.UI
{
    public sealed class MainMenu : MonoBehaviour
    {
        [SerializeField] Button playButton;
        [SerializeField] Button helpButton;
        [SerializeField] Button quitButton;
        [SerializeField] Text bestScore;
        [SerializeField] OverlayPanel overlay;
        [SerializeField] string gameScene = "Game";

        const string HelpText =
            "Mover: WASD / setas ou direcional do controle\n" +
            "Z desfaz | R reinicia a fase | ESC pausa\n" +
            "Controle: B desfaz, Y reinicia, Start pausa\n\n" +
            "Colete todos os diamantes e chegue a saida antes do tempo acabar.\n" +
            "Empurre pedras para tapar espinhos. Chaves abrem portas.\n" +
            "Cobras patrulham sem parar e a mumia persegue. Relogios dao +10s.";

        void Start()
        {
            Time.timeScale = 1f;
            overlay.Hide();

            playButton.onClick.AddListener(() => SceneManager.LoadScene(gameScene));
            helpButton.onClick.AddListener(ShowHelp);
            quitButton.onClick.AddListener(Application.Quit);

            // There is no "quit" in a browser build.
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                quitButton.gameObject.SetActive(false);

            int best = PlayerPrefs.GetInt(GameController.BestScoreKey, 0);
            bestScore.text = best > 0 ? $"Recorde: {best}" : string.Empty;

            EventSystem.current.SetSelectedGameObject(playButton.gameObject);
        }

        void ShowHelp()
        {
            overlay.Show("Como jogar", HelpText, "Voltar", () =>
            {
                overlay.Hide();
                EventSystem.current.SetSelectedGameObject(playButton.gameObject);
            });
        }
    }
}
