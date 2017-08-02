using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;

namespace Bubble_Butt_Tristana
{
    public static class Program
    {
        public static uint TrisRange = 550;
        public static Spell.Active Q;
        private static Spell.Skillshot W;
        private static Spell.Targeted E, R;
        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static Menu BubbleButtTristanaMenu,ComboMenu,DrawingsMenu;

        
        
        private static void Game_OnTick(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.Equals(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }
            if (Orbwalker.ActiveModesFlags.Equals((Orbwalker.ActiveModes.LaneClear) != 0))
            {
                Clear();
            }
            
        }

        private static void Clear()
        {
            var minion = TargetSelector.GetTarget(E.Range, DamageType.Physical);
        } 
        private static void Combo()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);

            var wpred = W.GetPrediction(target);

            if (ComboMenu["Q"].Cast<CheckBox>().CurrentValue)
            {
                if (target.IsValidTarget(E.Range) && Q.IsReady())
                {
                    Q.Cast();
                }
            }

            if (ComboMenu["W"].Cast<CheckBox>().CurrentValue)
            {
                if (target.IsValidTarget(W.Range) && W.IsReady() && wpred.HitChance >= HitChance.High && Q.IsReady())
                {
                    W.Cast(target);
                    if (W.IsReady())
                    {
                        W.Cast(target);
                    }
                }
            }

            if (ComboMenu["E"].Cast<CheckBox>().CurrentValue)
            {
                if (target.IsValidTarget(E.Range) && E.IsReady())
                {
                    E.Cast(target);
                }

            }

            if (ComboMenu["R"].Cast<CheckBox>().CurrentValue)
            {
                if (target.IsValidTarget(R.Range) && R.IsReady())
                {

                    var damage = Player.Instance.GetSpellDamage(target, SpellSlot.R);

                   

                    if (damage >= target.Health)
                    {
                        R.Cast(target);
                    }
                   
                }
            }


          

       
         

            if (target == null)
            {
                return;
            }
        }


        
        public static void OnLvlUp(Obj_AI_Base sender, Obj_AI_BaseLevelUpEventArgs args)
        {
            
            TrisRange = TrisRange + 8;
        }
        private static List<Spell.SpellBase> SpellList = new List<Spell.SpellBase>();

       

        private static void Drawing_OnDraw(EventArgs args)
        {
          
            if (DrawingsMenu["W"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(W.IsReady() ? Color.HotPink : Color.White, 900, User);
            }

        }


        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            
            Drawing.OnDraw += Drawing_OnDraw;
            BubbleButtTristanaMenu = MainMenu.AddMenu("Bubble Butt Tristana", "Bubble Butt Tristana");
            BubbleButtTristanaMenu.AddLabel("Welcome to Bubble Butt Tristana!", 20);
            BubbleButtTristanaMenu.AddLabel("(‿ˠ‿)", 20);
            DrawingsMenu = BubbleButtTristanaMenu.AddSubMenu("Drawings");
            ComboMenu = BubbleButtTristanaMenu.AddSubMenu("Combo");
            ComboMenu.Add("Q", new CheckBox("Use Rapid Fire"));
            ComboMenu.Add("W", new CheckBox("Use Rocket Jump"));
            ComboMenu.Add("E", new CheckBox("Use Explosive Charge"));
            ComboMenu.Add("R", new CheckBox("Use Buster Shot"));
            Obj_AI_Base.OnLevelUp += OnLvlUp;



            Q = new Spell.Active(SpellSlot.Q);
            W = new Spell.Skillshot(spellSlot: SpellSlot.W, spellRange: 900, skillShotType: SkillShotType.Linear, castDelay: 250, spellSpeed: 1000, spellWidth: 75) { AllowedCollisionCount = 0 };
            //RANGE BASED ON LEVEL
            E = new Spell.Targeted(SpellSlot.E, TrisRange);
            R = new Spell.Targeted(SpellSlot.R, TrisRange, DamageType.Magical);


            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            foreach (var Spell in SpellList)
            {
                DrawingsMenu.Add(SpellSlot.W.ToString(),new CheckBox("Draw Rocket Jump"));
            }

            Game.OnTick += Game_OnTick;

            if (User.ChampionName != "Tristana")
            {
                return;
            }
            Chat.Print("<font color='#FF69B4'>Bubble Butt Tristana Loaded! </font>");
            Console.WriteLine("Bubble Butt Tristana Loaded!");

        }

        public static AIHeroClient User = Player.Instance;



    }
}
