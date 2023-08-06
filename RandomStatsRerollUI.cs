using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Text;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria;
using Terraria.UI;
using Terraria.UI.Chat;

namespace RandomStats
{
    public class RandomStatsRerollUI : UIState
    {
        private VanillaItemSlotWrapper _vanillaItemSlot;

        public override void OnInitialize()
        {
            _vanillaItemSlot = new VanillaItemSlotWrapper(ItemSlot.Context.BankItem, 0.85f)
            {
                Left = { Pixels = 50 },
                Top = { Pixels = 370 },

                // TODO - Change valid item here later for "Random Stats" valid items
                ValidItemFunc = item => item.IsAir || !item.IsAir// && item.Prefix(-3)
            };

            // Here we limit the items that can be placed in the slot. We are fine with placing an empty item in or a non-empty item that can be prefixed. Calling Prefix(-3) is the way to know if the item in question can take a prefix or not.
            Append(_vanillaItemSlot);
        }

        // OnDeactivate is called when the UserInterface switches to a different state. In this mod, we switch between no state (null) and this state (ExamplePersonUI).
        // Using OnDeactivate is useful for clearing out Item slots and returning them to the player, as we do here.
        public override void OnDeactivate()
        {
            if (_vanillaItemSlot.Item.IsAir)
            {
                return;
            }

            // QuickSpawnClonedItem will preserve mod data of the item. QuickSpawnItem will just spawn a fresh version of the item, losing the prefix.
            Main.LocalPlayer.QuickSpawnItem(null, _vanillaItemSlot.Item, _vanillaItemSlot.Item.stack);

            // Now that we've spawned the item back onto the player, we reset the item by turning it into air.
            _vanillaItemSlot.Item.TurnToAir();

            // Note that in ExamplePerson we call .SetState(new UI.ExamplePersonUI());, thereby creating a new instance of this UIState each time. 
            // You could go with a different design, keeping around the same UIState instance if you wanted. This would preserve the UIState between opening and closing. Up to you.
        }

        // Update is called on a UIState while it is the active state of the UserInterface.
        // We use Update to handle automatically closing our UI when the player is no longer talking to our Example Person NPC.
        public override void Update(GameTime gameTime)
        {
            // Don't delete this or the UIElements attached to this UIState will cease to function.
            base.Update(gameTime);

            // talkNPC is the index of the NPC the player is currently talking to. By checking talkNPC, we can tell when the player switches to another NPC or closes the NPC chat dialog.
            if (Main.LocalPlayer.talkNPC == -1 || Main.npc[Main.LocalPlayer.talkNPC].type != NPCID.GoblinTinkerer)
            {
                // When that happens, we can set the state of our UserInterface to null, thereby closing this UIState. This will trigger OnDeactivate above.
                RandomStats.RandomStatsUserInterface.SetState(null);
            }
        }

        private bool tickPlayed;

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            // This will hide the crafting menu similar to the reforge menu. For best results this UI is placed before "Vanilla: Inventory" to prevent 1 frame of the craft menu showing.
            Main.hidePlayerCraftingMenu = true;

            // Here we have a lot of code. This code is mainly adapted from the vanilla code for the reforge option.
            // This code draws "Place an item here" when no item is in the slot and draws the reforge cost and a reforge button when an item is in the slot.
            // This code could possibly be better as different UIElements that are added and removed, but that's not the main point of this example.
            // If you are making a UI, add UIElements in OnInitialize that act on your ItemSlot or other inputs rather than the non-UIElement approach you see below.

            const int SlotX = 50;
            const int SlotY = 370;

            if (_vanillaItemSlot.Item.IsAir)
            {
                const string Message = "Place an item here to reroll base stats";

                ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value, Message, new Vector2(SlotX + 50, SlotY), new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor), 0f, Vector2.Zero, Vector2.One, -1f, 2f);
                return;
            }

            int awesomePrice = Item.buyPrice(0, 1, 0, 0);
            string costText = Language.GetTextValue("LegacyInterface.46") + ": ";
            int[] coins = Utils.CoinsSplit(awesomePrice);
            var coinsText = new StringBuilder();

            for (int i = 0; i < 4; i++)
            {
                coinsText.Append($"[c/{Colors.AlphaDarken(Colors.CoinPlatinum).Hex3()}:{coins[3 - i]} {Language.GetTextValue($"LegacyInterface.{15 + i}")}]");
            }

            ItemSlot.DrawSavings(Main.spriteBatch, SlotX + 130, Main.instance.invBottom, true);
            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value, costText, new Vector2(SlotX + 50, SlotY), new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor), 0f, Vector2.Zero, Vector2.One, -1f, 2f);
            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value, coinsText.ToString(), new Vector2(SlotX + 50 + FontAssets.MouseText.Value.MeasureString(costText).X, (float)SlotY), Color.White, 0f, Vector2.Zero, Vector2.One, -1f, 2f);

            int reforgeX = SlotX + 70;
            int reforgeY = SlotY + 40;
            bool hoveringOverReforgeButton = Main.mouseX > reforgeX - 15 && Main.mouseX < reforgeX + 15 && Main.mouseY > reforgeY - 15 && Main.mouseY < reforgeY + 15 && !PlayerInput.IgnoreMouseInterface;
            Texture2D reforgeTexture = TextureAssets.Reforge[hoveringOverReforgeButton ? 1 : 0].Value;

            Main.spriteBatch.Draw(reforgeTexture, new Vector2(reforgeX, reforgeY), null, Color.White, 0f, reforgeTexture.Size() / 2f, 0.8f, SpriteEffects.None, 0f);

            if (!hoveringOverReforgeButton)
            {
                tickPlayed = false;
                return;
            }

            Main.hoverItemName = Language.GetTextValue("LegacyInterface.19");

            if (!tickPlayed)
            {
                //SoundEngine.PlaySound(SoundID.MenuTick, -1, -1, 1, 1f, 0f);
                SoundEngine.PlaySound(SoundID.MenuTick, null, null);
            }

            tickPlayed = true;
            Main.LocalPlayer.mouseInterface = true;

            if (!Main.mouseLeftRelease || !Main.mouseLeft || !Main.LocalPlayer.CanAfford(awesomePrice, -1))
            {
                return;
            }

            Main.LocalPlayer.BuyItem(awesomePrice, -1);

            bool favorited = _vanillaItemSlot.Item.favorited;
            int stack = _vanillaItemSlot.Item.stack;

            Item reforgeItem = new Item();

            reforgeItem.netDefaults(_vanillaItemSlot.Item.netID);

            //reforgeItem = reforgeItem.CloneWithModdedDataFrom(_vanillaItemSlot.Item);
            reforgeItem = _vanillaItemSlot.Item.Clone();

            // TODO - Starting here is the part where stuff gets changed
            //      Currently it's only designed for weapon with no regards for armor

            // There's a bug where if you leave the item in the slot and close the game, the item will disappear (tested for normal goblin reforge slot and item properly drops back to player on relog)

            GlobalInstancedItems itemInst = reforgeItem.GetGlobalItem<GlobalInstancedItems>();
            reforgeItem.damage = reforgeItem.OriginalDamage;
            // Reset randomStat to 0 before calling setup so it actually rerolls
            itemInst.randomStat = 0;
            itemInst.SetupRandomDamage(reforgeItem);

            #region ChatFeature
            // Chat Feature for if player low/highrolls (just added it in for fun)

            // TODO - print is only to client, I assume it'd be more fun for it to print to server if playing MP

            // TODO - I assume chat things should configurable
            //      bool for whether chat feature is enabled/disabled
            //      the bottom x% for lowroll chat
            //      the top x% for highroll chat
            //      the minimum base damage weapon needs to print
            //      the minimum base armor needs to print (once that's implemented)

            int rngMinValue = ModContent.GetInstance<RandomStatsConfig>().MinRandomVariance;
            int rngMaxValue = ModContent.GetInstance<RandomStatsConfig>().MaxRandomVariance;

            // Assuming rngMinValue and rngMaxValue are in percentage format
            float lowerBound = rngMinValue / 100f;
            float upperBound = rngMaxValue / 100f;

            // Calculate the potential min/max damage
            int minDamage = (int)(reforgeItem.OriginalDamage * lowerBound);
            int maxDamage = (int)(reforgeItem.OriginalDamage * upperBound);
            int actualDamage = (int)(reforgeItem.damage * itemInst.randomStat);

            // Calculate differences for low/highroll (bottom 5% and top 5%)
            float lowRollBoundary = lowerBound + 0.05f * (upperBound - lowerBound);
            float highRollBoundary = upperBound - 0.05f * (upperBound - lowerBound);

            string tooltipFormatString = $"{actualDamage} [{minDamage} - {maxDamage}]";

            // Only says chat for weapon above certain attack (to prevent spam chat)
            if (reforgeItem.damage >= 15)
            {
                // Min roll chat
                if (itemInst.randomStat <= lowerBound)
                {
                    Main.NewText($"{Main.LocalPlayer.name} hit rock bottom... [i/s1:{reforgeItem.type}] {tooltipFormatString}", Color.White);
                }
                // Max roll chat
                else if (itemInst.randomStat == upperBound) // Since max roll is distinct, we don't add tolerance here
                {
                    Main.NewText($"{Main.LocalPlayer.name} hit the jackpot! [i/s1:{reforgeItem.type}] {tooltipFormatString}", Color.White);
                }
                // Low roll chat
                else if (itemInst.randomStat > lowerBound && itemInst.randomStat <= lowRollBoundary)
                {
                    Main.NewText($"{Main.LocalPlayer.name} just lowrolled... [i/s1:{reforgeItem.type}] {tooltipFormatString}", Color.White);
                }
                // High roll chat
                else if (itemInst.randomStat >= highRollBoundary && itemInst.randomStat < upperBound)
                {
                    Main.NewText($"{Main.LocalPlayer.name} just highrolled! [i/s1:{reforgeItem.type}] {tooltipFormatString}", Color.White);
                }
            }
            #endregion

            _vanillaItemSlot.Item = reforgeItem.Clone();
            _vanillaItemSlot.Item.position.X = Main.LocalPlayer.position.X + (float)(Main.LocalPlayer.width / 2) - (float)(_vanillaItemSlot.Item.width / 2);
            _vanillaItemSlot.Item.position.Y = Main.LocalPlayer.position.Y + (float)(Main.LocalPlayer.height / 2) - (float)(_vanillaItemSlot.Item.height / 2);
            _vanillaItemSlot.Item.favorited = favorited;
            _vanillaItemSlot.Item.stack = stack;

            ItemLoader.PostReforge(_vanillaItemSlot.Item);
            PopupText.NewText(PopupTextContext.RegularItemPickup, _vanillaItemSlot.Item, _vanillaItemSlot.Item.stack, true, false);
            SoundEngine.PlaySound(SoundID.Item37, null, null);
        }
    }
}

