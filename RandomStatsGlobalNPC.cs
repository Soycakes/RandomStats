using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RandomStats
{
    internal class RandomStatsGlobalNPC : GlobalNPC
    {
        public override bool AppliesToEntity(NPC npc, bool lateInstatiation)
        {
            return npc.type == NPCID.GoblinTinkerer;
        }

        public override void OnChatButtonClicked(NPC npc, bool firstButton)
        {
            if (!firstButton)// && Main.npcChatText == "Reforge")
            {
                RandomStats.RandomStatsUserInterface.SetState(RandomStats.RerollUI);
                RandomStats.Instance.ShowRerollUI();
            }
        }
    }
}
