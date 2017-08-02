﻿using EloBuddy;
using EloBuddy.SDK;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Sincerly_Yi.Libs
{
    class Lib_DamageIndicator
    {
        public static class DamageIndicator
        {
            public delegate float DamageToUnitDelegate(AIHeroClient hero);

            private const int BarWidth = 110;
            private const int LineThickness = 9;

            private static readonly Vector2 BarOffset = new Vector2(2, 10 + LineThickness / 2f);

            private static System.Drawing.Color _drawingColor;

            private static DamageToUnitDelegate DamageToUnit { get; set; }

            public static System.Drawing.Color DrawingColor = System.Drawing.Color.HotPink;
      

            public static bool HealthbarEnabled { get; set; }
            public static bool PercentEnabled { get; set; }

            public static void Initialize(DamageToUnitDelegate damageToUnit)
            {
                // Apply needed field delegate for damage calculation
                DamageToUnit = damageToUnit;
                DrawingColor = System.Drawing.Color.Aqua;
                HealthbarEnabled = true;

                // Register event handlers
                Drawing.OnEndScene += OnEndScene;
            }

            private static void OnEndScene(EventArgs args)
            {
                if (HealthbarEnabled || PercentEnabled)
                {
                    foreach (var unit in EntityManager.Heroes.Enemies.Where(u => u.IsValidTarget() && u.IsHPBarRendered))
                    {
                        // Get damage to unit
                        var damage = DamageToUnit(unit);

                        // Continue on 0 damage
                        if (damage <= 0)
                        {
                            continue;
                        }

                        if (HealthbarEnabled)
                        {
                            // Get remaining HP after damage applied in percent and the current percent of health
                            var damagePercentage = ((unit.TotalShieldHealth() - damage) > 0
                                                       ? (unit.TotalShieldHealth() - damage)
                                                       : 0) /
                                                   (unit.MaxHealth + unit.AllShield + unit.AttackShield + unit.MagicShield);
                            var currentHealthPercentage = unit.TotalShieldHealth() /
                                                          (unit.MaxHealth + unit.AllShield + unit.AttackShield +
                                                           unit.MagicShield);

                            // Calculate start and end point of the bar indicator
                            var startPoint =
                                new Vector2((int)(unit.HPBarPosition.X + BarOffset.X + damagePercentage * BarWidth),
                                    (int)(unit.HPBarPosition.Y + BarOffset.Y) - 5);
                            var endPoint =
                                new Vector2(
                                    (int)(unit.HPBarPosition.X + BarOffset.X + currentHealthPercentage * BarWidth) + 1,
                                    (int)(unit.HPBarPosition.Y + BarOffset.Y) - 5);

                            // Draw the line
                            Drawing.DrawLine(startPoint, endPoint, LineThickness, DrawingColor);
                        }

                        if (PercentEnabled)
                        {
                            // Get damage in percent and draw next to the health bar
                            Drawing.DrawText(unit.HPBarPosition - new Vector2(0, 12),
                                unit.TotalShieldHealth() < damage ? System.Drawing.Color.LawnGreen : System.Drawing.Color.Red,
                                string.Concat(Math.Ceiling((damage / unit.TotalShieldHealth()) * 100), "%"), 10);
                        }
                    }
                }
            }
        }
    }
}