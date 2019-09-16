using Verse;

namespace RimworldRendererMod
{
    public class MyModSettings : ModSettings
    {
        public string DefaultBaseImagesPath = string.Empty;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref DefaultBaseImagesPath, "DefaultBaseImagesPath", string.Empty);
        }
    }
}
