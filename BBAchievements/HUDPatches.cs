using HarmonyLib;
using MTM101BaldAPI.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BBAchievements
{
    class Updater : MonoBehaviour
    {
        public UnityAction onDestroy;
        public UnityAction<object> toDo;
        public object obj;
        void Update()
        {
            if (obj != null && toDo != null)
                toDo(obj);
        }
        void OnDestroy()
        {
            if (onDestroy != null)
                onDestroy();
        }
        public Updater Set<T>(T o)
        {
            obj = o;
            return this;
        }
        public Updater Set(UnityAction<object> a)
        {
            toDo = a;
            return this;
        }
        public Updater Set(UnityAction a)
        {
            onDestroy = a;
            return this;
        }
    }
    [HarmonyPatch(typeof(HudManager))]
    class HUDPatches
    {
        public static float Step => 0.05f;
        public static bool InProcces => textInProcces || imageInProcces;
        private static bool textInProcces;
        private static bool imageInProcces;
        private static List<Achievement> queue;
        private static HudManager hud;
        private static Image image;
        private static TextMeshProUGUI textMeshProUGUI;
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        private static void AddImage(HudManager __instance)
        {
            queue = new List<Achievement>();
            imageInProcces = false;
            textInProcces = false;
            hud = __instance;
            hud.gameObject.AddComponent<Updater>()
                .Set(hud)
                .Set(() => { queue.Clear(); })
                .Set(x => 
                {
                    HudManager manager = (HudManager)x;
                    if (!InProcces && queue.Count > 0)
                    {
                        Show(queue[0]);
                    }
                    if (!InProcces && queue.Count == 0 && image != null)
                    {
                        image.color = new Color(0, 0, 0, 0);
                    }
                });
            image = UIHelpers.CreateImage(BasePlugin.AssetManager.Get<Sprite>("AchievementBG"), __instance.transform, Vector3.zero, true, 1.5f);
            image.gameObject.transform.localPosition = new Vector3(-408, 360, 0);
            image.color = new Color(1, 1, 1, 0);
            __instance.spritesToDarken = __instance.spritesToDarken.AddToArray(image);
            textMeshProUGUI = UIHelpers.CreateText<TextMeshProUGUI>(BaldiFonts.ComicSans18, "Test", __instance.transform, Vector3.zero);
            textMeshProUGUI.gameObject.transform.localPosition = new Vector3(232, 0, 0);
            textMeshProUGUI.alignment = TextAlignmentOptions.Center;
            textMeshProUGUI.fontSize = 16;
            textMeshProUGUI.color = new Color(1, 1, 1, 0);
        }
        private static IEnumerator ShowText()
        {
            while (textMeshProUGUI.color.a < 1)
            {
                textMeshProUGUI.color = new Color(1, 1, 1, textMeshProUGUI.color.a + Step);
                yield return null;
            }
            float time = 3;
            while (time > 0)
            {
                time -= Time.deltaTime;
                yield return null;
            }
            while (textMeshProUGUI.color.a > 0)
            {
                textMeshProUGUI.color = new Color(1, 1, 1, textMeshProUGUI.color.a - Step);
                yield return null;
            }
            textInProcces = false;
            yield break;
        }
        private static IEnumerator ShowImage()
        {
            while (image.color.a < 1)
            {
                image.color = new Color(1, 1, 1, image.color.a + Step);
                yield return null;
            }
            float time = 3;
            while (time > 0)
            {
                time -= Time.deltaTime;
                yield return null;
            }
            while (image.color.a > 0)
            {
                image.color = new Color(1, 1, 1, image.color.a - Step);
                yield return null;
            }
            imageInProcces = false;
            yield break;
        }
        private static void Show(Achievement achievement)
        {
            achievement.PlaySound();
            queue.Remove(achievement);
            if (textMeshProUGUI != null)
            {
                textMeshProUGUI.text = LocalizationManager.Instance.GetLocalizedText(achievement.nameKey);
                textMeshProUGUI.color = new Color(1, 1, 1, 0);
                textInProcces = true;
                hud.StartCoroutine(ShowText());
            }
            if (image != null)
            {
                image.color = new Color(1, 1, 1, 0);
                imageInProcces = true;
                hud.StartCoroutine(ShowImage());
            }
        }
        public static void AddToQueue(Achievement achievement) => queue.Add(achievement);
    }
}
