using UnityEngine;
using System.Collections;
using System;
namespace FantasyRPG {
    [Serializable]
    public class CharacterStats {
        [SerializeField]
        private BarScript Bar;
        [SerializeField]
        private float currentVal;
        [SerializeField]
        private float maxVal;

        public float CurrentVal {
            get { return currentVal; }
            set {
                this.currentVal = Mathf.Clamp(value, 0, MaxVal);
                Bar.Value = currentVal;
            }
        }

        public float MaxVal {
            get { return maxVal; }
            set {
                this.maxVal = value;
                Bar.MaxValue = maxVal;
            }
        }

        public void Initialize() {
            this.MaxVal = maxVal;
            this.CurrentVal = currentVal;

        }
    }
}