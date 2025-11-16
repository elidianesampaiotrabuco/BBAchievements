using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
namespace BBAchievements
{
    [HarmonyPatch]
    class AchievementsPatches
    {
        private static List<Items> collected = new List<Items>();
        private static string[] recordList = new string[] { "obs32.exe", "obs64.exe", "obs.exe", "xsplit.core.exe", "livehime.exe", "pandatool.exe", "yymixer.exe", "douyutool.exe", "huomaotool.exe" };
        private static Dictionary<string, bool> datas = new Dictionary<string, bool>();
        [HarmonyPatch(typeof(GameLoader), "Initialize")]
        [HarmonyPostfix]
        private static void SetupData()
        {
            collected = new List<Items> { Items.None, Items.Map, Items.Points };
            datas = new Dictionary<string, bool>
            {
                { "BBA_StealthPerfection1", true },
                { "BBA_StealthPerfection2", true }
            };
        }
        [HarmonyPatch(typeof(PlayerManager), "SetHidden")]
        [HarmonyPostfix]
        private static void InvisibleAchievement(bool value)
        {
            if (value)
                Achievement.Get("BBA_Disappearance").Unlock();
        }
        [HarmonyPatch(typeof(PlaceholderWinManager), "BeginPlay")]
        [HarmonyPrefix]
        private static void OnWin()
        {
            Achievement.Get("BBA_VictorySong").Unlock();
            foreach (var data in datas)
            {
                if (data.Value)
                    Achievement.Get(data.Key).Unlock();
            }
            try
            {
                if (Achievement.TryGet("BBA_NotBBCR", out Achievement achievement))
                {
                    if (CoreGameManager.Instance.inventoryChallenge && CoreGameManager.Instance.mapChallenge && CoreGameManager.Instance.timeLimitChallenge)
                        achievement.Unlock();
                }
            }
            catch (MissingFieldException) { }
            if (CoreGameManager.Instance.GetPlayer(0).itm.InventoryFull())
                Achievement.Get("BBA_Collector").Unlock();
        }
        [HarmonyPatch(typeof(Pickup), "Clicked")]
        [HarmonyPrefix]
        private static void AchievementFullSet(Pickup __instance, int player)
        {
            if (CoreGameManager.Instance.GetPoints(player) < __instance.price && !__instance.free)
                return;
            if (!__instance.item.itemType.IsModded() && !collected.Contains(__instance.item.itemType))
            {
                collected.Add(__instance.item.itemType);
            }
            if (collected.Count == Helpers.All<Items>().Length)
                Achievement.Get("BBA_FullSet").Unlock();

        }
        [HarmonyPatch(typeof(DrReflex), "Hammer")]
        [HarmonyPrefix]
        private static void SquishTest(Entity entity)
        {
            if (entity == null)
                return;
            if (entity.TryGetComponent<LookAtGuy>(out _))
                Achievement.Get("BBA_ReflexTest").Unlock();
        }
        [HarmonyPatch(typeof(BaseGameManager), "BeginPlay")]
        [HarmonyPrefix]
        private static void OBSAchievement(BaseGameManager __instance)
        {
            try
            {
                Achievement a = Achievement.Get("BBA_HelloViewers");
                if (a.Unlocked)
                    return;
                if (__instance.playStarted)
                {
                    if (Process.GetProcesses().Count(x => recordList.Contains(x.ProcessName.ToLower() + ".exe")) > 0)
                        a.Unlock();
                }
            }
            catch(InvalidOperationException) { }
        }
        [HarmonyPatch(typeof(Baldi_StateBase), "PlayerInSight")]
        [HarmonyPostfix]
        private static void StealthPerfectionFail()
        {
            datas["BBA_StealthPerfection1"] = false;
            datas["BBA_StealthPerfection2"] = false;
        }
        [HarmonyPatch(typeof(Baldi), "Hear")]
        [HarmonyPrefix]
        private static void StealthPerfection2Fail(int value)
        {
            datas["BBA_StealthPerfection2"] = false;
        }
        [HarmonyPatch(typeof(ITM_Scissors), "Use")]
        [HarmonyPrefix]
        private static void MultitaskingAchievement(PlayerManager pm)
        {
            if (pm.jumpropes.Count > 0 && Gum.playerGum.Count > 0)
                Achievement.Get("BBA_Multitask").Unlock();
        }
        [HarmonyPatch(typeof(DetentionRoomFunction), "Activate")]
        [HarmonyPrefix]
        private static void GetDetention(float time)
        {
            if (time == 99)
                Achievement.Get("BBA_99Seconds").Unlock();
        }
    }
}
