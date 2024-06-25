using RimWorld;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace MeatLantern.PsychicRitual
{
    public class PsychicRitualToil_ExtractMeatLantern : PsychicRitualToil
    {

        private PsychicRitualRoleDef invokerRole;

        protected PsychicRitualToil_ExtractMeatLantern()
        { }

        public PsychicRitualToil_ExtractMeatLantern(PsychicRitualRoleDef invokerRole)
        {
            this.invokerRole = invokerRole;
        }

        public override void Start(Verse.AI.Group.PsychicRitual psychicRitual, PsychicRitualGraph parent)
        {
            Pawn pawn = psychicRitual.assignments.FirstAssignedPawn(invokerRole);
            if (pawn != null)
            {
                ApplyOutcome(psychicRitual, pawn);
            }
        }

        private void ApplyOutcome(Verse.AI.Group.PsychicRitual psychicRitual, Pawn invoker)
        {
            IntVec3 cell = psychicRitual.assignments.Target.Cell;

            Thing item = GenSpawn.Spawn(Def.ThingDefOf.DyingMeatLantern, cell, psychicRitual.Map);

            if (item == null)
            {
                Log.Warning("仪式：肉食提灯的蛋不存在");
                return;
            }

            psychicRitual.Map.effecterMaintainer.AddEffecterToMaintain(EffecterDefOf.Skip_ExitNoDelay.Spawn(cell, psychicRitual.Map), cell, 60);
            RimWorld.SoundDefOf.Psycast_Skip_Exit.PlayOneShot(new TargetInfo(cell, psychicRitual.Map));

            TaggedString text = "ExtractMeatLanternCompleteText".Translate(invoker.Named("INVOKER"), psychicRitual.def.Named("RITUAL"), item.Named("TARGET"), Faction.OfEntities);
            Find.LetterStack.ReceiveLetter("PsychicRitualCompleteLabel".Translate(psychicRitual.def.label), text, LetterDefOf.NeutralEvent, new LookTargets(item));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref invokerRole, "invokerRole");
        }
    }
}