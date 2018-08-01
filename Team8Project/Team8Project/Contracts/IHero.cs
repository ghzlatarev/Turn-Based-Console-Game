﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Team8Project.Contracts
{
    public interface IHero
    {
        string Name { get; set; }
        int HealthPoints { get; set; }
        int DmgStartOfRange { get; set; }
        int DmgEndOfRange { get; set; }
        IHero Opponent { get; set; }
        bool HasTurn { get; set; }
        List<IAbility> Abilities { get; }
        void AddAbility(IAbility ability);
        void UseAbility(IAbility ability);
    }
}