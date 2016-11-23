using System;
using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;

namespace PrivatePoppy
{
    class Program
    {
        public static Menu Param;
        public static Obj_AI_Hero Player => ObjectManager.Player;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        private static void Main(string[] args)
        {
            if (Player.ChampionName != "Poppy")
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, true);
            W = new Spell(SpellSlot.W, true);
            E = new Spell(SpellSlot.E, true);
            R = new Spell(SpellSlot.R, true);

            Param = new Menu("PrivatePoppy", "PrivatePoppy", true);
            Param.AddSubMenu(new Menu("Orbwalker Settings", "InitOrbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(Param.SubMenu("InitOrbwalker"));
            Param.AddToMainMenu();

            Game.PrintChat("<font color='#F5D76E'>Loaded PrivatePoppy by kyps.</font>");
            Game.OnUpdate += Game_OnTick;
            Drawing.OnDraw += Game_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += Game_EnemyGap;
        }

        private static void Game_OnDraw(EventArgs args)
        {

            Drawing.DrawText(Drawing.Width - (Drawing.Width / 3), 10, Color.White, "" + DateTime.Now);

            if (Player.IsDead)
            {
                return;
            }
            var Target = GetTarget(420);
            if (Target == null)
            {
                return;
            }
            var from = ObjectManager.Player.Position;
            var to = Drawing.WorldToScreen(Target.Position);
            var finish = Drawing.WorldToScreen(Target.Position + (Target.Position - from).Normalized() * 300);

            Drawing.DrawLine(to[0], to[1], finish[0], finish[1], 2, Color.White);
        }

        private static void Game_OnTick(EventArgs args)
        {
            try
            {
                if (Player.IsDead)
                {
                    return;
                }
                switch (Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        Combo();
                        break;
                    case Orbwalking.OrbwalkingMode.Mixed:
                        Harass();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: '{0}'", e);
            }
        }

        private static void Combo()
        {
            var Target = GetTarget(430);
            if (Target == null)
            {
                return;
            }
            if (E.IsReady() && CanStun())
            {
                E.Cast(GetTarget(425));
            }
            if (Q.IsReady())
            {
                Q.Cast(Target);
            }
        }

        private static void Harass()
        {
            var Target = GetTarget(430);
            if (Target == null)
            {
                return;
            }
            if (Q.IsReady())
            {
                Q.Cast(Target);
            }
        }

        private static void Game_EnemyGap(ActiveGapcloser gapcloser)
        {
            if (W.IsReady())
            {
                W.Cast();
            }
        }

        private static void OnInterruptableTarget(Obj_AI_Hero Unit, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (Player.Distance(Unit) > 425)
            {
                return;
            }
            if (E.IsReady())
            {
                E.Cast(Unit);
            }
            else if (R.IsReady())
            {
                R.Cast(Unit);
                R.Cast(Unit);
            }
        }

        private static Obj_AI_Hero GetTarget(int Range)
        {
            Obj_AI_Hero Target = TargetSelector.GetTarget(Range, TargetSelector.DamageType.Physical);
            if (Target == null || Target.IsValid == false || Target.IsTargetable == false || Target.IsInvulnerable == true)
            {
                Target = null;
            }
            return Target;
        }

        private static bool CanStun()
        {
            var Unit = GetTarget(425);
            if (Unit == null)
            {
                return false;
            }
            var UPos = E.GetPrediction(Unit).UnitPosition;
            var FP = Unit.Position + (Unit.Position - ObjectManager.Player.Position).Normalized() * 300;
            if (FP.IsWall())
            {
                return true;
            }
            return false;
        }
    }
}