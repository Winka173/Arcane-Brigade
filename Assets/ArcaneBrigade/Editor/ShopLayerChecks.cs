using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class ShopLayerChecks
    {
        public static void Verify()
        {
            Directory.CreateDirectory("PreviewExports/ShopLayerFix");
            var notes = new List<string>();
            void Require(bool condition, string caption)
            {
                if (!condition) throw new InvalidOperationException(caption);
                notes.Add("PASS " + caption);
            }
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            var clothObject = new GameObject("CarpetGeometryCheck");
            var clothTexture = new Texture2D(256, 128);
            Sprite clothSprite = Sprite.Create(clothTexture, new Rect(0, 0, 256, 128), Vector2.one * 0.5f, 64);
            try
            {
                SpriteRenderer cloth = clothObject.AddComponent<SpriteRenderer>(); cloth.sprite = clothSprite;
                MarketCarpetRoll roll = clothObject.AddComponent<MarketCarpetRoll>(); roll.Configure(cloth);
                roll.Display(0.25f, 1);
                Require(Mathf.Abs(roll.RollFront + 1) < 0.001f, "Roll begins at left quarter");
                Require(roll.RollHeight > 0 && !cloth.enabled, "Physical cylinder replaces flat sprite while rolling");
                Require(clothObject.transform.Find("MovingCarpetRoll").GetComponent<MeshFilter>().sharedMesh.vertices.Any(vertex => vertex.z < -0.01f), "Roll is raised geometry, not a reveal mask");
                float firstHeight = roll.RollHeight;
                roll.Display(0.75f, 1);
                Require(Mathf.Abs(roll.RollFront - 1) < 0.001f && roll.RollHeight < firstHeight, "Roll travels right and shrinks as cloth unwinds");
                roll.Display(0.25f, 1);
                Require(Mathf.Abs(roll.RollFront + 1) < 0.001f, "Departure rolls back toward left");
                roll.Display(1, 1);
                Require(cloth.enabled && !clothObject.transform.Find("MovingCarpetRoll").GetComponent<MeshRenderer>().enabled, "Finished cloth restores undistorted original sprite");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(clothObject);
                UnityEngine.Object.DestroyImmediate(clothSprite);
                UnityEngine.Object.DestroyImmediate(clothTexture);
            }
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 171077);
                session.State.Phase = SessionPhase.Shop;
                session.State.Gold = 100000000;
                foreach (int purchased in new[] { 0, 1, 2, 3 })
                {
                    session.State.NewRecruitsThisWave = purchased;
                    int recruits = 0;
                    for (int roll = 0; roll < 200; roll++)
                    {
                        Require(session.Reroll(), "Refresh " + purchased + "/" + roll);
                        int count = session.State.Offers.Count(offer => offer.Kind == OfferKind.Companion);
                        Require(count <= 1, "At most one ally slot " + purchased + "/" + roll);
                        if (purchased < 2) Require(count == 1, "Guaranteed ally before two purchases " + purchased + "/" + roll);
                        if (purchased == 3) Require(count == 0, "Quota exhausted " + roll);
                        recruits += count;
                    }
                    if (purchased == 2) Require(recruits > 5 && recruits < 55, "Third-body offer frequency is strongly reduced: " + recruits + "/200");
                }
            }
            using (var session = new RunSession(catalog))
            {
                session.Start("LoneJelly", 171077);
                session.State.Phase = SessionPhase.Shop; session.State.Gold = 1000000;
                for (int roll = 0; roll < 20; roll++)
                {
                    Require(session.Reroll(), "Solo refresh " + roll);
                    Require(session.State.Offers.All(offer => offer.Kind == OfferKind.Item), "Solo keeps item-only shop " + roll);
                }
            }
            File.WriteAllLines("PreviewExports/ShopLayerFix/Recruitment_Checks.txt", notes);
            Debug.Log("SHOP_LAYER_CHECKS_PASSED " + notes.Count);
            K17Builder.Verify();
        }
    }
}
