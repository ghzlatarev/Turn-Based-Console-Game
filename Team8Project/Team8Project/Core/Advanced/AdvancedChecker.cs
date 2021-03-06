﻿using System;
using System.Linq;
using System.Threading;
using Team8Project.Common;
using Team8Project.Common.Enums;
using Team8Project.Core.Commands;
using Team8Project.Core.Contracts;
using Team8Project.Data;
using Team8Project.IO.Contracts;

namespace Team8Project.Core.Advanced
{
    public class AdvancedChecker : IAdvancedChecker
    {
        private readonly IReader reader;
        private readonly IWriter writer;
        private readonly ITurnProcessor turn;
        private readonly ICommandProcessor processor;
        private readonly ITerrainManager terrainManager;
        private readonly IDataContainer data;
        private readonly IRenderer renderer;

        public AdvancedChecker(IReader reader, IWriter writer, IRenderer renderer, ITurnProcessor turn,
            ICommandProcessor processor, IDataContainer data, ITerrainManager terrainManager)
        {
            this.reader = reader;
            this.writer = writer;
            this.renderer = renderer;
            this.turn = turn;
            this.processor = processor;
            this.data = data;
            this.terrainManager = terrainManager;
        }

        public void SetAbilityThatIsReadyForUse()
        {
            var selectedAbility = this.data.SelectedAbility;

            if (selectedAbility.OnCD == true)
            {
                this.data.Log.AppendLine("Chosen ability is on cooldown, choose another");
                this.writer.PrintOnPosition(Constants.LOG_ROW_POS, Constants.LOG_COL_POS, this.data.Log.ToString());
                Console.SetCursorPosition(2, 9);
                while (this.data.SelectedAbility.OnCD == true)
                {
                    var selectedAbilityCommand = this.CheckIfabilityInputIsValid(this.reader.ConsoleReadKey());
                    this.writer.ConsoleClear();
                    this.writer.WriteLine("I told you to choose other option!!! Try again. I will be wathcing you!");
                    this.processor.ProcessCommand(selectedAbilityCommand);
                }
            }
        }

        public void CheckForTerrainContitiousEffect()
        {
            if (turn.TurnNumber % 3 == 0)
            {
                this.data.Log.AppendLine(this.terrainManager.ChangeDayNight());
            }
            string continiousEffect = this.terrainManager.ApplyContinuousEffect(this.turn.ActiveHero);
            if (continiousEffect != string.Empty) { this.data.Log.AppendLine(continiousEffect); }

            this.renderer.UpdataScreen();
        }

        public string CheckIfabilityInputIsValid(string abilityHotKey)
        {
            if (int.Parse(abilityHotKey) < 1 || int.Parse(abilityHotKey) > 3)
            {
                this.writer.WriteLine(" Invalid command");
                while (int.Parse(abilityHotKey) < 1 || int.Parse(abilityHotKey) > 3)
                {
                    abilityHotKey = this.reader.ConsoleReadKey();
                    this.writer.ConsoleClear();
                    this.writer.WriteLine("Invalid command, I told you to choose other option!!! Try again. I will be wathcing you!");
                }
            }
            return abilityHotKey;
        }


        public bool CheckForIncapacitation()
        {
            if (turn.ActiveHero.IsIncapacitated)
            {
                var effect = this.turn.ActiveHero.AppliedEffects.FirstOrDefault(e => e.Type == EffectType.Incapacitated);
                this.data.Log.AppendLine(effect.Affect());
                return true;
            }
            return false;
        }

        public bool CheckIfGameIsOver()
        {
            if (turn.ActiveHero.Opponent.HealthPoints < 0)
            {
                this.writer.ConsoleClear();
                Console.Beep();
                this.writer.PrintOnPosition(0, 0, $"{turn.ActiveHero.Name.ToUpper()} WON!", ConsoleColor.Green);
                Thread.Sleep(5000);
                Console.Beep();
                return true;
            }
            return false;
        }
    }
}
