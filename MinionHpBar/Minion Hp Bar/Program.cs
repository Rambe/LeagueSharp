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
        public static Obj_AI_Base myHero = ObjectManager.Player;
        public static Menu myMenu;
        
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_Load;
        }

        private static void Game_Load(EventArgs args)
        {

            initMenu();
            Game.PrintChat("<font color=\"#FF001E\">Minion HP Bar - </font><font color=\"#FF980F\"> Loaded</font>");

            Drawing.OnDraw += OnDraw;
        }

        private static void initMenu()
        {
            myMenu = new Menu("Minion HP Bar", "RambeMinionHpBar", true);

            myMenu.AddSubMenu(new Menu("Draw", "Draw"));
                myMenu.SubMenu("Draw").AddItem(new MenuItem("drawFrame", "Draw frame").SetValue(new Circle(true, Color.Black)));
                myMenu.SubMenu("Draw").AddItem(new MenuItem("frameColor", "Change frame color when minion dead").SetValue(new Circle(true, Color.White)));
                myMenu.SubMenu("Draw").AddItem(new MenuItem("drawBars", "Draw Bars").SetValue(new Circle(true, Color.Black)));

            myMenu.AddItem(new MenuItem("minionRange", "Minion Range").SetValue(new Slider(1500, 200, 2000)));

            myMenu.AddItem(new MenuItem("void", ""));
            myMenu.AddItem(new MenuItem("author", "Author: Rambe"));

            myMenu.AddToMainMenu();

        }

        private static void OnDraw(EventArgs args)
        {
            
            if (myHero == null)
                return;

            var enemyMinions = MinionManager.GetMinions(myHero.Position, myMenu.Item("minionRange").GetValue<Slider>().Value, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.None);      
            foreach (var minion in enemyMinions.Where(minion => minion.IsValidTarget(myMenu.Item("minionRange").GetValue<Slider>().Value)))
            {
                var hpBarPosition = minion.HPBarPosition;
                double Dmg = Math.Ceiling(myHero.GetAutoAttackDamage(minion, true));
                double nbrAttack = Math.Ceiling(minion.MaxHealth / Dmg);
                double xMultiplicator = (60 / nbrAttack);

                // Draw Bars
                if (myMenu.Item("drawBars").GetValue<Circle>().Active)
                {
                    for (int i = 1; i < nbrAttack; i++)
                    {
                        float newX = hpBarPosition.X + ((float)xMultiplicator * i);
                        Vector2 posA = new Vector2(newX, hpBarPosition.Y + 4);
                        Vector2 posB = new Vector2(newX, hpBarPosition.Y + 9);
                        Drawing.DrawLine(posA, posB, 2, myMenu.Item("drawBars").GetValue<Circle>().Color);
                    }
                }

                // Draw Frame
                if (!myMenu.Item("drawFrame").GetValue<Circle>().Active)
                    return;

                Color myColor = myMenu.Item("drawFrame").GetValue<Circle>().Color;
                if (minion.Health < Dmg && myMenu.Item("frameColor").GetValue<Circle>().Active)
                    myColor = myMenu.Item("frameColor").GetValue<Circle>().Color;
       
                Drawing.DrawLine(new Vector2(hpBarPosition.X ,hpBarPosition.Y+3), new Vector2(hpBarPosition.X+61, hpBarPosition.Y + 3), 1, myColor);
                Drawing.DrawLine(new Vector2(hpBarPosition.X , hpBarPosition.Y + 9), new Vector2(hpBarPosition.X + 61, hpBarPosition.Y + 9), 1, myColor);
                Drawing.DrawLine(new Vector2(hpBarPosition.X, hpBarPosition.Y + 4), new Vector2(hpBarPosition.X, hpBarPosition.Y + 9), 1, myColor);
                Drawing.DrawLine(new Vector2(hpBarPosition.X + 62, hpBarPosition.Y + 4), new Vector2(hpBarPosition.X + 62, hpBarPosition.Y + 9), 1, myColor);
            }


        }

    }
}
