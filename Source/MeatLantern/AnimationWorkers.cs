using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MeatLantern
{
    public class AnimationWorker_MeatLanternHypnotise : AnimationWorker_Keyframes
    {
        private const float LensFlareScale = 0.7f;

        private const float LensFlareAnimPeriod = 30f;

        private const float Layer_AboveHead = 80f;

        private const float PxToUnitScale = 100f;

        private static readonly List<Vector3> EyePositionsPxNS = new List<Vector3>
    {
        new Vector3(-8f, 0f, -3f),
        new Vector3(12f, 0f, -5f),
        new Vector3(6f, 0f, 12f)
    };

        private static readonly List<Vector3> EyePositionsPxEW = new List<Vector3>
    {
        new Vector3(19f, 0f, 9f),
        new Vector3(18f, 0f, -7f)
    };

        private static readonly SimpleCurve LensFlareOpacity = new SimpleCurve
    {
        new CurvePoint(0f, 0.4f),
        new CurvePoint(0.5f, 0.8f),
        new CurvePoint(1f, 0.4f)
    };

        private Material lensFlareMat;

        private Material LensFlareMat
        {
            get
            {
                if (lensFlareMat == null)
                {
                    lensFlareMat = MaterialPool.MatFrom("Things/Pawn/MeatLantern/MeatLantern_LensFlare", ShaderDatabase.MoteGlow);
                }

                return lensFlareMat;
            }
        }

        public AnimationWorker_MeatLanternHypnotise(AnimationDef def, Pawn pawn, AnimationPart part, PawnRenderNode node)
            : base(def, pawn, part, node)
        {
        }

        public override void Draw(PawnDrawParms parms, Matrix4x4 matrix)
        {
            foreach (Vector3 item in (parms.facing == Rot4.North || parms.facing == Rot4.South) ? EyePositionsPxNS : EyePositionsPxEW)
            {
                Vector3 current = item;
                current.y = 1f;
                if (parms.facing == Rot4.North || parms.facing == Rot4.West)
                {
                    current.x *= -1f;
                }

                if (parms.facing == Rot4.North)
                {
                    current.y = -1f;
                }

                float x = (float)(Find.TickManager.TicksGame - node.tree.animationStartTick) % 30f / 30f;
                LensFlareMat.SetColor(ShaderPropertyIDs.Color, new Color(1f, 1f, 1f, LensFlareOpacity.Evaluate(x)));
                GenDraw.DrawMeshNowOrLater(MeshPool.GridPlane(Vector2.one * 0.7f), matrix * Matrix4x4.Translate(current / 100f), LensFlareMat, parms.DrawNow);
            }

            if (node.tree.pawn.mindState.enemyTarget is Pawn pawn)
            {
                Rot4 rotation = ((pawn.GetPosture() == PawnPosture.Standing) ? Rot4.North : pawn.Drawer.renderer.LayingFacing());
                Vector3 vector = pawn.Drawer.renderer.BaseHeadOffsetAt(rotation).RotatedBy(pawn.Drawer.renderer.BodyAngle(PawnRenderFlags.None));
                Vector3 loc = (pawn.DrawPos + vector).WithYOffset(PawnRenderUtility.AltitudeForLayer(80f));
                Quaternion quat = Quaternion.Euler(Vector3.up * pawn.Drawer.renderer.BodyAngle(PawnRenderFlags.None));
                GenDraw.DrawMeshNowOrLater(MeshPool.GridPlane(Vector2.one * 2f), loc, quat, LensFlareMat, parms.DrawNow);
            }
        }
    }

    public class AnimationWorker_MeatLanternSpasm : AnimationWorker
    {
        private IntRange SpasmIntervalShort = new IntRange(6, 18);

        private IntRange SpasmIntervalLong = new IntRange(120, 180);

        private int ShortSpasmLength = 6;

        private IntRange SpasmLength = new IntRange(30, 60);

        private float nextSpasm = -99999f;

        private float spasmStart = -99999f;

        private float spasmLength;

        private float startHeadRot = 90f;

        private Vector3 startHeadOffset = Vector3.zero;

        private float targetHeadRot = 90f;

        private Vector3 targetHeadOffset = Vector3.zero;

        private float AnimationProgress => ((float)Find.TickManager.TicksGame - spasmStart) / spasmLength;

        private float Rotation => Mathf.Lerp(startHeadRot, targetHeadRot, AnimationProgress);

        private Vector3 Offset => new Vector3(Mathf.Lerp(startHeadOffset.x, targetHeadOffset.x, AnimationProgress), 0f, Mathf.Lerp(startHeadOffset.z, targetHeadOffset.z, AnimationProgress));

        public AnimationWorker_MeatLanternSpasm(AnimationDef def, Pawn pawn, AnimationPart part, PawnRenderNode node)
            : base(def, pawn, part, node)
        {
        }

        public override float AngleAtTick(int tick, PawnDrawParms parms)
        {
            CheckAndDoSpasm();
            if (parms.facing == Rot4.East || parms.facing == Rot4.West)
            {
                return Rotation / 2f;
            }

            return Rotation;
        }

        public override Vector3 OffsetAtTick(int tick, PawnDrawParms parms)
        {
            CheckAndDoSpasm();
            if (parms.facing == Rot4.East || parms.facing == Rot4.West)
            {
                return Offset / 2f;
            }

            return Offset;
        }

        private void CheckAndDoSpasm()
        {
            if ((float)Find.TickManager.TicksGame > nextSpasm)
            {
                startHeadRot = Rotation;
                startHeadOffset = Offset;
                targetHeadRot = Rand.Range(-20, 20);
                targetHeadOffset = new Vector3(Rand.Range(-0.1f, 0.1f), 0f, Rand.Range(-0.05f, 0.05f));
                spasmStart = Find.TickManager.TicksGame;
                spasmLength = (Rand.Bool ? SpasmLength.RandomInRange : ShortSpasmLength);
                nextSpasm = Find.TickManager.TicksGame + (Rand.Bool ? SpasmIntervalShort.RandomInRange : SpasmIntervalLong.RandomInRange);
            }
        }
    }

}
