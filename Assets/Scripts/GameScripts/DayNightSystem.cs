using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
namespace FantasyRPG {
    [RequireComponent(typeof(Light))]
    public class DayNightSystem : MonoBehaviour {
        public enum SunType {
            early,
            dawn,
            morning,
            noon,
            afternoon,
            dusk,
            evening,
        }
        public delegate void updateType();
        public static event updateType OnUpdateType;
        public SunType currentType;
        public int days;
        public Text timeText;
        public float time = 18000f;
        public bool paused = false;
        public float timeScale = 100;
        public float intensity;
        public Color fogday = Color.grey;
        public Color fognight = Color.black;


        private Transform sun;
        private float m_Angle;

        private TimeSpan CurrentTime;


        void Start() {
            sun = transform;
        }

        void Update() {
            if (!paused && sun != null) {
                //if (CurrentTime > (24 * 60)) {
                //    CurrentTime -= 24 * 60;
                //}
                time += timeScale * Time.deltaTime;

                if (time > 86400) {
                    days++;
                    time = 0;
                }

                CurrentTime = TimeSpan.FromSeconds(time);
                string[] _time = CurrentTime.ToString().Split(":"[0]);
                timeText.text = "<color=#ded>" + _time[0] + ":" + _time[1]+ "</color>";
                sun.rotation = Quaternion.Euler(new Vector3((time - 21600) / 86400 * 360, 0, 0));
                if (time < 43200) {
                    intensity = 1 - (43200 - time) / 43200;
                } else {
                    intensity = 1 - ((43200 - time) / 43200 * -1);
                }
                RenderSettings.fogColor = Color.Lerp(fognight, fogday, intensity * intensity);
                GetComponent<Light>().intensity = intensity;
                //if (CurrentTime > 1080 && currentType != SunType.evening) {
                //    currentType = SunType.evening;
                //    if (OnUpdateType != null) OnUpdateType();
                //} else if (CurrentTime > 1020 && CurrentTime < 1080 && currentType != SunType.dusk) {
                //    currentType = SunType.dusk;
                //    if (OnUpdateType != null) OnUpdateType();
                //} else if (CurrentTime > 780 && CurrentTime < 1020 && currentType != SunType.afternoon) {
                //    currentType = SunType.afternoon;
                //    if (OnUpdateType != null) OnUpdateType();
                //} else if (CurrentTime > 660 && CurrentTime < 780 && currentType != SunType.noon) {
                //    currentType = SunType.noon;
                //    if (OnUpdateType != null) OnUpdateType();
                //} else if (CurrentTime > 420 && CurrentTime < 660 && currentType != SunType.morning) {
                //    currentType = SunType.morning;
                //    if (OnUpdateType != null) OnUpdateType();
                //} else if (CurrentTime > 360 && CurrentTime < 420 && currentType != SunType.dawn) {
                //    currentType = SunType.dawn;
                //    if (OnUpdateType != null) OnUpdateType();
                //} else if (CurrentTime > 0 && CurrentTime < 360 && currentType != SunType.early) {
                //    currentType = SunType.early;
                //    if (OnUpdateType != null) OnUpdateType();
                //}

                //m_Angle = ((CurrentTime / (24 * 60)) * 360) - 90;
                //sun.eulerAngles = Vector3.right * m_Angle;
            }
        }
        void FixedUpdate() {
            if (Input.GetKeyUp(KeyCode.Comma)) {
                if (timeScale >= 200) {
                    timeScale -= 100;
                } else {
                    timeScale = 100;
                }
            }
            if (Input.GetKeyUp(KeyCode.Period)) {
                if (timeScale <= 900) { timeScale += 100; } else {
                    timeScale = 1000;
                }
            }
        }

        //public string FloatToTime(float toConvert, string format) {
        //    string result = "";
        //    switch (format) {
        //        case "00.0":
        //            result = string.Format("{0:00}:{1:0}",
        //                Mathf.Floor(toConvert) % 60,//seconds
        //                Mathf.Floor((toConvert * 10) % 10));//miliseconds
        //            break;
        //        case "#0.0":
        //            result = string.Format("{0:#0}:{1:0}",
        //                Mathf.Floor(toConvert) % 60,//seconds
        //                Mathf.Floor((toConvert * 10) % 10));//miliseconds
        //            break;
        //        case "00.00":
        //            result = string.Format("{0:00}:{1:00}",
        //                Mathf.Floor(toConvert) % 60,//seconds
        //                Mathf.Floor((toConvert * 100) % 100));//miliseconds
        //            break;
        //        case "00.000":
        //            result = string.Format("{0:00}:{1:000}",
        //                Mathf.Floor(toConvert) % 60,//seconds
        //                Mathf.Floor((toConvert * 1000) % 1000));//miliseconds
        //            break;
        //        case "#00.000":
        //            result = string.Format("{0:#00}:{1:000}",
        //                Mathf.Floor(toConvert) % 60,//seconds
        //                Mathf.Floor((toConvert * 1000) % 1000));//miliseconds
        //            break;
        //        case "#0:00":
        //            result = string.Format("{0:#0}:{1:00}",
        //                Mathf.Floor(toConvert / 60),//minutes
        //                Mathf.Floor(toConvert) % 60);//seconds
        //            break;
        //        case "#00:00":
        //            result = string.Format("{0:#00}:{1:00}",
        //                Mathf.Floor(toConvert / 60),//minutes
        //                Mathf.Floor(toConvert) % 60);//seconds
        //            break;
        //        case "0:00.0":
        //            result = string.Format("{0:0}:{1:00}.{2:0}",
        //                Mathf.Floor(toConvert / 60),//minutes
        //                Mathf.Floor(toConvert) % 60,//seconds
        //                Mathf.Floor((toConvert * 10) % 10));//miliseconds
        //            break;
        //        case "#0:00.0":
        //            result = string.Format("{0:#0}:{1:00}.{2:0}",
        //                Mathf.Floor(toConvert / 60),//minutes
        //                Mathf.Floor(toConvert) % 60,//seconds
        //                Mathf.Floor((toConvert * 10) % 10));//miliseconds
        //            break;
        //        case "0:00.00":
        //            result = string.Format("{0:0}:{1:00}.{2:00}",
        //                Mathf.Floor(toConvert / 60),//minutes
        //                Mathf.Floor(toConvert) % 60,//seconds
        //                Mathf.Floor((toConvert * 100) % 100));//miliseconds
        //            break;
        //        case "#0:00.00":
        //            result = string.Format("{0:#0}:{1:00}.{2:00}",
        //                Mathf.Floor(toConvert / 60),//minutes
        //                Mathf.Floor(toConvert) % 60,//seconds
        //                Mathf.Floor((toConvert * 100) % 100));//miliseconds
        //            break;
        //        case "0:00.000":
        //            result = string.Format("{0:0}:{1:00}.{2:000}",
        //                Mathf.Floor(toConvert / 60),//minutes
        //                Mathf.Floor(toConvert) % 60,//seconds
        //                Mathf.Floor((toConvert * 1000) % 1000));//miliseconds
        //            break;
        //        case "#0:00.000":
        //            result = string.Format("{0:#0}:{1:00}.{2:000}",
        //                Mathf.Floor(toConvert / 60),//minutes
        //                Mathf.Floor(toConvert) % 60,//seconds
        //                Mathf.Floor((toConvert * 1000) % 1000));//miliseconds
        //            break;
        //    }
        //    return result;
        //}
    }
}