using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
//using Debug = LeagueSharp.SDK.Core.Utils.Logging;
// Logging.Write()(LeagueSharp.SDK.LogLevel.Info, "this is a test {0}", Variable.ToString());
using LeagueSharp.Common;
using SharpDX; 
using Collision = LeagueSharp.Common.Collision;

namespace Caitlyn_Master_Headshot
{
    class Program
    {
        // Variable
        public static Menu myMenu;
        public static Obj_AI_Hero myHero;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static float lastTrap;
        private static Dictionary<int, GameObject> trapDict = new Dictionary<int, GameObject>();

        // Loader
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;     
        }

        private static void OnLoad(EventArgs args)
        {       
            initVariable();

            if (myHero.ChampionName != "Caitlyn")
                return;

            Game.PrintChat("Loaded");
            initMenu();

            GameObject.OnCreate += Obj_AI_Base_OnCreate;
            GameObject.OnDelete += Obj_AI_Base_OnDelete;
            Game.OnUpdate += Update;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
        }

        private static void initVariable()
        {
            Q = new Spell(SpellSlot.Q, true);
            W = new Spell(SpellSlot.W, true);
            E = new Spell(SpellSlot.E, true);
            R = new Spell(SpellSlot.R, true);
            lastTrap = Game.Time;
            myHero = ObjectManager.Player;
        }

        private static void initMenu()
        {
            myMenu = new Menu("Caitlyn - Master Headshot", "CaitlynMasterHeadshot", true);
            myMenu.AddSubMenu(new Menu("Orbwalk Settings", "InitOrbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(myMenu.SubMenu("InitOrbwalker"));

            myMenu.AddSubMenu(new Menu("Combo Settings", "Combo"));;
            myMenu.SubMenu("Combo").AddItem(new MenuItem("useQ", "Use (Q)").SetValue(true));
            myMenu.SubMenu("Combo").AddItem(new MenuItem("qHitChance", "(Q) Hit Chance").SetValue(new Slider(4, 3, 6)));
            myMenu.SubMenu("Combo").AddItem(new MenuItem("infoW", ""));
            myMenu.SubMenu("Combo").AddItem(new MenuItem("useW", "Use (W)").SetValue(true));
            myMenu.SubMenu("Combo").AddItem(new MenuItem("wDelay", "Delay Between Each Trap (ms)").SetValue(new Slider(1500, 0, 3000)));
            myMenu.SubMenu("Combo").AddItem(new MenuItem("wHitChance", "(W) Hit Chance").SetValue(new Slider(5, 3, 6)));
            myMenu.SubMenu("Combo").AddItem(new MenuItem("infoE", ""));
            myMenu.SubMenu("Combo").AddItem(new MenuItem("useE", "Use (E)").SetValue(true));
            myMenu.SubMenu("Combo").AddItem(new MenuItem("eHitChance", "(E) Hit Chance").SetValue(new Slider(5, 3, 6)));

            myMenu.AddSubMenu(new Menu("Harass Settings", "Harass")); ;
            myMenu.SubMenu("Harass").AddItem(new MenuItem("useQ.Harass", "Use (Q)").SetValue(true));
            myMenu.SubMenu("Harass").AddItem(new MenuItem("qHitChance.Harass", "(Q) Hit Chance").SetValue(new Slider(5, 3, 6)));
            myMenu.SubMenu("Harass").AddItem(new MenuItem("qMana", "Mana Manger").SetValue(new Slider(80, 0, 100)));

            myMenu.AddSubMenu(new Menu("Ultimate Settings", "R"));
                myMenu.SubMenu("R").AddItem(new MenuItem("useR", "Auto Use (R)").SetValue(true));
                myMenu.SubMenu("R").AddItem(new MenuItem("rCombo", "Use while Combo").SetValue(true));

            myMenu.AddToMainMenu();
        }

        // Callback
        private static void Update(EventArgs args)
        {
            if (myHero == null || myHero.IsDead)
                return;

            if (myMenu.Item("useR").GetValue<Boolean>())
                autoCastR();

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
            }            
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs Args)
        {
            if (Q.IsReady() && sender.IsMe && (int)Args.Slot == 49)
            {
                var Target = TargetSelector.GetTarget(1200, TargetSelector.DamageType.Physical);
                if (ValidTarget(Target) && Target.NetworkId == Args.Target.NetworkId)
                {
                    PredictionOutput qPred = Q.GetPrediction(Target);
                    if ((int)qPred.Hitchance > myMenu.Item("qHitChance").GetValue<Slider>().Value)
                        Q.Cast(qPred.CastPosition);
                }
            }
        }

        private static void Obj_AI_Base_OnCreate(GameObject obj, EventArgs args)
        {
            if (obj.IsAlly && obj.Name == "Cupcake Trap")
            {
                trapDict.Add(obj.NetworkId, obj);
            }
               
        }

        private static void Obj_AI_Base_OnDelete(GameObject obj, EventArgs args)
        {
            if (obj.IsAlly && obj.Name == "Cupcake Trap")
            {
                if (trapDict.ContainsKey(obj.NetworkId))
                    trapDict.Remove(obj.NetworkId);
            }
        }

        // Utility
        private static bool ValidTarget(Obj_AI_Hero unit)
        {
            return !(unit == null) && unit.IsValid && unit.IsTargetable && !unit.IsInvulnerable;
        }

        private static int IsTrapNear(Vector3 Position, int Range)
        {
            int trapNear = 0;
            foreach (var trap in trapDict)
            {
                if (Position.Distance(trap.Value.Position) < Range)
                    trapNear++;
            }           

            return trapNear;
        }

        private static int CountEnemyNear(Vector3 From)
        {
            int enemyNear = 0;
            foreach(var unit in HeroManager.Enemies)
            {
                if (From.Distance(unit.Position) < 500)
                    enemyNear++;
            }
            return enemyNear;
        }

        private static Obj_AI_Hero GetTarget()
        {
            Obj_AI_Hero Target = TargetSelector.GetTarget(1200, TargetSelector.DamageType.Physical);
            return Target;
        }

        // Combo
        private static void Combo()
        {
            var Target = GetTarget();

            if (!ValidTarget(Target))
                return;

            wCastCombo(Target);
            eCastCombo(Target);      
        }

        private static void wCastCombo(Obj_AI_Base Target)
        {
            if (myMenu.Item("useW").GetValue<Boolean>() && W.IsReady() && Game.Time - lastTrap > (myMenu.Item("wDelay").GetValue<Slider>().Value)/1000)
            {
                PredictionOutput wPred = W.GetPrediction(Target);
                if (IsTrapNear(wPred.CastPosition, 100) == 0 && (int)wPred.Hitchance >= myMenu.Item("wHitChance").GetValue<Slider>().Value)
                {
                    W.Cast(wPred.CastPosition);
                    lastTrap = Game.Time;
                }
            }

        }

        private static void eCastCombo(Obj_AI_Base Target)
        {
            if (myMenu.Item("useE").GetValue<Boolean>() && E.IsReady())
            {
                PredictionOutput ePred = E.GetPrediction(Target);
                               
                if ((int)ePred.Hitchance >= myMenu.Item("eHitChance").GetValue<Slider>().Value)
                    E.Cast(ePred.CastPosition);
            }
        }

        // Harass
        private static void Harass()
        {
            var Target = GetTarget();

            if (!ValidTarget(Target))
                return;

            qHarass(Target);
        }

        private static void qHarass(Obj_AI_Hero unit)
        {
            if (Q.IsReady() && myMenu.Item("useQ.Harass").GetValue<Boolean>())
            {
                PredictionOutput qPred = Q.GetPrediction(unit);
                if ((int)qPred.Hitchance >= myMenu.Item("qHitChance.Harass").GetValue<Slider>().Value)
                    Q.Cast(qPred.CastPosition);
            }
        }


        // Misc
        private static void autoCastR()
        {
            if (!R.IsReady() || (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && !myMenu.Item("rCombo").GetValue<Boolean>()))
                return;

            foreach (var unit in HeroManager.Enemies)
            {
                if (ValidTarget(unit) && myHero.Distance(unit) > (myHero.AttackRange+150) && R.GetDamage(unit, 0) > unit.Health && CountEnemyNear(myHero.Position) == 0)
                {
                    PredictionInput predInput = new PredictionInput { From = myHero.Position, Radius = 800, Range = 3000 };
                    predInput.CollisionObjects[0] = CollisionableObjects.YasuoWall;
                    predInput.CollisionObjects[1] = CollisionableObjects.Heroes;
               
                    IEnumerable<Obj_AI_Base> rCol = Collision.GetCollision(new List<Vector3> { unit.Position }, predInput).ToArray();
                    IEnumerable<Obj_AI_Base> rObjCol = rCol as Obj_AI_Base[] ?? rCol.ToArray();

                    if (rObjCol.Count() == 0)
                        R.Cast(unit);
                }
            }
        }
    }
}
