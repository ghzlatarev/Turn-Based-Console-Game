﻿using System.Linq;
using System.Text;
using Team8Project.Common.Enums;
using Team8Project.Contracts;
using Team8Project.Core;

namespace Team8Project.Models.Terrains
{
    public class Tundra : Terrain
    {
        //create an object of SingleObject
        private static ITerrain instance;

        //make the constructor private so that this class cannot be
        //instantiated
        private Tundra() { }

        public static ITerrain Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Tundra();
                }
                return instance;
            }
        }

        public override void HeroEffect(IHero hero)
        {
            switch (hero.HeroClass)
            {
                case HeroClass.Warrior:
                    hero.HealthPoints -= 25;
                    GameEngine.Instance.Log.AppendLine(hero.Name + "'s healthpoints decreased by 25");
                    break;
                case HeroClass.Assasin:
                    hero.HealthPoints -= 50;
                    GameEngine.Instance.Log.AppendLine(hero.Name + "'s health points decreased by 50");
                    break;
                case HeroClass.Cleric:
                    foreach (var ability in hero.Abilities.OfType<IDamagingAbility>())
                    {
                        ability.AbilityPower -= 2;
                    }
                    GameEngine.Instance.Log.AppendLine(hero.Name + "'s damaging abilities decreased by 2");
                    break;
                case HeroClass.Mage:
                    foreach (var ability in hero.Abilities.OfType<IDamagingAbility>())
                    {
                        ability.AbilityPower += 5;
                    }
                    GameEngine.Instance.Log.AppendLine(hero.Name + "'s damaging abilities power increased by 5");
                    break;
            }
        }
        public override void ContinuousEffect(IHero hero)
        {
            //TODO FIX!
            if (this.IsDay)
            {
                if (hero.AppliedEffects.Count != 0)
                {
                    var effects = hero.AppliedEffects;

                    effects
                        .Where(e => e.Type == EffectType.Incapacitated)
                        .ToList()
                        .ForEach(e => e.CurrentStacks++);
                    GameEngine.Instance.Log.AppendLine("'s incapacitating effects' duration increased by 1");
                }
            }
            else
            {
                foreach (var ability in hero.Abilities.OfType<IEffect>())
                {
                    if(ability.OnCD == true)
                    {
                        ability.OnCD = false;
                        GameEngine.Instance.Log.AppendLine(ability.Name + "'s cooldown changed");
                    }
                    else
                    {
                        var effects = hero.Abilities;

                        effects
                            .Where(e => e.Type == EffectType.HOT)
                            .ToList()
                            .ForEach(e => e.AbilityPower--);
                        GameEngine.Instance.Log.AppendLine(ability.Name + "'s HOT abilities heal decreased by 1");
                    }
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (!this.IsDay)
            {
                //sb.AppendLine("Incapacitating effects extended with 1 turn");
            }
            else
            {
                //sb.AppendLine("Hero max damage reduced by 2");
            }
            return sb.ToString();
        }
    }
}
