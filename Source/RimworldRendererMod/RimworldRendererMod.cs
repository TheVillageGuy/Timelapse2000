using HarmonyLib;
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
        public static MyModSettings Settings;

        public RimworldRendererMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<MyModSettings>();

            var harmony = new Harmony("TVG.RimworldRendererMod");
            harmony.PatchAll();

            Log.Message("Patched in Timelapse 2023 1.4 Option should now be in main menu.");

            BaseFolder = base.Content.RootDir;
            Verse.Log.Message($"Timelapse2023 base dir: {BaseFolder}");

            // Load from settings into UI.
            UI_Dialog.SourceFolder = RimworldRendererMod.Settings.DefaultBaseImagesPath;
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);
            Settings.DefaultBaseImagesPath = listing.TextEntryLabeled("UI_ImagePathDir".Translate(), Settings.DefaultBaseImagesPath, 1);
            Settings.DefaultSaveLocation = listing.TextEntryLabeled("UI_DefaultSaveDir".Translate(), Settings.DefaultSaveLocation, 1);
            Settings.FileExtension = listing.TextEntryLabeled("UI_FileExtension".Translate(), Settings.FileExtension, 1);
            listing.End();

            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "ModDisplayName".Translate();
        }
    }
}
