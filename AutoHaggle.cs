using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.Trade;
using PotionCraft.ObjectBased.Haggle;
using UnityEngine;

namespace NagaseYami.AutoHaggle
{
    [BepInPlugin("com.nagaseyami.mod.potioncraft.autohaggle", "Auto Haggle", "1.0.0")]
    [BepInProcess("Potion Craft.exe")]
    public class AutoHaggle : BaseUnityPlugin
    {
        private void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(Patch), "com.nagaseyami.mod.potioncraft.autohaggle");
            Logger.LogInfo("Auto haggle (by NagaseYami) is loaded!");
        }

        private static class Patch
        {
            private static List<BonusInfo> bonuses;

            private static void GetCurrentBonuses()
            {
                var currentBonuses = AccessTools.FieldRefAccess<TradeManager, Haggle>(Managers.Trade, "haggle")
                    .currentBonuses;
                bonuses = currentBonuses.GetRange(1, currentBonuses.Count - 2);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(Haggle), nameof(Haggle.OnPointerClick))]
            private static void HaggleOnPointerClickPrefix(Haggle __instance)
            {
                if (__instance.state == HaggleState.DifficultyNotSelected) GetCurrentBonuses();
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Pointer), "Update")]
            private static void PointerUpdatePostfix(Pointer __instance)
            {
                var bonusInfo = bonuses?.FirstOrDefault(info =>
                    Mathf.Abs(info.bonusComponent.Position - __instance.Position) <= info.size / 2.0);
                if (bonusInfo != null)
                {
                    AccessTools.Method(typeof(Pointer), "CheckBonuses").Invoke(__instance, null);
                    GetCurrentBonuses();
                }
            }
        }
    }
}