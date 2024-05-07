using LCAnomalyLibrary.Comp;
using RimWorld;
using Verse;

namespace MeatLantern
{
    public class CompSpawnsMeatLantern : LC_CompSpawns
    {
        public override void CompTickRare()
        {
            if (spawnTick > 0 && Find.TickManager.TicksGame > spawnTick && parent.MapHeld != null)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(KindDefOf.MeatLanternEscaped, Faction.OfEntities);
                GenSpawn.Spawn(pawn, parent.PositionHeld, parent.MapHeld);
                CompMeatLantern compMeatLantern = pawn.TryGetComp<CompMeatLantern>();
                compMeatLantern.Invisibility.BecomeInvisible(true);
                compMeatLantern.SetState(MeatLanternState.Wait);
                Find.LetterStack.ReceiveLetter("LetterLabelRevenantEmergence".Translate(), "LetterRevenantEmergence".Translate(), LetterDefOf.ThreatBig, pawn);
                parent.Destroy();
            }
        }
    }
}
