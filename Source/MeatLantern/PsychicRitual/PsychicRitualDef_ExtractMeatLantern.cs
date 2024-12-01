using LCAnomalyCore.Defs;
using System.Collections.Generic;
using Verse.AI.Group;

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
    }
}