using EloBuddy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy.SDK;
namespace Sincerly_Yi.Libs
{
    class Lib_SpellDamage
    {
        public static class SpellDamage
        {
            public static float GetTotalDamage(AIHeroClient target)
            {

                var damage = SYi.Player.GetAutoAttackDamage(target);
                if (SYi.R.IsReady())
                    damage = SYi.Player.GetSpellDamage(target, SpellSlot.R);
                if (SYi.E.IsReady())
                    damage = SYi.Player.GetSpellDamage(target, SpellSlot.E);
                if (SYi.W.IsReady())
                    damage = SYi.Player.GetSpellDamage(target, SpellSlot.W);
                if (SYi.Q.IsReady())
                    damage = SYi.Player.GetSpellDamage(target, SpellSlot.Q);

                return damage;
            }
        }
    }
}