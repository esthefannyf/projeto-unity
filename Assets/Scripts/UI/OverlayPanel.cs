using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CrystalRuins.UI
{
    // Reusable modal: title, body text and up to two buttons.
    public sealed class OverlayPanel : MonoBehaviour
    {
        [SerializeField] Text title;
        [SerializeField] Text body;
        [SerializeField] Button primary;
        [SerializeField] Button secondary;

        public void Show(string titleText, string bodyText,
                         string primaryLabel, Action onPrimary,
                         string secondaryLabel = null, Action onSecondary = null)
        {
            title.text = titleText;
            body.text = bodyText;
            Bind(primary, primaryLabel, onPrimary);
            Bind(secondary, secondaryLabel, onSecondary);

            gameObject.SetActive(true);
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(primary.gameObject);
        }

        public void Hide() => gameObject.SetActive(false);

        static void Bind(Button button, string label, Action action)
        {
            bool used = !string.IsNullOrEmpty(label);
            button.gameObject.SetActive(used);
            button.onClick.RemoveAllListeners();
            if (!used) return;

            button.GetComponentInChildren<Text>().text = label;
            button.onClick.AddListener(() => action?.Invoke());
        }
    }
}
