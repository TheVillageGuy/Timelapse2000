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
            //Text.Anchor = TextAnchor.MiddleLeft;
            //float y = 0;

            //Text.Font = GameFont.Medium;

            //Rect titleRect = new Rect(0, y, area.width, 30);
            //Widgets.Label(titleRect, "StartupImpactStartupTime".Translate(ProfilerBar.TimeText(StartupImpact.loadingTime)));
            //y += titleRect.height;

            //Rect profileRect = new Rect(0, y, area.width - 16, 46);
            //ProfilerBar.Draw(profileRect, metricsTotal, categoriesTotal, StartupImpact.loadingTime, categoryHints, categoryColors, defaultColor);
            //y += profileRect.height + 4;

            //Rect nonmodsTitleRect = new Rect(0, y, area.width, 30);
            //Widgets.Label(nonmodsTitleRect, "StartupImpactStartupNonmods".Translate(ProfilerBar.TimeText(basegameLoadingTime)));
            //y += nonmodsTitleRect.height;

            //Rect nonmodsProfileRect = new Rect(0, y, area.width - 16, 46);
            //ProfilerBar.Draw(nonmodsProfileRect, StartupImpact.baseGameProfiler.metrics, categoriesNonmods, basegameLoadingTime, categoryHintsNonmods, categoryColorsNonmods, defaultColor);
            //y += nonmodsProfileRect.height + 8;

            //Rect modsTitleRect = new Rect(0, y, area.width, 30);
            //Widgets.Label(modsTitleRect, "StartupImpactStartupMods".Translate(ProfilerBar.TimeText(modsLoadingTime)));
            //y += titleRect.height + 8;
            //Text.Font = GameFont.Small;

            //table.Start(0, y, area.width, area.height - y - 55);

            //int row = 0;
            //foreach (ModInfo info in mods)
            //{
            //    if (Widgets.ButtonImage(table.cell(0, row), eye, info.hideInUi ? Color.white : Color.grey))
            //    {
            //        info.hideInUi = !info.hideInUi;
            //        redoStats();
            //    }

            //    GUI.color = info.hideInUi ? Color.grey : Color.white;

            //    table.text(1, row, info.mod.Name);

            //    table.text(2, row, ProfilerBar.TimeText(info.profile.totalImpact));

            //    ProfilerBar.Draw(table.cell(3, row), info.profile.metrics, categories, Math.Max(maxImpact, info.profile.totalImpact), categoryHints, categoryColors, defaultColor);

            //    row++;
            //}

            //table.Stop();


            //GUI.color = infoColor;
            //Text.Anchor = TextAnchor.LowerLeft;
            //Rect rect4 = new Rect(0, area.height - 35f, area.width - 120, 35f);
            //string verinfo = "StartupImpactVerinfo".Translate(StartupImpact.GetVersion(), StartupImpact.baseGameProfiler.profilerType.ToString().ToLowerInvariant());
            //if (failedMeasuringLoadingTime) verinfo += "StartupImpactFailedMeasuringLoadingTime".Translate();
            //Widgets.Label(rect4, verinfo);

            //GUI.color = Color.white;
            //Rect rect3 = new Rect(area.width - 120, area.height - 35f, 120, 35f);
            //if (Widgets.ButtonText(rect3, "Close".Translate(), true, false, true))
            //{
            //    Close();
            //}
            //Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}
