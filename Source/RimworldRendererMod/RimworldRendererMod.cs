
using Harmony;
using UnityEngine;
using Verse;

namespace RimworldRendererMod
{
    public class RimworldRendererMod : Mod
    {
        public MyModSettings Settings;

        public RimworldRendererMod(ModContentPack content) : base(content)
        {
            this.Settings = GetSettings<MyModSettings>();

            var harmony = HarmonyInstance.Create("com.github.Epicguru.RimworldRendererMod");
            harmony.PatchAll();

            Log.Message("Hello?>!?");
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);
            Settings.DefaultBaseImagesPath = listing.TextEntryLabeled("UI_ImagePathDir", Settings.DefaultBaseImagesPath, 1);
            listing.End();

            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "ModDisplayName".Translate();
        }
    }
}
