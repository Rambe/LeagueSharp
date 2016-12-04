using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace LeeSin_EloClimber
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad; // Load Game
        }

        static void OnLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != "LeeSin")
                return;

            LeeSin.Load();

            Game.PrintChat("<font color=\"#FF001E\">[Lee Sin] Elo Climber </font><font color=\"#FF980F\"> Loaded</font>");
        }
    }
}
