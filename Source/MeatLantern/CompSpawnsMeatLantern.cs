using RimWorld;
using Verse;

namespace MeatLantern
{
    public class CompSpawnsMeatLantern : ThingComp
    {
        public int spawnTick = -1;

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref spawnTick, "spawnTick", 0);
        }

        public override void CompTickRare()
        {
            if (spawnTick > 0 && Find.TickManager.TicksGame > spawnTick && parent.MapHeld != null)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(KindDef.MeatLanternEscaped, Faction.OfEntities);
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
