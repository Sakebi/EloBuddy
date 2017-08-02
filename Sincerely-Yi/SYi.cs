using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Rendering;
using SharpDX;
using EloBuddy.SDK.Events;
using Activator = Sincerly_Yi.Libs.Lib_Activator;
using SincerlyMenu = Sincerly_Yi.SMenu;
using Color = System.Drawing.Color;

namespace Sincerly_Yi
{
    class SYi
    {
        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
            Bootstrap.Init(null);

        }

        //Define Spells
        public static Spell.Active W, E, R;

        public static Spell.Targeted Q;

        public static List<string> EvadeMenu = new List<string>();

        public static readonly AIHeroClient Player = ObjectManager.Player;

        public static AIHeroClient Target = null;


        //After Loading

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.ChampionName != "MasterYi") return;

            SincerlyMenu.LoadMenu();
            Activator.LoadSpells();
            Game.OnUpdate += OnGameUpdate;
            Game.OnTick += Game_OnTick;
            Gapcloser.OnGapcloser += AntiGapCloser;
            Drawing.OnDraw += GameOnDraw;
           

            Obj_AI_Base.OnBuffGain += OnBuffGain;
            Obj_AI_Base.OnProcessSpellCast += OnSpellCast;
            Obj_AI_Base.OnBuffLose += OnBuffLose;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;

            Q = new Spell.Targeted(SpellSlot.Q, 600, DamageType.Physical);
            W = new Spell.Active(SpellSlot.W, 0, DamageType.Magical);
            E = new Spell.Active(SpellSlot.E, 0, DamageType.True);
            R = new Spell.Active(SpellSlot.R, 0, DamageType.Physical);

            Chat.Print("Sincerly Yi", Color.HotPink);
            Chat.Print("By Sakebi", Color.White);
            Chat.Print("Use common sense!", Color.HotPink);

        }




        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        //On Tick
        private static void Game_OnTick(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                OnHarrass();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                OnLaneClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                OnJungle();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee)) { }
            KillSteal();
            AutoPotions();
            SmartR();
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        //When Champion Levels up

        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static void Combo()
        {
            var enemies = EntityManager.Heroes.Enemies.OrderByDescending(a => a.HealthPercent)
                .Where(a => !a.IsMe && a.IsValidTarget() && a.Distance(Player) <= Q.Range);
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (!target.IsValidTarget(Q.Range) || target == null)
            {
                return;
            }
            if (E.IsReady() && target.IsValidTarget(Q.Range) && SincerlyMenu.ComboE())
            {
                E.Cast();
            }
            if (Q.IsReady() && target.IsValidTarget(Q.Range))
                foreach (var eenemies in enemies)
                {
                    var useQ = SincerlyMenu.ComboMenu["combo.q" + eenemies.ChampionName].Cast<CheckBox>().CurrentValue;
                    if (useQ)
                    {
                        Q.Cast(target);
                    }
                }
            if (SincerlyMenu.ComboW() && Player.HealthPercent < SincerlyMenu.Hpw() && W.IsReady() &&
                target.IsValidTarget(Q.Range) && !target.IsInvulnerable)
            {
                W.Cast();
            }
            if (SincerlyMenu.ResetAa() && W.IsReady() && target.IsValidTarget(250) && !target.IsInvulnerable)
            {
                W.Cast();
                Orbwalker.ResetAutoAttack();
                Orbwalker.ResetAutoAttack();
            }
            if (R.IsReady() && SincerlyMenu.ComboR1() &&
                Player.CountEnemyChampionsInRange(1000) == SincerlyMenu.ComboREnemies() && !target.IsInvulnerable)
            {
                R.Cast();
            }
            if ((ObjectManager.Player.CountEnemyChampionsInRange(ObjectManager.Player.AttackRange) >=
                 SincerlyMenu.YoumusEnemies() ||
                 Player.HealthPercent >= SincerlyMenu.ItemsYoumuShp()) &&
                Activator.Youmus.IsReady() && SincerlyMenu.Youmus() && Activator.Youmus.IsOwned())
            {
                Activator.Youmus.Cast();
                return;
            }
            if (Player.HealthPercent <= SincerlyMenu.BilgewaterHp() &&
                SincerlyMenu.Bilgewater() &&
                Activator.Bilgewater.IsReady() && Activator.Bilgewater.IsOwned())
            {
                Activator.Bilgewater.Cast(target);
                return;
            }
            if (Activator.Smite.IsReady() && SincerlyMenu.Combosmite())
            {
                Activator.Smite.Cast(target);
            }
            if (Player.HealthPercent <= SincerlyMenu.BotrkHp() && SincerlyMenu.Botrk() &&
                Activator.Botrk.IsReady() &&
                Activator.Botrk.IsOwned())
            {
                Activator.Botrk.Cast(target);
            }



        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //Jungle Area

        public static readonly string[] MonsterList = {
            "SRU_Crab",
            "SRU_Krug",
            "SRU_Gromp",
            "SRU_Murkwolf",
            "SRU_Razorbeak",
            "TTNGolem",
            "TTNWolf",
            "TTNWraith",
            "SRU_Red",
            "SRU_Blue",
            "SRU_Dragon_Water",
            "SRU_Dragon_Fire",
            "SRU_Dragon_Earth",
            "SRU_Dragon_Air",
            "SRU_Dragon_Elder",
            "SRU_RiftHerald",
            "SRU_Baron",
            "TT_Spiderboss"
        };

        public static readonly string[] SencefulBuffs = {
            "SRU_Red",
            "SRU_Blue",
            "SRU_Dragon_Water",
            "SRU_Dragon_Fire",
            "SRU_Dragon_Earth",
            "SRU_Dragon_Air",
            "SRU_Dragon_Elder",
            "SRU_Baron",
            "SRU_RiftHerald",
            "TT_Spiderboss"
        };

        //Smite

        
        public static bool Out;
        public static bool Healing;
        public static bool Channeling;
        public static int QOff = 0, WOff = 0, EOff = 0, ROff = 0;

        public static float SmiteMonster(Obj_AI_Base target)
        {
            return Player.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Smite);
        }

        public static float SmiteEnemy(AIHeroClient target)
        {
            return Player.CalculateDamageOnUnit(target, DamageType.True,
                390 + Player.Level * 61);
        }

        private static void OnSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == "Meditate")
            {
                Healing = true;
                Channeling = true;
                Orbwalker.DisableAttacking = true;
                Orbwalker.DisableMovement = true;
                Out = true;
            }
        }

        private static void OnBuffLose(Obj_AI_Base sender, Obj_AI_BaseBuffLoseEventArgs args)
        {
            if (sender.IsMe && args.Buff.DisplayName == "Meditate")
            {
                Healing = false;
                Channeling = false;
                Orbwalker.DisableAttacking = false;
                Orbwalker.DisableMovement = false;
                Out = false;
            }
        }

        public static void AntiGapCloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (SincerlyMenu.GapcloserQ() && sender.IsEnemy && sender.IsValidTarget(Q.Range) &&
                e.End.Distance(Player) <= 250)
            {
                Q.Cast(e.End);
            }
        }

        //Spells

        private static
            void OnGameUpdate(EventArgs args)
        {
            if (Activator.Barrier != null)
                Barrier();
            if (Activator.Heal != null)
                Heal();
            if (Activator.Ignite != null)
                Ignite();
            if (Activator.Smite != null)
                Smite();
        }

        private static void Barrier()
        {
            if (Player.IsFacing(Target) && Activator.Barrier.IsReady() &&
                Player.HealthPercent <= SincerlyMenu.SpellsBarrierHp())
                Activator.Barrier.Cast();
        }

        private static void Smite()
        {
            var unit =
                ObjectManager.Get<Obj_AI_Base>()
                    .Where(
                        a =>
                            MonsterList.Contains(a.BaseSkinName) &&
                            Player.GetSummonerSpellDamage(a, DamageLibrary.SummonerSpells.Smite) >= a.Health &&
                            SincerlyMenu.SmiteMenu[a.BaseSkinName].Cast<CheckBox>().CurrentValue &&
                            Activator.Smite.IsInRange(a))
                    .OrderByDescending(a => a.MaxHealth)
                    .FirstOrDefault();
            if (unit != null && Activator.Smite.IsReady())
                Activator.Smite.Cast(unit);
        }

        private static void Ignite()
        {
            var autoIgnite = TargetSelector.GetTarget(Activator.Ignite.Range, DamageType.True);
            if (autoIgnite != null && autoIgnite.Health <= Player.GetSpellDamage(autoIgnite, Activator.Ignite.Slot) ||
                autoIgnite != null && autoIgnite.HealthPercent <= SincerlyMenu.SpellsIgniteFocus())
                Activator.Ignite.Cast(autoIgnite);
        }

        private static void Heal()
        {
            if (Activator.Heal != null && Activator.Heal.IsReady() &&
                Player.HealthPercent <= SincerlyMenu.SpellsHealHp() && Player.CountEnemyChampionsInRange(600) > 0 &&
                Activator.Heal.IsReady())
            {
                Activator.Heal.Cast();
            }
        }

        private static void OnBuffGain(Obj_AI_Base sender, Obj_AI_BaseBuffGainEventArgs args)
        {
            if (!sender.IsMe) return;

            if (args.Buff.Type == BuffType.Taunt && SincerlyMenu.Taunt())
            {
                DoQss();
            }
            if (args.Buff.Type == BuffType.Stun && SincerlyMenu.Stun())
            {
                DoQss();
            }
            if (args.Buff.Type == BuffType.Snare && SincerlyMenu.Snare())
            {
                DoQss();
            }
            if (args.Buff.Type == BuffType.Polymorph && SincerlyMenu.Polymorph())
            {
                DoQss();
            }
            if (args.Buff.Type == BuffType.Blind && SincerlyMenu.Blind())
            {
                DoQss();
            }
            if (args.Buff.Type == BuffType.Flee && SincerlyMenu.Fear())
            {
                DoQss();
            }
            if (args.Buff.Type == BuffType.Charm && SincerlyMenu.Charm())
            {
                DoQss();
            }
            if (args.Buff.Type == BuffType.Suppression && SincerlyMenu.Suppression())
            {
                DoQss();
            }
            if (args.Buff.Type == BuffType.Silence && SincerlyMenu.Silence())
            {
                DoQss();
            }
        }

        private static void DoQss()
        {
            if (Activator.Qss.IsOwned() && Activator.Qss.IsReady())
            {
                Activator.Qss.Cast();
            }

            if (Activator.Mercurial.IsOwned() && Activator.Mercurial.IsReady())
            {
                Activator.Mercurial.Cast();
            }
        }

        public static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsEnemy && EvadeMenu.Any(el => el == args.SData.Name) &&
                Player.Distance(sender) <= args.SData.CastRange)
            {
                if (Q.IsReady() && (SincerlyMenu.EvadeMenu[args.SData.Name].Cast<Slider>().CurrentValue == 1 ||
                                    SincerlyMenu.EvadeMenu[args.SData.Name].Cast<Slider>().CurrentValue == 3))
                {
                    if (args.SData.Name == "JaxCounterStrike")
                    {
                        Core.DelayAction(() => Q.Cast(Target), 2000 - Game.Ping - 100);
                        return;
                    }

                    if (args.SData.Name == "KarthusFallenOne")
                    {
                        Core.DelayAction(() => Q.Cast(Target), 3000 - Game.Ping - 100);
                        return;
                    }

                    if (args.SData.Name == "ZedR" && args.Target.IsMe)
                    {
                        Core.DelayAction(() => Q.Cast(Target), 750 - Game.Ping - 100);
                        return;
                    }

                    if (args.SData.Name == "SoulShackles")
                    {
                        Core.DelayAction(() => Q.Cast(Target), 3000 - Game.Ping - 100);
                        return;
                    }

                    if (args.SData.Name == "AbsoluteZero")
                    {
                        Core.DelayAction(() => Q.Cast(Target), 3000 - Game.Ping - 100);
                        return;
                    }

                    if (args.SData.Name == "NocturneUnspeakableHorror" && args.Target.IsMe)
                    {
                        Core.DelayAction(() => Q.Cast(Target), 2000 - Game.Ping - 100);
                        return;
                    }

                    Core.DelayAction(delegate {
                        if (Target != null && Target.IsValidTarget(Q.Range)) Q.Cast(Target);
                    }, (int)args.SData.SpellCastTime - Game.Ping - 100);

                    Core.DelayAction(delegate {
                        if (sender.IsValidTarget(Q.Range)) Q.Cast(sender);
                    }, (int)args.SData.SpellCastTime - Game.Ping - 50);
                }
                else if (W.IsReady() && Player.IsFacing(sender) &&
                         SincerlyMenu.EvadeMenu[args.SData.Name].Cast<Slider>().CurrentValue > 1 &&
                         (args.Target != null && args.Target.IsMe ||
                          new Geometry.Polygon.Rectangle(args.Start, args.End, args.SData.LineWidth).IsInside(Player) ||
                          new Geometry.Polygon.Circle(args.End, args.SData.CastRadius).IsInside(Player)))
                {
                    var delay =
                        (int)
                        (Player.Distance(sender) / ((args.SData.MissileMaxSpeed + args.SData.MissileMinSpeed) / 2) *
                         1000) - 150 + (int)args.SData.SpellCastTime;

                    if (args.SData.Name != "ZedR" && args.SData.Name != "NocturneUnpeakableHorror")
                    {
                        Core.DelayAction(() => W.Cast(), delay);
                        if (Target != null && Target.IsValidTarget())
                            Core.DelayAction(() => EloBuddy.Player.IssueOrder(GameObjectOrder.AttackTo, Target),
                                delay + 100);
                    }
                }
            }
        }




        private static void KillSteal()
        {
            var enemies = EntityManager.Heroes.Enemies.OrderByDescending(a => a.HealthPercent)
                .Where(
                    a =>
                        !a.IsMe && a.IsValidTarget() && a.Distance(Player) <= Q.Range && !a.IsDead && !a.IsZombie &&
                        a.HealthPercent <= 35);
            foreach (
                var target in enemies)
            {
                if (!target.IsValidTarget())
                {
                    return;
                }

                if (SincerlyMenu.KillstealQ() && Q.IsReady() &&
                    target.Health + target.AttackShield <
                    Player.GetSpellDamage(target, SpellSlot.Q))
                {
                    Q.Cast(target);
                }
                if (target.IsValidTarget(570) && target.Health < SmiteEnemy(target) && SincerlyMenu.KillstealSmite() &&
                    Activator.Smite.IsReady())
                {
                    Activator.Smite.Cast(target);
                }
            }
        }

        private static void OnLaneClear()
        {
            var count =
                EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.ServerPosition,
                    Player.AttackRange, false).Count();
            var source =
                EntityManager.MinionsAndMonsters.GetLaneMinions()
                    .OrderBy(a => a.MaxHealth)
                    .FirstOrDefault(a => a.IsValidTarget(Q.Range));
            if (count == 0) return;
            if (SincerlyMenu.LaneQ() && Q.IsReady() && Player.ManaPercent > SincerlyMenu.LaneMana())
            {
                Q.Cast(source);
            }
            if (SincerlyMenu.LaneE() && E.IsReady() && Player.ManaPercent > SincerlyMenu.LaneMana())
            {
                E.Cast();
            }
        }

        private static
            void OnJungle()
        {
            var junleminions =
                EntityManager.MinionsAndMonsters.GetJungleMonsters()
                    .OrderByDescending(a => a.MaxHealth)
                    .FirstOrDefault(a => a.IsValidTarget(900));

            if (SincerlyMenu.JungleQ() && Q.IsReady() && Player.ManaPercent > SincerlyMenu.Junglemana() &&
                junleminions.IsValidTarget(Q.Range))
            {
                Q.Cast(junleminions);
            }
            if (SincerlyMenu.JungleE() && E.IsReady() && Player.ManaPercent > SincerlyMenu.Junglemana() &&
                junleminions.IsValidTarget(Q.Range))
            {
                E.Cast();
            }
            if (SincerlyMenu.JungleW() && W.IsReady() && Player.ManaPercent > SincerlyMenu.Junglemana() &&
                Player.HealthPercent < 50 &&
                junleminions.IsValidTarget(Q.Range))
            {
                W.Cast();
            }
        }

        private static void OnHarrass()
        {
            var enemies = EntityManager.Heroes.Enemies.OrderByDescending(a => a.HealthPercent)
                .Where(a => !a.IsMe && a.IsValidTarget() && a.Distance(Player) <= Q.Range);
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (!target.IsValidTarget())
            {
                return;
            }

            if (Q.IsReady() && target.IsValidTarget(700))
                foreach (var eenemies in enemies)
                {
                    var useQ = SincerlyMenu.HarassMenu["harass.Q" + eenemies.ChampionName].Cast<CheckBox>()
                        .CurrentValue;
                    if (useQ && Player.ManaPercent >= SincerlyMenu.HarassQe())
                    {
                        Q.Cast(target);
                    }
                }
        }

        private static
            void SmartR()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (!target.IsValidTarget(Q.Range) || target == null)
            {
                return;
            }

            if (R.IsReady() && SincerlyMenu.ComboR() && Player.CountEnemyChampionsInRange(1000) == 1 &&
                target.HealthPercent <= 45 && !target.IsInvulnerable)
            {
                R.Cast();
            }
        }

        private static void GameOnDraw(EventArgs args)
        {
            if (SincerlyMenu.Nodraw()) return;

            {
                if (SincerlyMenu.DrawingsQ())
                {
                    new Circle { Color = Color.HotPink , Radius = Q.Range, BorderWidth = 2f }.Draw(Player.Position);
                
                }
            }
            
        }

        private static
            void AutoPotions()
        {
            if (SincerlyMenu.SpellsPotionsCheck() && !Player.IsInShopRange() &&
                Player.HealthPercent <= SincerlyMenu.SpellsPotionsHp() &&
                !(Player.HasBuff("RegenerationPotion") || Player.HasBuff("ItemCrystalFlaskJungle") ||
                  Player.HasBuff("ItemMiniRegenPotion") || Player.HasBuff("ItemCrystalFlask") ||
                  Player.HasBuff("ItemDarkCrystalFlask")))
            {
                if (Activator.HuntersPot.IsReady() && Activator.HuntersPot.IsOwned())
                {
                    Activator.HuntersPot.Cast();
                }
                if (Activator.CorruptPot.IsReady() && Activator.CorruptPot.IsOwned())
                {
                    Activator.CorruptPot.Cast();
                }
                if (Activator.Biscuit.IsReady() && Activator.Biscuit.IsOwned())
                {
                    Activator.Biscuit.Cast();
                }
                if (Activator.HpPot.IsReady() && Activator.HpPot.IsOwned())
                {
                    Activator.HpPot.Cast();
                }
                if (Activator.RefillPot.IsReady() && Activator.RefillPot.IsOwned())
                {
                    Activator.RefillPot.Cast();
                }
            }
            if (SincerlyMenu.SpellsPotionsCheck() && !Player.IsInShopRange() &&
                Player.ManaPercent <= SincerlyMenu.SpellsPotionsM() &&
                !(Player.HasBuff("RegenerationPotion") || Player.HasBuff("ItemCrystalFlaskJungle") ||
                  Player.HasBuff("ItemMiniRegenPotion") || Player.HasBuff("ItemCrystalFlask") ||
                  Player.HasBuff("ItemDarkCrystalFlask")))
            {
                if (Activator.CorruptPot.IsReady() && Activator.CorruptPot.IsOwned())
                {
                    Activator.CorruptPot.Cast();
                }
            }









        }
    }
}