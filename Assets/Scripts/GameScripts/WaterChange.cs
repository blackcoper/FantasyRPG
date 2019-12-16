using UnityEngine;
using System.Collections;
namespace FantasyRPG {
    public class WaterChange : MonoBehaviour {

        public GameObject DayWater;
        public GameObject NightWater;

        private DayNightSystem sunSystem;
        void Start() {
            sunSystem = GameObject.FindObjectOfType<DayNightSystem>().GetComponent<DayNightSystem>();
            checkType();
            DayNightSystem.OnUpdateType += checkType;
        }

        void checkType() {
            if (sunSystem.currentType == DayNightSystem.SunType.dusk || sunSystem.currentType == DayNightSystem.SunType.evening || sunSystem.currentType == DayNightSystem.SunType.early) {
                setNight();
            } else {
                setDay();
            }
        }

        void setNight() {
            if (DayWater && NightWater) {
                DayWater.SetActive(false);
                NightWater.SetActive(true);
            }

        }
        void setDay() {
            if (DayWater && NightWater) {
                DayWater.SetActive(true);
                NightWater.SetActive(false);
            }
        }
    }
}