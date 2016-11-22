using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
namespace MinionHPBar
{

    class Program
    {
        public static Obj_AI_Base Player = ObjectManager.Player;
        public static Menu Menu;

        private static void Main(string[] args)
        {
            Menu = new Menu("Rambe - Minion HP Bar", "RamberMinionHpBar", true);
                Menu.AddItem(new MenuItem("drawFrame", "Draw frame").SetValue(new Circle(true, Color.Black)));
                Menu.AddItem(new MenuItem("frameColor", "Change frame color when minion dead").SetValue(new Circle(true, Color.White)));
                Menu.AddItem(new MenuItem("drawBars", "Draw Bars").SetValue(new Circle(true, Color.Black)));
                Menu.AddItem(new MenuItem("minionRange", "Minion Range").SetValue(new Slider(1500, (int)Player.AttackRange, 2000)));
            Menu.AddToMainMenu();

            Game.PrintChat("<font color=\"#FF001E\">Minion HP Bar - </font><font color=\"#FF980F\"> Loaded</font>");

            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var minionList = MinionManager.GetMinions(Player.Position, 50000, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);
            foreach (var minion in minionList.Where(minion => minion.IsValidTarget(Menu.Item("minionRange").GetValue<Slider>().Value)))
            {
                var hpBarPosition = minion.HPBarPosition;
                double Dmg = Math.Ceiling(Player.GetAutoAttackDamage(minion, true));
                double nbrAttack = Math.Ceiling(minion.MaxHealth / Dmg);
                double xMultiplicator = (60 / nbrAttack);
                for (int i=1; i < nbrAttack; i++)
                {
                    if (!Menu.Item("drawBars").GetValue<Circle>().Active)
                        break;
                    float newX = hpBarPosition.X + ((float)xMultiplicator * i);
                    Vector2 posA = new Vector2(newX, hpBarPosition.Y+4);
                    Vector2 posB = new Vector2(newX, hpBarPosition.Y + 9);
                    Drawing.DrawLine(posA, posB, 2, Menu.Item("drawBars").GetValue<Circle>().Color);
                }

                if (!Menu.Item("drawFrame").GetValue<Circle>().Active)
                    return;

                Color myColor = Menu.Item("drawFrame").GetValue<Circle>().Color;
                if (minion.Health < Dmg && Menu.Item("frameColor").GetValue<Circle>().Active)
                    myColor = Menu.Item("frameColor").GetValue<Circle>().Color;
                // Upper Line          
                Drawing.DrawLine(new Vector2(hpBarPosition.X ,hpBarPosition.Y+3), new Vector2(hpBarPosition.X+61, hpBarPosition.Y + 3), 1, myColor);
                // Down Line
                Drawing.DrawLine(new Vector2(hpBarPosition.X , hpBarPosition.Y + 9), new Vector2(hpBarPosition.X + 61, hpBarPosition.Y + 9), 1, myColor);
                //// Left Line
                Drawing.DrawLine(new Vector2(hpBarPosition.X, hpBarPosition.Y + 4), new Vector2(hpBarPosition.X, hpBarPosition.Y + 9), 1, myColor);
                // Right Lin
                Drawing.DrawLine(new Vector2(hpBarPosition.X + 62, hpBarPosition.Y + 4), new Vector2(hpBarPosition.X + 62, hpBarPosition.Y + 9), 1, myColor);
            }


        }

    }
}