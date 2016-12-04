using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace LeeSin_EloClimber
{
    class rUtility
    {
        public Vector3 pos;
        public int hit;

        // Default constructor:
        public rUtility()
        {
        }

        // Constructor:
        public rUtility(Vector3 pos, int hit)
        {
            this.pos = pos;
            this.hit = hit;
        }
    }

    internal class Combo
    {
        private static Obj_AI_Hero Target;
        private static float jumpToPos = Environment.TickCount;

        internal static void Load()
        {
            Game.OnUpdate += Update;
            Drawing.OnDraw += OnDraw;
        }

        private static void OnDraw(EventArgs args)
        {
            if (Target != null && Target.IsValidTarget(LeeSin.Q.Range + LeeSin.W.Range) && MenuManager.myMenu.Item("combo.target").GetValue<Boolean>())
            {
                Render.Circle.DrawCircle(Target.Position, 100, Color.Aquamarine);
            }
        }

        private static void Update(EventArgs args)
        {
            Target = TargetSelector.GetTarget(LeeSin.Q.Range + LeeSin.W.Range, TargetSelector.DamageType.Physical);

            if (LeeSin.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                launchCombo(Target);
            }
        }

        private static void launchCombo(Obj_AI_Hero unit)
        {
            if (unit != null & unit.IsValidTarget(LeeSin.Q.Range + LeeSin.W.Range))
            {
                if (MenuManager.myMenu.Item("combo.useQ").GetValue<Boolean>())
                    CastQ(unit);
                if (MenuManager.myMenu.Item("combo.useE").GetValue<Boolean>())
                    CastE(unit);
                if (MenuManager.myMenu.Item("combo.useR").GetValue<Boolean>())
                    CastR(unit);
            }
        }

        private static void CastQ(Obj_AI_Hero target)
        {
            if (LeeSin.Q.IsReady() && !LeeSin.IsSecondCast(LeeSin.Q))
            {
                PredictionOutput qPred = LeeSin.Q.GetPrediction(target);
                if ((int)qPred.Hitchance >= MenuManager.myMenu.Item("combo.qHitChance").GetValue<Slider>().Value)
                    LeeSin.Q.Cast(qPred.CastPosition);
            }
            else if (LeeSin.Q.IsReady() && LeeSin.IsSecondCast(LeeSin.Q) && target.HasBuff("BlindMonkQOne"))
            {
                if (LeeSin.myHero.Position.Distance(target.Position) > 700 || Environment.TickCount - LeeSin.lastQ > 2800 || LeeSin.GetDamage_Q2(target,0) > target.Health)
                LeeSin.Q.Cast();
            }
        }

        private static void CastE(Obj_AI_Hero target)
        {
            if (LeeSin.E.IsReady() && !LeeSin.IsSecondCast(LeeSin.E) && LeeSin.myHero.Position.Distance(target.Position) < LeeSin.E.Range - 20)
                LeeSin.E.Cast();
            else if (LeeSin.E.IsReady() && LeeSin.IsSecondCast(LeeSin.E) && (LeeSin.myHero.Position.Distance(target.Position) > LeeSin.E.Range || LeeSin.PassiveStack == 0))
                LeeSin.E.Cast();
        }

        private static void CastR(Obj_AI_Hero target)
        {
            if (LeeSin.R.IsReady())
            {
                if (MenuManager.myMenu.Item("combo.rLogic").GetValue<Boolean>())
                {
                    // Find best pos to kick
                    Spell wardSpell = LeeSin.FindWard();
                    rUtility value = Find_R_BestPos(target);
                    if (value.hit > 1 && value.pos.Distance(LeeSin.myHero.Position) < 600)
                    {
                        if (target.Position.Distance(LeeSin.myHero.Position) < LeeSin.R.Range && Environment.TickCount - jumpToPos < 2000)
                        {
                            LeeSin.R.Cast(target);
                        }
                        else if (LeeSin.W.IsReady() && !LeeSin.IsSecondCast(LeeSin.W) && wardSpell != null)
                        {
                            LeeSin.WardJump_Position(value.pos);
                            jumpToPos = Environment.TickCount;
                        }
                    }
                }
                // Kill
                if (LeeSin.myHero.Position.Distance(target.Position) < LeeSin.R.Range)
                {
                    if (target.Health < LeeSin.GetDamage_R(target))
                        LeeSin.R.Cast(target);

                    if (LeeSin.Q.IsReady() && !LeeSin.IsSecondCast(LeeSin.Q))
                    {
                        if(LeeSin.GetDamage_Q(target, LeeSin.GetDamage_R(target)) > target.Health)
                            LeeSin.R.Cast(target);
                    }

                    if (LeeSin.Q.IsReady() && LeeSin.IsSecondCast(LeeSin.Q) && target.HasBuff("BlindMonkQOne"))
                    {
                        if (LeeSin.GetDamage_Q2(target, LeeSin.GetDamage_R(target)) > target.Health)
                            LeeSin.R.Cast(target);
                    }
                }   

            }         
        }

        private static rUtility Find_R_BestPos(Obj_AI_Base target)
        {
            rUtility result = new rUtility(new Vector3(), 0);
            Vector3 bestPos = new Vector3();
            int unitHit = 1;
            int maxHit = 1;

            foreach (var unit in HeroManager.Enemies)
            {              
                if (unit.NetworkId != target.NetworkId && target.Position.Distance(unit.Position) < 1100 && unit.IsValidTarget())
                {
                    Vector3 startPos = target.Position;
                    Vector3 endPos = unit.Position;
                    endPos = startPos + (endPos - startPos).Normalized() * 900;
                    var zone = new Geometry.Polygon.Rectangle(startPos, endPos, target.BoundingRadius-5);

                    foreach(var unit2 in HeroManager.Enemies)
                    {
                        if (unit2.NetworkId != target.NetworkId && unit2.NetworkId != unit.NetworkId && unit2.IsValidTarget())
                        {
                            if(zone.IsInside(unit2))
                            {
                                unitHit++;
                                if(unitHit > maxHit)
                                {
                                    maxHit = unitHit;
                                    bestPos = target.Position + (target.Position - unit2.Position).Normalized() * 250;
                                    result.pos = bestPos;
                                    result.hit = maxHit;
                                }
                            }
                        }
                    }
                    if(maxHit == 1)
                    {
                        maxHit = 2;
                        bestPos = target.Position + (target.Position - unit.Position).Normalized() * 250;
                        result.pos = bestPos;
                        result.hit = maxHit;
                    }
                }
            }

            return result;
        }
    }
}
