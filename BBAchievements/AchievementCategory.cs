using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using MTM101BaldAPI.OptionsAPI;
using MTM101BaldAPI.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BBAchievements
{
    class AchievementOptionsData
    {
        public StandardMenuButton text;
        public StandardMenuButton reset;
    }
    class AchievementCategory : CustomOptionsCategory
    {
        // z = 90 - верх
        private static int currentIndex = 0;
        private static List<float> y = new List<float>() 
        {
            50,
            5,
            -40,
            -85,
            -130
        };
        private TextMeshProUGUI textMesh;
        private List<List<AchievementOptionsData>> achievements;
        private void UpdateText()
        {
            if (textMesh == null)
                return;
            textMesh.autoSizeTextContainer = true;
            textMesh.text = Achievement.All.Where(x => x.Unlocked).Count() + "/" + Achievement.All.Count();
        }
        private void ShowAchievements()
        {
            foreach (var achievement in achievements)
            {
                foreach (AchievementOptionsData data in achievement)
                {
                    data.text.gameObject.SetActive(false);
                    data.reset.gameObject.SetActive(false);
                }
            }
            foreach (AchievementOptionsData ach in achievements[currentIndex])
            {
                ach.text.gameObject.SetActive(true);
                ach.reset.gameObject.SetActive(true);
            }
        }
        private void ChangeIndex(bool state)
        {
            if (state) currentIndex++;
            else currentIndex--;
            if (currentIndex < 0) currentIndex = achievements.Count - 1;
            if (currentIndex >= achievements.Count) currentIndex = 0;
        }
        public override void Build()
        {
            List<AchievementOptionsData> datas = new List<AchievementOptionsData>();
            currentIndex = 0;
            CreateButton(() =>
            {
                ChangeIndex(false);
                ShowAchievements();
            }, BasePlugin.AssetManager.Get<Sprite>("MenuArrowSheet_2"), BasePlugin.AssetManager.Get<Sprite>("MenuArrowSheet_0"), "ArrowUp", new Vector3(130, 50, 0)).transform.rotation = Quaternion.Euler(0, 0, 270);
            CreateButton(() =>
            {
                ChangeIndex(true);
                ShowAchievements();
            }, BasePlugin.AssetManager.Get<Sprite>("MenuArrowSheet_2"), BasePlugin.AssetManager.Get<Sprite>("MenuArrowSheet_0"), "ArrowUp", new Vector3(130, -130, 0)).transform.rotation = Quaternion.Euler(0, 0, 90);
            int index = 0;
            textMesh = CreateText("AchievementsCounter", "", new Vector3(136, -160, 0), BaldiFonts.ComicSans24, TextAlignmentOptions.Right, Vector2.one, Color.black);
            foreach (Achievement achievement in Achievement.All)
            {
                string name = LocalizationManager.Instance.GetLocalizedText(achievement.nameKey);
                if (!achievement.Unlocked && achievement.Hide)
                    name = LocalizationManager.Instance.GetLocalizedText("BBA_Secret");
                StandardMenuButton text = CreateTextButton(() => { }, achievement.nameKey, name, new Vector3(-80, y[index], 0), BaldiFonts.ComicSans18,
                    TMPro.TextAlignmentOptions.Left, Vector2.one, new Color(0, 0, 0, 0.5f));
                StandardMenuButton reset = CreateButton(() => { 
                    achievement.Reset(); 
                    text.text.color = new Color(0, 0, 0, 0.5f);
                    UpdateText();
                }, BasePlugin.AssetManager.Get<Sprite>("BackArrow1"), BasePlugin.AssetManager.Get<Sprite>("BackArrow2"), "Reset", new Vector3(90, y[index], 0));
                text.text.autoSizeTextContainer = true;
                string desc = LocalizationManager.Instance.GetLocalizedText(achievement.Description) + "\nMod: " + achievement.GUID;
                if (achievement.Unlocked)
                {
                    text.text.color = new Color(0, 0, 0, 1);
                }
                datas.Add(new AchievementOptionsData()
                {
                    reset = reset,
                    text = text,
                });
                AddTooltip(reset, "BBA_Reset");
                if (!achievement.Unlocked && achievement.Hide)
                    desc = LocalizationManager.Instance.GetLocalizedText("BBA_Secret_Desc");
                AddTooltip(text, desc);
                index++;
                if (index >= 5)
                {
                    index = 0;
                }
            }
            UpdateText();
            achievements = datas.SplitList(5);
            ShowAchievements();
        }
    }
}
