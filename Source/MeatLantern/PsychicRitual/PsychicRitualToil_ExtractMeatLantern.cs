using MeatLantern.Things;
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
                foreach (var thing in pawn.MapHeld.listerThings.AllThings)
                {
                    //Log.Warning(thing.def.defName);
                    //检查地图上是否存在肉食提灯蛋
                    if (thing.def == Def.ThingDefOf.MeatLanternEgg)
                    {
                        Failed();
                        return;
                    }

                    //检查地图收容平台内是否存在肉食提灯
                    if (thing is LCAnomalyCore.Building.Building_HoldingPlatform platform && LCAnomalyLibrary.Util.Types.CheckIfLCEntity(platform.HeldPawn))
                    {
                        Failed();
                        return;
                    }

                    //检查地图内是否有野生的肉食提灯
                    if (thing is LC_MeatLanternPawn)
                    {
                        Failed();
                        return;
                    }
                }

                ApplyOutcome(psychicRitual, pawn);
            }
        }

        private void ApplyOutcome(Verse.AI.Group.PsychicRitual psychicRitual, Pawn invoker)
        {
            IntVec3 cell = psychicRitual.assignments.Target.Cell;

            Thing item = GenSpawn.Spawn(Def.ThingDefOf.MeatLanternEgg, cell, psychicRitual.Map);

            if (item == null)
            {
                Log.Warning("仪式：肉食提灯的蛋不存在");
                return;
            }

            psychicRitual.Map.effecterMaintainer.AddEffecterToMaintain(EffecterDefOf.Skip_ExitNoDelay.Spawn(cell, psychicRitual.Map), cell, 60);
            RimWorld.SoundDefOf.Psycast_Skip_Exit.PlayOneShot(new TargetInfo(cell, psychicRitual.Map));

            TaggedString text = "ExtractMeatLanternCompleteText".Translate();
            Find.LetterStack.ReceiveLetter("ExtractMeatLanternCompleteLabel".Translate(), text, LetterDefOf.NeutralEvent);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref invokerRole, "invokerRole");
        }

        protected void Failed()
        {
            Log.Warning("检测到地图内有重复的肉食提灯对象/蛋，仪式取消");
        }
    }
}