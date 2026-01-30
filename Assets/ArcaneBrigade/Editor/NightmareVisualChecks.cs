using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class NightmareVisualChecks
    {
        public static void Run()
        {
            var notes = new List<string>();
            void Check(bool value, string caption)
            {
                if (!value) throw new InvalidOperationException("Nightmare visuals: " + caption);
                notes.Add("PASS " + caption);
            }
            float previous = 2;
            float[] expectedAmbient = { .92f, .7125f, .505f, .2975f, .09f };
            for (int depth = 0; depth <= 4; depth++)
            {
                float ambient = NightmareLighting.AmbientFor(depth, 0);
                Check(ambient < previous && Mathf.Abs(ambient - expectedAmbient[depth]) < .0001f, "Progressive nightmare ambient " + depth);
                Check(NightmareLighting.AmbientFor(depth, 1) > ambient, "Dawn restores light " + depth);
                previous = ambient;
            }
            Check(NightmareLighting.Falloff(Vector2.zero, 3) == 1, "Light centre is continuous");
            Check(NightmareLighting.Falloff(Vector2.right * 3, 3) < .1f, "Soft source falloff");
            Check(NightmareLighting.Falloff(Vector2.one, 0) == 0, "Disabled light is finite");
            Check(NightmareLighting.Falloff(Vector2.right, 3) == NightmareLighting.Falloff(Vector2.left, 3), "Stable symmetric light energy");
            Check(NightmareDeparture.Travel(-1, 0, false) == 0 && NightmareDeparture.Travel(10, 0, false) == 1, "Departure travel bounds");
            Check(NightmareDeparture.Travel(2, 0, false) > NightmareDeparture.Travel(2, 3, false), "Departure party is staggered");
            Check(NightmareDeparture.Travel(0, 0, true) == NightmareDeparture.Travel(100, 5, true), "Reduced departure is stationary");
            Check(NightmareDeparture.Visibility(0) == 1 && NightmareDeparture.Visibility(.88f) == 1 && NightmareDeparture.Visibility(1) == 0, "Party stays opaque until entering the real doorway");
            for (int index = 0; index < 8; index++)
            {
                Vector2 position = NightmareLighting.LandmarkPosition(index, true, new Vector2(17, 11));
                Check(Mathf.Abs(position.x) < 8 && Mathf.Abs(position.y) < 5, "Shop landmark inside composed view " + index);
            }
            Rect tooltip = RunHud.ShopTooltipBounds(600);
            Check(tooltip.y >= 845 && tooltip.yMax <= 1030, "Tooltip reserves price band and safe bottom");
            NightmareAssets assets = NightmareAssets.Current;
            Check(assets.GrandStage.Length == 4 && System.Array.TrueForAll(assets.GrandStage, sprite => sprite != null), "Four original finale architecture layers imported");
            Check(NightmareGrandStage.StrengthFor(1) == 0 && NightmareGrandStage.StrengthFor(20) == 1, "Final hall has a distinct maximal composition");
            Check(NightmareGrandStage.StrengthFor(9) < NightmareGrandStage.StrengthFor(17), "Architecture builds gradually before finale");
            Check(assets.Customer.Body.texture.name == "LastCustomerIdle_v4", "Larger-cell cartoon customer idle art is active");
            Check(assets.CustomerFrames[4].texture.name == "LastCustomerActions_v4", "Separate four-pose customer attack atlas is active");
            Check(assets.Scenery[0].texture.name == "NightmareProps_v2", "Clean cartoon landmarks are active");
            Check(assets.CustomerFrames[8].texture.name == "LastCustomerOrgans_v2", "Matching boss anatomy is active");
            Directory.CreateDirectory("PreviewExports/Nightmare/Lighting");
            File.WriteAllLines("PreviewExports/Nightmare/Lighting/Focused_Checks.txt", notes);
            Debug.Log("NIGHTMARE_VISUAL_CHECKS " + notes.Count + " PASS");
        }
        public static void Gate()
        {
            NightmareBuilder.Build(); WindowsIntegrationBuilder.Build();
        }
    }
}
