
using Harmony;
using System.IO;
using UnityEngine;
using Verse;

namespace RimworldRendererMod
{
    public class RimworldRendererMod : Mod
    {
        public static bool Is64
        {
            get
            {
                return System.IntPtr.Size == 8;
            }
        }
        public static string BaseFolder;
        public MyModSettings Settings;

        public RimworldRendererMod(ModContentPack content) : base(content)
        {
            this.Settings = GetSettings<MyModSettings>();

            var harmony = HarmonyInstance.Create("com.github.Epicguru.RimworldRendererMod");
            harmony.PatchAll();

            Log.Message("Patched in RimworldRenderer. Option should now be in main menu.");

            BaseFolder = new DirectoryInfo(base.Content.AssembliesFolder).Parent.FullName;
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
