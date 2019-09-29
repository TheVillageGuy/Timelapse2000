using System;
using System.Collections.Generic;
using System.IO;
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
        public static string SourceFolder;


        public UI_Dialog()
        {

        }

        [TweakValue("RimworldRenderer.UI", 0f, 100f)]
        private static float textInputHeights = 32f;

        [TweakValue("RimworldRenderer.UI", 0f, 100f)]
        private static float inputsPreSapce = 20f;

        [TweakValue("RimworldRenderer.UI", 0f, 500f)]
        private static float resolutionInputWidths = 55f;

        public static int ResX = 1920, ResY = 1080;
        private static string ResXS = "1920", ResYS = "1080";

        private static float bitsPerSecondRaw = 1000 * 1000;

        private string PrettyBitrate(float bps)
        {
            if(bps < 1000)
            {
                return $"{bps:F0} bps";
            }
            else if(bps < 1000 * 1000)
            {
                return $"{bps/1000f:F0} kbps";
            }
            else
            {
                return $"{bps / (1000f * 1000f):F0} mbps";
            }
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

            Text.Anchor = TextAnchor.MiddleLeft;
            Label($"Status: {Status}");            

            Widgets.FillableBarLabeled(new Rect(0, y, area.width, 30f), ProgressBarPercentage, 50, $"{ProgressBarPercentage * 100f:F0}%");
            y += 40f;

            if (Runner.IsRendering)
            {
                Label($"ETA: {ETA}");
                Label($"<color=green>{"SafeContinueMessage".Translate()}</color>");
            }
            else
            {
                y += inputsPreSapce;

                Label("Folder containing images:");
                string dirString = Directory.Exists(SourceFolder) ? "<color=green>Good</color>" : "<color=red>Folder not found</color>";
                float width = Text.CalcSize(dirString).x;
                SourceFolder = Widgets.TextField(new Rect(0, y, area.width - width - 30, textInputHeights), SourceFolder);
                Text.Font = GameFont.Medium;
                Widgets.Label(new Rect(area.width - width - 25, y, width + 30, textInputHeights), dirString);
                Text.Font = GameFont.Medium;
                y += textInputHeights + 5;

                Label("Video Resolution:");
                Widgets.TextFieldNumeric(new Rect(0, y, resolutionInputWidths, 30), ref ResX, ref ResXS, 1, 9999);
                Widgets.Label(new Rect(resolutionInputWidths + 5, y, 20, 32), "x");
                Widgets.TextFieldNumeric(new Rect(resolutionInputWidths + 20, y, resolutionInputWidths, 30), ref ResY, ref ResYS, 1, 9999);
                Widgets.Label(new Rect(resolutionInputWidths * 2 + 25, y, 120, 32), "pixels.");
                y += 40;

                Label("Video Bitrate:");
                bitsPerSecondRaw = Widgets.HorizontalSlider(new Rect(0, y, area.width - 120, 30), bitsPerSecondRaw, 1000, 1000 * 1000 * 40);
                Widgets.Label(new Rect(area.width - 120, y, 120, 30), PrettyBitrate(bitsPerSecondRaw).ToString());
                y += 40;
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
                y += h + 5f;
            }
        }
    }
}
