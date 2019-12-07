using System;
using System.IO;
using Verse;

namespace RimworldRendererMod
{
    public class MyModSettings : ModSettings
    {
        public string DefaultBaseImagesPath = null;
        public string DefaultSaveLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RimWorld Timelapses");
        public string FileExtension = ".mp4";

        public override void ExposeData()
        {
            Scribe_Values.Look(ref DefaultBaseImagesPath, "RR_DefaultBaseImagesPath", string.Empty);
            Scribe_Values.Look(ref DefaultSaveLocation, "RR_DefaultSaveLocation", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RimWorld Timelapses"));
            Scribe_Values.Look(ref FileExtension, "RR_FileExtension", ".mp4");
        }
    }
}
