using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.OptionsAPI;
using MTM101BaldAPI.Registers;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace BBAchievements
{
    [BepInPlugin("rost.moment.baldiplus.achievements", "Baldi Basics Plus Achievements", "0.2")]
    public class BasePlugin : BaseUnityPlugin
    {
        public static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("Baldi Basics Plus Achievements");
        public static BasePlugin Instance { get; private set; }
        public static AssetManager AssetManager { get; private set; }
        public static string ModPath => AssetLoader.GetModPath(Instance) + "/";
        private IEnumerator Load()  
        {
            yield return 3;
            yield return "Creating assets...";
            AssetManager.Add<Sprite>("AchievementBG", AssetLoader.SpriteFromFile(ModPath + "BBA_Achievement.png", Vector2.one / 2f, 1));
            AssetManager.Add<SoundObject>("WOW", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(ModPath + "BBA_WOW.mp3"), "", SoundType.Effect, Color.clear, 0));
            for (int i = 0; i < 4; i++)
            {
                AssetManager.AddFromResources<Sprite>("MenuArrowSheet_" + i.ToString());
            }
            BasePlugin.AssetManager.AddRange(Helpers.CreateSpriteSheet(Helpers.LoadAsset<Texture2D>("BackArrow"), 2, 1).ToArray(), new string[] { "BackArrow1", "BackArrow2" });
            yield return "Creating save file...";
            new SaveFile().Initialize().Update();
            yield return "Creating achievements...";
            Achievement.Create(Info, "BBA_VictorySong", "BBA_VictorySong_Desc");
            Achievement.Create(Info, "BBA_99Seconds", "BBA_99Seconds_Desc");
            Achievement.Create(Info, "BBA_StealthPerfection1", "BBA_StealthPerfection1_Desc");
            Achievement.Create(Info, "BBA_StealthPerfection2", "BBA_StealthPerfection2_Desc");

            if (typeof(CoreGameManager).GetField("mapChallenge", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static) != null)
                Achievement.Create(Info, "BBA_NotBBCR", "BBA_NotBBCR_Desc");

            Achievement.Create(Info, "BBA_ReflexTest", "BBA_ReflexTest_Desc");
            Achievement.Create(Info, "BBA_HelloViewers", "BBA_HelloViewers_Desc", true);
            Achievement.Create(Info, "BBA_Multitask", "BBA_Multitask_Desc");
            Achievement.Create(Info, "BBA_FullSet", "BBA_FullSet_Desc");
            Achievement.Create(Info, "BBA_Collector", "BBA_Collector_Desc");
            Achievement.Create(Info, "BBA_Disappearance", "BBA_Disappearance_Desc");
        }
        private void Awake()
        {
            Harmony harmony = new Harmony("rost.moment.baldiplus.achievements");
            harmony.PatchAllConditionals();
            if (Instance == null)
            {
                Instance = this;
            }
            AssetManager = new AssetManager();
            AssetLoader.LoadLocalizationFolder(ModPath, Language.English);
            LoadingEvents.RegisterOnAssetsLoaded(Info, Load(), false);
            CustomOptionsCore.OnMenuInitialize += (x, y) =>
            {
                y.AddCategory<AchievementCategory>("Achievements");
            };
        }
    }
}