using Verse;

namespace RimworldRendererMod
{
    public class MyModSettings : ModSettings
    {
        public float TestValue = 20f;
        public bool TestToggle = false;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref TestToggle, "TestToggle");
            Scribe_Values.Look(ref TestValue, "TestValue", 20f);
        }
    }
}
