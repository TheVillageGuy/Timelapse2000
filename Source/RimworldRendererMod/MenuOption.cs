using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimworldRendererMod
{
    [HarmonyPatch(typeof(OptionListingUtility), "DrawOptionListing"), StaticConstructorOnStartup]
    class MenuOption
    {
        static void Prefix(List<ListableOption> optList)
        {
            if (optList.Count == 0) return;
            if (optList[0].GetType() != typeof(ListableOption_WebLink)) return;

            optList.Add(new ListableOption_WebLink("RimworldRenderer".Translate(), delegate () {
                Find.WindowStack.Add(new UI_Dialog());
            }, icon));
        }

        public static readonly Texture2D icon = ContentFinder<Texture2D>.Get("Icon", true);
    }
}
