using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace RandomStats
{
    public class RandomStats : Mod
    {
        // Instance of this mod
        public static RandomStats Instance;

        // UI and UserInterface
        public static UserInterface RandomStatsUserInterface;
        public static RandomStatsRerollUI RerollUI;

        public override void Load()
        {
            base.Load();
            //On.Terraria.Main.DamageVar += (orig, damage, luck) => (int)Math.Round(damage);

            Instance = this;

            RerollUI = new RandomStatsRerollUI();
            RandomStatsUserInterface = new UserInterface();
        }

        public override void Unload()
        {
            RerollUI = null;
            RandomStatsUserInterface = null;
        }
    }

    // Thank you chatGPT for this one - Soycake
    public class RandomStatsSystem : ModSystem
    {
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            // Show the UI when the custom state is active
            if (RandomStats.RandomStatsUserInterface?.CurrentState is RandomStatsRerollUI)
            {
                layers.Insert(layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory")),
                    new LegacyGameInterfaceLayer("RandomStatsGoblinTest: Custom UI",
                    delegate
                {
                    RandomStats.RandomStatsUserInterface.Draw(Main.spriteBatch, new GameTime());
                    return true;
                },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
}