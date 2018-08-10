﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team8Project.Contracts;
using Team8Project.Common;


namespace Team8Project.Models.Terrains
{
    public abstract class Terrain: ITerrain
    {
        private bool isDay = true;
        public bool IsDay
        {
            get
            {
                return this.isDay;
            }
            set
            {
                this.isDay = value;
            }
        }
        public abstract void HeroEffect(IHero hero);
        public abstract void ContinuousEffect(IHero hero);
    }
}