using System;
using System.Collections.Generic;
using System.IO;
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
        private static bool confirmClose = false;

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
        public static int ImagesPerSecond = 10;
        private static string IPSS = "8"; // Images per second string.
        private static string ResXS = "1920", ResYS = "1080";

        public static InterpolationMode CurrentInterpolationMode = InterpolationMode.HighQualityBicubic;
        public static VideoCodec CurrentCodec = VideoCodec.Default;
        public static int Bitrate { get { return (int)Math.Ceiling(bitsPerSecondRaw); } }
        private static float bitsPerSecondRaw = 1000 * 1000 * 5;
        private static Vector2 scrollPos = Vector2.zero;
        private static Vector2 scrollPos2 = Vector2.zero;
        private static bool inHelp = false;
        private static int LastHeight;

        public static void UponRenderComplete()
        {
            confirmClose = true;
        }

        private string PrettyBitrate(float bps)
        {
            if (bps < 1000)
            {
                return $"{bps:F0} bps";
            }
            else if (bps < 1000 * 1000)
            {
                return $"{bps / 1000f:F0} kbps";
            }
            else
            {
                return $"{bps / (1000f * 1000f):F1} mbps";
            }
        }

        public override void DoWindowContents(Rect area)
        {
            float y = 0f;

            Text.Anchor = TextAnchor.UpperCenter;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0, y, area.width, 40f), new GUIContent(" " + "ModDisplayName".Translate(), MenuOption.Icon));
            y += 36f;
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(0, y, area.width, 30f), "by Epicguru/TheVillageGuy");
            y += 40f;
            Text.Font = GameFont.Medium;

            if (!RimworldRendererMod.Is64)
            {
                Text.Anchor = TextAnchor.UpperLeft;
                Label($"<color=red>{"UI_Error64Bit".Translate()}\n[Debug info]\nOperating system: {SystemInfo.operatingSystem}\nIntPtr size: {IntPtr.Size}</color>");
                // Close button...
                GUI.color = Color.white;
                Rect rect4 = new Rect(area.width - 120, area.height - 35f, 120, 35f);
                if (Widgets.ButtonText(rect4, "UI_Close".Translate(), true, false, true))
                {
                    Close();
                }
                return;
            }

            if (inHelp)
            {
                DrawHelpMenu(area, ref y);

                GUI.color = Color.white;
                Rect rect4 = new Rect(area.width - 120, area.height - 35f, 120, 35f);
                if (Widgets.ButtonText(rect4, "UI_Back".Translate(), true, false, true))
                {
                    inHelp = false;
                }
                Text.Anchor = TextAnchor.UpperLeft;

                return;
            }

            if (!confirmClose && !Runner.IsRendering && Widgets.ButtonText(new Rect(0f, y, 250f, 40f), "UI_Start".Translate()))
            {
                Runner.StartRender();
            }
            GUI.color = Color.green;
            if(!confirmClose && Widgets.ButtonText(new Rect(area.width - 100f, y + 2.5f, 100f, 35f), "UI_Help".Translate()))
            {
                inHelp = true;
            }
            GUI.color = Color.white;
            y += 50f;

            Text.Anchor = TextAnchor.MiddleLeft;
            Label($"Status: {(Status.Length > 120 ? Status.Substring(0, 120) + "..." : Status)}");

            if (confirmClose)
            {
                if (!Status.StartsWith("Error"))
                {
                    Label($"<color=green>{"UI_SaveConfirmation".Translate(Runner.SavePath)}</color>");
                }
                if(Widgets.ButtonText(new Rect(0, y, 200, 30), "UI_Confirm".Translate()))
                {
                    confirmClose = false;
                }

                Text.Anchor = TextAnchor.UpperLeft;
                return;
            }

            Widgets.FillableBarLabeled(new Rect(0, y, area.width, 30f), ProgressBarPercentage, 50, $"{ProgressBarPercentage * 100f:F0}%");
            y += 40f;

            if (Runner.IsRendering)
            {
                Label($"ETA: {ETA}");
                Label($"<color=green>{"UI_SafeContinueMessage".Translate()}</color>");
            }
            else
            {
                y += inputsPreSapce;
                Widgets.BeginScrollView(new Rect(0, y, area.width, area.height - y - 40), ref scrollPos, new Rect(0, y, 100, LastHeight));
                y += inputsPreSapce * 0.5f;

                string s = $"{"UI_ImagesFolder".Translate()}:";
                float w = Text.CalcSize(s).x;
                Widgets.Label(new Rect(0, y, w, 30), s);
                bool dirExists = Directory.Exists(SourceFolder);
                bool dirContainsImages = false;
                List<string> suggestions = null;
                if (dirExists)
                {
                    dirContainsImages = ContainsImages(SourceFolder);
                    
                    if (!dirContainsImages)
                    {
                        try
                        {
                            suggestions = new List<string>();
                            foreach (var dir in Directory.GetDirectories(SourceFolder))
                            {
                                if(ContainsImages(dir))
                                    suggestions.Add(new DirectoryInfo(dir).Name);
                            }
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
                string dirString = dirExists ? (dirContainsImages ? $"<color=green>{"UI_Good".Translate()}</color>" : $"<color=yellow>{"UI_NoImgInFolder".Translate()}</color>") : $"<color=red>{"UI_FolderNotFound".Translate()}</color>";                
                float width = Text.CalcSize(dirString).x + 5;
                SourceFolder = Widgets.TextField(new Rect(w + 5, y, area.width - width - 30 - w, textInputHeights), SourceFolder);                

                // Folder path status.
                Text.Font = GameFont.Medium;
                Widgets.Label(new Rect(area.width - width - 20, y, width + 30, textInputHeights), dirString);
                Text.Font = GameFont.Medium;
                y += textInputHeights + 15;

                // Suggestions for folders.
                if (dirExists && !dirContainsImages && suggestions != null && suggestions.Count > 0)
                {
                    Widgets.Label(new Rect(0, y, width + 30, 32), $"<color=yellow>{"UI_PerhapsYouMeant".Translate()}:</color>");
                    y += 32 + 5;
                    foreach (var dir in suggestions)
                    {
                        if(Widgets.ButtonText(new Rect(0, y, width + 30, 32), $"...{dir}"))
                        {
                            SourceFolder = Path.Combine(SourceFolder, dir);
                        }
                        y += 35;
                    }
                }

                s = $"{"UI_Resolution".Translate()}:";
                w = Text.CalcSize(s).x;
                Widgets.Label(new Rect(0, y, w, 30), s);
                Widgets.TextFieldNumeric(new Rect(w + 5, y, resolutionInputWidths, 30), ref ResX, ref ResXS, 1, 9999);
                Widgets.Label(new Rect(w + resolutionInputWidths + 10, y, 20, 32), "x");
                Widgets.TextFieldNumeric(new Rect(w + resolutionInputWidths + 25, y, resolutionInputWidths, 30), ref ResY, ref ResYS, 1, 9999);
                Widgets.Label(new Rect(w + resolutionInputWidths * 2 + 30, y, 120, 32), $"{"UI_Pixels".Translate()}:");
                y += 40;

                s = $"{"UI_Bitrate".Translate()}:";
                w = Text.CalcSize(s).x;
                Widgets.Label(new Rect(0, y, w, 30), s);
                bitsPerSecondRaw = Widgets.HorizontalSlider(new Rect(w + 5, y + 12, area.width - 120 - w, 30), bitsPerSecondRaw, 1000, 1000 * 1000 * 120);
                Widgets.Label(new Rect(area.width - 110, y, 120, 30), PrettyBitrate(bitsPerSecondRaw).ToString());
                y += 40;

                Text.Anchor = TextAnchor.MiddleLeft;
                Label($"{"UI_QualitySuggestions".Translate()}");

                s = $"{"UI_ImagesPerSecond".Translate()}:";
                w = Text.CalcSize(s).x;
                Widgets.Label(new Rect(0, y, w, 30), s);
                Widgets.IntEntry(new Rect(w, y, 200, 30), ref ImagesPerSecond, ref IPSS);
                if (ImagesPerSecond < 1)
                {
                    IPSS = "1";
                    ImagesPerSecond = 1;
                }
                if (ImagesPerSecond > 120)
                {
                    ImagesPerSecond = 120;
                    IPSS = "120";
                }
                y += 40f;

                s = $"{"UI_Sampling".Translate()}:";
                w = Text.CalcSize(s).x;
                Widgets.Label(new Rect(0, y, w, 30), s);
                Widgets.Dropdown(new Rect(w, y, 250, 30), InterpolationMode.HighQualityBicubic, (enumThing) => { return enumThing.ToString(); }, (thing) => this.DropdownGenerator(thing), CurrentInterpolationMode.ToString());
                y += 40f;

                s = $"{"UI_VideoCodec".Translate()}:";
                w = Text.CalcSize(s).x;
                Widgets.Label(new Rect(0, y, w, 30), new GUIContent(s, "The codec to use in the ouput video. Only change if you know what you are doing."));
                Widgets.Dropdown(new Rect(w, y, 250, 30), VideoCodec.Default, (enumThing) => { return enumThing.ToString(); }, (thing) => this.DropdownGenerator2(thing), CurrentCodec.ToString());
                y += 40f;

                Widgets.EndScrollView();
            }

            // Close button...
            GUI.color = Color.white;
            Rect rect3 = new Rect(area.width - 120, area.height - 35f, 120, 35f);
            if (Widgets.ButtonText(rect3, "UI_Close".Translate(), true, false, true))
            {
                Close();
            }
            Text.Anchor = TextAnchor.UpperLeft;

            LastHeight = (int)y + 10;

            void Label(string s)
            {
                float h = Text.CalcHeight(s, area.width);
                Widgets.Label(new Rect(0, y, area.width, h), s);
                y += h + 5f;
            }

            bool ContainsImages(string dir)
            {
                try
                {
                    foreach (var path in Directory.GetFiles(dir))
                    {
                        var info = new FileInfo(path);
                        if (info.Extension.ToLower() == ".png" || info.Extension.ToLower() == ".jpg")
                        {
                            return true;
                        }
                    }
                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }

        private void DrawHelpMenu(Rect area, ref float yThing)
        {
            float y = yThing;

            Text.Anchor = TextAnchor.UpperLeft;

            Widgets.BeginScrollView(new Rect(0, y, area.width, area.height - y - 40), ref scrollPos2, new Rect(0, y, 100, 970));
            Label("Help!", true);
            y += 15f;
            Label("How to create a timelapse:", true);
            Label("1.", true);
            Label("Locate the folder that contains the images.\nIf you are using the Progress Renderer mod, then you " +
                "should check the mod settings to see where the images are being saved:");
            Widgets.DrawTextureFitted(new Rect(0, y, 664, 515), MenuOption.PRInfo, 1f);
            y += 520;
            Label("Copy the full path to the images folder, Such as \"E:\\RimworldRenders\\MyWorld\"");
            Label("2.", true);
            Label("Paste the folder path into this field in the other menu:");
            Widgets.DrawTextureFitted(new Rect(0, y, 443, 37), MenuOption.ImagesFolderInfo, 1f);
            y += 45;

            Label("3.", true);
            Label("Press the Start button and wait for the video to be created!");

            Widgets.EndScrollView();

            yThing = y;
            void Label(string s, bool large = false)
            {
                if (large)
                    Text.Font = GameFont.Medium;
                else
                    Text.Font = GameFont.Small;
                float h = Text.CalcHeight(s, area.width);
                Widgets.Label(new Rect(0, y, area.width, h), s);
                y += h + 5f;
            }
        }

        private IEnumerable<Widgets.DropdownMenuElement<string>> DropdownGenerator(InterpolationMode enumThing)
        {
            foreach (var item in Enum.GetValues(typeof(InterpolationMode)))
            {
                if ((InterpolationMode)item == InterpolationMode.Invalid)
                    continue;

                yield return new Widgets.DropdownMenuElement<string>()
                {
                    payload = "",
                    option = new FloatMenuOption((InterpolationMode)item == InterpolationMode.HighQualityBicubic ? item.ToString() + " (Recommended)" : item.ToString(), () =>
                    {
                        CurrentInterpolationMode = (InterpolationMode)item;
                    })
                };
            }
        }

        private IEnumerable<Widgets.DropdownMenuElement<string>> DropdownGenerator2(VideoCodec enumThing)
        {
            foreach (var item in Enum.GetValues(typeof(VideoCodec)))
            {
                yield return new Widgets.DropdownMenuElement<string>()
                {
                    payload = "",
                    option = new FloatMenuOption(item.ToString(), () =>
                    {
                        CurrentCodec = (VideoCodec)item;
                    })
                };
            }
        }
    }

    //
    // Summary:
    //     The System.Drawing.Drawing2D.InterpolationMode enumeration specifies the algorithm
    //     that is used when images are scaled or rotated.
    public enum InterpolationMode
    {
        //
        // Summary:
        //     Equivalent to the System.Drawing.Drawing2D.QualityMode.Invalid element of the
        //     System.Drawing.Drawing2D.QualityMode enumeration.
        Invalid = -1,
        //
        // Summary:
        //     Specifies default mode.
        Default = 0,
        //
        // Summary:
        //     Specifies low quality interpolation.
        Low = 1,
        //
        // Summary:
        //     Specifies high quality interpolation.
        High = 2,
        //
        // Summary:
        //     Specifies bilinear interpolation. No prefiltering is done. This mode is not suitable
        //     for shrinking an image below 50 percent of its original size.
        Bilinear = 3,
        //
        // Summary:
        //     Specifies bicubic interpolation. No prefiltering is done. This mode is not suitable
        //     for shrinking an image below 25 percent of its original size.
        Bicubic = 4,
        //
        // Summary:
        //     Specifies nearest-neighbor interpolation.
        NearestNeighbor = 5,
        //
        // Summary:
        //     Specifies high-quality, bilinear interpolation. Prefiltering is performed to
        //     ensure high-quality shrinking.
        HighQualityBilinear = 6,
        //
        // Summary:
        //     Specifies high-quality, bicubic interpolation. Prefiltering is performed to ensure
        //     high-quality shrinking. This mode produces the highest quality transformed images.
        HighQualityBicubic = 7
    }

    //
    // Summary:
    //     Enumeration of some video codecs from FFmpeg library, which are available for
    //     writing video files.
    public enum VideoCodec
    {
        //
        // Summary:
        //     Default video codec, which FFmpeg library selects for the specified file format.
        Default = -1,
        //
        // Summary:
        //     MPEG-4 part 2.
        MPEG4 = 0,
        //
        // Summary:
        //     Windows Media Video 7.
        WMV1 = 1,
        //
        // Summary:
        //     Windows Media Video 8.
        WMV2 = 2,
        //
        // Summary:
        //     MPEG-4 part 2 Microsoft variant version 2.
        MSMPEG4v2 = 3,
        //
        // Summary:
        //     MPEG-4 part 2 Microsoft variant version 3.
        MSMPEG4v3 = 4,
        //
        // Summary:
        //     H.263+ / H.263-1998 / H.263 version 2.
        H263P = 5,
        //
        // Summary:
        //     Flash Video (FLV) / Sorenson Spark / Sorenson H.263.
        FLV1 = 6,
        //
        // Summary:
        //     MPEG-2 part 2.
        MPEG2 = 7,
        //
        // Summary:
        //     Raw (uncompressed) video.
        Raw = 8,
        //
        // Summary:
        //     FF video codec 1 lossless codec.
        FFV1 = 9,
        //
        // Summary:
        //     FFmpeg's HuffYUV lossless codec.
        FFVHUFF = 10,
        //
        // Summary:
        //     H.264/MPEG-4 Part 10.
        H264 = 11,
        //
        // Summary:
        //     H.265
        H265 = 12,
        //
        // Summary:
        //     H.264/MPEG-4 Part 10.
        Theora = 13,
        //
        // Summary:
        //     VP-8.
        VP8 = 14,
        //
        // Summary:
        //     VP-9.
        VP9 = 15
    }
}
