﻿using System;
using System.Linq;
using Team8Project.Common;
using Team8Project.Contracts;

namespace Team8Project.Core
{
    public class EffectManager
    {
        private static EffectManager instance;
        private EffectManager()
        {
        }

        //TODO : MESSAGES
        public int TransformDamage(int damage, IHero activeHero)
        {
            foreach (var effect in activeHero.AppliedEffects)
            {
                switch (effect.Type)
                {
                    case EffectType.Buff:
                        damage += effect.AbilityPower;
                        effect.CurrentStacks--;

                        break;
                    case EffectType.Debuff:
                        if (damage - effect.AbilityPower < 0) { damage = 0; }
                        else { damage -= effect.AbilityPower; }
                        effect.CurrentStacks--;
                        break;
                }

            }
            foreach (var effect in activeHero.Opponent.AppliedEffects)
            {
                switch (effect.Type)
                {
                    case EffectType.Resistance:
                        damage = 0;
                        effect.CurrentStacks--;
                        break;
                }
            }
            return damage;
        }

        public void AtTurnStart(IHero activeHero)
        {
            foreach (var effect in activeHero.AppliedEffects)
            {
                switch (effect.Type)
                {
                    case EffectType.DOT:
                        activeHero.HealthPoints -= effect.AbilityPower;
                        Console.WriteLine($"{activeHero.Name} takes {effect.AbilityPower}dmg from {effect.Name}");
                        effect.CurrentStacks--;
                        break;
                    case EffectType.HOT:
                        activeHero.HealthPoints += effect.AbilityPower;
                        Console.WriteLine($"{activeHero.Name} heals {effect.AbilityPower} hp from {effect.Name}");
                        effect.CurrentStacks--;
                        break;
                }

            }
        }


        public static EffectManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new EffectManager();
                }
                return instance;
            }
        }
    }
}