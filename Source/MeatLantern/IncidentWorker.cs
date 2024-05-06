using RimWorld;
using Verse;

namespace MeatLantern
{
    public class IncidentWorker_MeatLantern : IncidentWorker
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            IntVec3 result = parms.spawnCenter;
            if (!result.IsValid && !RCellFinder.TryFindRandomPawnEntryCell(out result, map, CellFinder.EdgeRoadChance_Hostile))
            {
                return false;
            }

            Rot4 rot = Rot4.FromAngleFlat((map.Center - result).AngleFlat);
            GenSpawn.Spawn(PawnGenerator.GeneratePawn(new PawnGenerationRequest(KindDefOf.MeatLanternEscaped, Faction.OfEntities, PawnGenerationContext.NonPlayer, map.Tile)), result, map, rot);
            return true;
        }
    }
}
