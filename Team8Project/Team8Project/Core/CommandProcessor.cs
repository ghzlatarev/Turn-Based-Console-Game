﻿using System;
using System.Collections.Generic;
using Team8Project.Common.Enums;
using Team8Project.Contracts;
using Team8Project.Core.Commands;
using Team8Project.Data;

namespace Team8Project.Core
{
    public class CommandProcessor
    {
        private IFactory factory;
        private TurnProcessor turn;
        private readonly IDataContainer data;
        private readonly ICommandProvider commandProvider;
        private Dictionary<string, string> heroSelection;
        private Dictionary<string, string> abilitySelection;

        public CommandProcessor(IFactory factory, TurnProcessor turn, IDataContainer data, ICommandProvider commandProvider)
        {
            this.factory = factory;
            this.turn = turn;
            this.data = data;
            this.commandProvider = commandProvider;
            this.heroSelection = new Dictionary<string, string>()
            {
                { "1", "CreateAssasin"},
                { "2", "CreateWarrior"},
                { "3", "CreateMage"},
                { "4", "CreateCleric"},
            };
            this.factory = factory;
            this.turn = turn;
            this.data = data;
            this.commandProvider = commandProvider;
            this.heroSelection = new Dictionary<string, string>()
            {
                { "1", "CreateAssasin"},
                { "2", "CreateWarrior"},
                { "3", "CreateMage"},
                { "4", "CreateCleric"},
            };
            this.abilitySelection = new Dictionary<string, string>()
            {
                { "1", "SelectBasicAbility"},
                { "2", "SelectDamageAbility"},
                { "3", "SelectEffectAbility"}
            };
        }


        public void ProcessCommand(string[] players)
        {
            foreach (var player in players)
            {
                var command = this.commandProvider.GetCommand(heroSelection[player].ToLower());
                command.Execute();
            }
        }

        public void ProcessCommand(string key)
        {
            var command = this.commandProvider.GetCommand(abilitySelection[key].ToLower());
            command.Execute();
        }
    }
}
