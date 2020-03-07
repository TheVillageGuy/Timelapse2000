using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace RimworldRendererMod
{
    [HarmonyPatch(typeof(OptionListingUtility), "DrawOptionListing"), StaticConstructorOnStartup]
    class MenuOption
    {
        static void Prefix(List<ListableOption> optList)
        {
            if (optList.Count == 0)
            {
                return;
            }
            if (optList[0].GetType() != typeof(ListableOption_WebLink))
            {
                return;
            }

            optList.Add(new ListableOption_WebLink("MainMenuOptionLabel".Translate(), delegate () {
                Find.WindowStack.Add(new UI_Dialog());
            }, IconMain));
        }

        public static readonly Texture2D Icon = ContentFinder<Texture2D>.Get("Icon");
        public static readonly Texture2D IconMain = ContentFinder<Texture2D>.Get("Icon Main");
        public static readonly Texture2D PRInfo = ContentFinder<Texture2D>.Get("PRInfo");
        public static readonly Texture2D ImagesFolderInfo = ContentFinder<Texture2D>.Get("ImagesFolderInfo");
    }
}
