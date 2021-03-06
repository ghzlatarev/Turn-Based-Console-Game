﻿using Team8Project.Core.Advanced;
using Team8Project.Core.Contracts;
using Team8Project.Data;

namespace Team8Project.Core.Commands.SelectAbility
{
    public class SelectDamageAbilityCommand : Command, ICommand
    {
        private readonly ITurnProcessor turn;

        public SelectDamageAbilityCommand(IFactory factory, IDataContainer data, ITurnProcessor turn) : base(factory, data)
        {
            this.turn = turn;
        }

        public override void Execute()
        {
            this.data.SelectedAbility = turn.ActiveHero.Abilities[1];
        }
    }
}
