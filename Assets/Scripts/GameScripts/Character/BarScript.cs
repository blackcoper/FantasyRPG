using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace FantasyRPG {
    public class BarScript : MonoBehaviour {

        private float fillAmount;
        [SerializeField]
        private float lerpSpeed;
        [SerializeField]
        private Image content;
        [SerializeField]
        private Text valueText;
        [SerializeField]
        private Color fullColor;
        [SerializeField]
        private Color lowColor;

        public bool lerpColor;
        public float MaxValue { get; set; }
        public float Value {
            set {
                if (valueText != null) {
                    valueText.text = value + " / " + MaxValue;
                }
                fillAmount = Utility.Math.LimitedRange(value, 0, MaxValue, 0, 1);
            }
        }
        void Start() {
            if (lerpColor) {
                content.color = Color.Lerp(lowColor, fullColor, content.fillAmount);

            }
        }

        void Update() {
            HandleBar();
        }

        private void HandleBar() {
            if (fillAmount != content.fillAmount) {
                content.fillAmount = Mathf.Lerp(content.fillAmount, fillAmount, Time.deltaTime * lerpSpeed);
            }
            if (lerpColor) {
                content.color = Color.Lerp(lowColor, fullColor, content.fillAmount);
            }
        }
    }
}