using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimworldRendererMod
{
    [StaticConstructorOnStartup]
    class UI_Dialog : Window
    {
        public static string Status = "Waiting...";
        public static string ETA = "---";
        public static float ProgressBarPercentage = 0f;
        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(800f, Math.Min(800, UI.screenHeight * 0.75f));
            }
        }


        public UI_Dialog()
        {

        }

        public override void DoWindowContents(Rect area)
        {
            float y = 0f;

            Text.Anchor = TextAnchor.UpperCenter;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0, y, area.width, 60f), new GUIContent("ModDisplayName".Translate(), MenuOption.Icon, "By Epicguru (James B)"));
            y += 60f;

            if (!RimworldRendererMod.Is64)
            {
                Text.Anchor = TextAnchor.UpperLeft;
                Label($"<color=red>{"Error64Bit".Translate()}\n[Debug info]\nOperating system: {SystemInfo.operatingSystem}\nIntPtr size: {IntPtr.Size}</color>");
                // Close button...
                GUI.color = Color.white;
                Rect rect4 = new Rect(area.width - 120, area.height - 35f, 120, 35f);
                if (Widgets.ButtonText(rect4, "UI_Close".Translate(), true, false, true))
                {
                    Close();
                }
                return;
            }

            if (Widgets.ButtonText(new Rect(0f, y, 250f, 40f), "Start"))
            {
                Runner.StartRender();
            }
            y += 50f;

            Label($"{SystemInfo.operatingSystem}");
            Label($"{(IntPtr.Size != 8 ? "64 bit" : "32 bit")}");

            Text.Anchor = TextAnchor.MiddleLeft;
            Label($"Status: {Status}");            

            Widgets.FillableBarLabeled(new Rect(0, y, area.width, 30f), ProgressBarPercentage, 50, $"{ProgressBarPercentage * 100f:F0}%");
            y += 30f;

            if (Runner.IsRendering)
            {
                Label($"ETA: {ETA}");
                Label($"<color=green>{"SafeContinueMessage".Translate()}</color>");
            }

            // Close button...
            GUI.color = Color.white;
            Rect rect3 = new Rect(area.width - 120, area.height - 35f, 120, 35f);
            if (Widgets.ButtonText(rect3, "UI_Close".Translate(), true, false, true))
            {
                Close();
            }

            Text.Anchor = TextAnchor.UpperLeft;

            void Label(string s)
            {
                float h = Text.CalcHeight(s, area.width);
                Widgets.Label(new Rect(0, y, area.width, h), s);
                y += h + 0f;
            }
        }
    }
}
