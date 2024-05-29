using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse.AI.Group;
using Verse;
using LCAnomalyLibrary.Defs;

namespace MeatLantern.PsychicRitual
{
    public class PsychicRitualDef_ExtractMeatLantern : ExtractRitualDef_InvocationCircle
    {

        public override List<PsychicRitualToil> CreateToils(Verse.AI.Group.PsychicRitual psychicRitual, PsychicRitualGraph parent)
        {
            List<PsychicRitualToil> list = base.CreateToils(psychicRitual, parent);
            list.Add(new PsychicRitualToil_ExtractMeatLantern(this.InvokerRole));
            return list;
        }

        public override TaggedString OutcomeDescription(FloatRange qualityRange, string qualityNumber, PsychicRitualRoleAssignments assignments)
        {
            string value = Mathf.FloorToInt(this.comaDurationDaysFromQualityCurve.Evaluate(qualityRange.min) * 60000f).ToStringTicksToDays("F1");
            return this.outcomeDescription.Formatted(value);
        }

        // Token: 0x04002C7E RID: 11390
        public SimpleCurve comaDurationDaysFromQualityCurve;
    }
}
