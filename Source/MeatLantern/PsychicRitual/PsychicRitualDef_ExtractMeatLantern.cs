using LCAnomalyLibrary.Defs;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace MeatLantern.PsychicRitual
{
    public class PsychicRitualDef_ExtractMeatLantern : ExtractRitualDef_InvocationCircle
    {
        //private SimpleCurve comaDurationDaysFromQualityCurve;

        public override List<PsychicRitualToil> CreateToils(Verse.AI.Group.PsychicRitual psychicRitual, PsychicRitualGraph parent)
        {
            List<PsychicRitualToil> list = base.CreateToils(psychicRitual, parent);
            list.Add(new PsychicRitualToil_ExtractMeatLantern(this.InvokerRole));
            return list;
        }

        //public override TaggedString OutcomeDescription(FloatRange qualityRange, string qualityNumber, PsychicRitualRoleAssignments assignments)
        //{
        //    string value = Mathf.FloorToInt(this.comaDurationDaysFromQualityCurve.Evaluate(qualityRange.min) * 60000f).ToStringTicksToDays("F1");
        //    return this.outcomeDescription.Formatted(value);
        //}
    }
}