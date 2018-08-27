﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Team8Project.Common;
using Team8Project.Common.Enums;
using Team8Project.Contracts;
using Team8Project.Core.Contracts;
using Team8Project.Data;
using Team8Project.IO;
using Team8Project.IO.Contracts;
using Team8Project.Models.Magic.EffectAbilities;

namespace Team8Project.Core
{

    public class GameEngine : IEngine
    {
        private readonly IFactory factory;
        private TurnProcessor turn;
        //  private IEffectManager effect;
        private readonly IReader reader;
        private readonly IWriter writer;
        private readonly IDataContainer data;
        private TerrainManager terrainManager;
        private CommandProcessor commandProcessor;
        private bool endGame = false;

        public GameEngine(IFactory factory, TurnProcessor turn, /*IEffectManager effect,*/ IReader reader, IWriter writer, CommandProcessor commandProcessor, IDataContainer data,
            TerrainManager terrainManager)
        {
            this.factory = factory;
            this.turn = turn;
            //    this.effect = effect;
            this.reader = reader;
            this.writer = writer;
            this.commandProcessor = commandProcessor;
            this.data = data;
            this.terrainManager = terrainManager;
        }

        public void Run()
        {
            PreBuildGame();

            //START GAME
            while (true)
            {
                this.UpdataLog();
                this.data.Log.AppendLine($"Turn {this.turn.TurnNumber}: ");

                if (turn.TurnNumber % 3 == 0)
                {
                    this.data.Log.AppendLine(this.terrainManager.ChangeDayNight());
                }

                string continiousEffect = this.terrainManager.ApplyContinuousEffect(this.turn.ActiveHero);
                if (continiousEffect != string.Empty) { this.data.Log.AppendLine(continiousEffect); }


                this.UpdataLog();

                Act(turn.ActiveHero); //first hero move
                turn.EndAct();
                this.UpdataLog();
                if (this.endGame) { return; }
                Act(turn.ActiveHero); //second hero move
                turn.EndAct();
                this.UpdataLog();

                turn.UpdateCooldowns(turn.ActiveHero);
                turn.NextTurn();
                this.UpdataLog();
            }
        }

        private void PreBuildGame()
        {
            Console.SetWindowSize(160, 40);

            //inital screen for creating heros
            this.writer.WriteLine(string.Format(Constants.INITIAL_MESSAGE, HeroClass.Assasin, HeroClass.Warrior, HeroClass.Mage, HeroClass.Cleric));
            this.writer.WriteLine(new String('-', Console.WindowWidth));


            string[] players = new string[2];

            this.writer.ConsoleWrite("Player 1: ");
            players[0] = this.reader.ConsoleReadKey();
            this.writer.WriteLine("");
            this.writer.ConsoleWrite("Player 2: ");
            players[1] = this.reader.ConsoleReadKey();
            this.writer.ConsoleClear();

            commandProcessor.ProcessCommand(players);

            turn.SetFirstTurnActiveHero();
            factory.CreateSpellBook(turn.ActiveHero);
            factory.CreateSpellBook(turn.ActiveHero.Opponent);

            this.terrainManager.SetTerrain();
            turn.ActiveHero.InitializeTerrain(this.terrainManager.Terrain);
            turn.ActiveHero.Opponent.InitializeTerrain(this.terrainManager.Terrain);
        }

        private void Act(IHero activeHero)
        {
            //checks for status incapacitated
            if (turn.ActiveHero.IsIncapacitated)
            {
                var effect = this.turn.ActiveHero.AppliedEffects.FirstOrDefault(e => e.Type == EffectType.Incapacitated);
                this.data.Log.AppendLine(effect.Affect());
            }
            else
            {
                foreach (var effect in turn.ActiveHero.AppliedEffects.ToList())
                {
                    var resultToBeLogged = effect.Affect();
                    if (resultToBeLogged != string.Empty) { this.data.Log.AppendLine(resultToBeLogged); }
                }

                this.UpdataLog();
                // effect.RemoveExpired(activeHero);

                writer.WriteLine($"{turn.ActiveHero.HeroClass.ToString()} { turn.ActiveHero.Name} is active. HP: {turn.ActiveHero.HealthPoints}");
                this.writer.WriteLine($"{turn.ActiveHero.Name}'s abilities: ");

                int pos = 0;
                foreach (var ability in turn.ActiveHero.Abilities)
                {
                    pos++;
                    writer.WriteLine($"{pos}. {ability.Print()}");
                }


                if (turn.ActiveHero.AppliedEffects.Count == 0) { this.writer.WriteLine("Applied effects: No effects."); }
                else { this.writer.WriteLine($"Applied effects: {string.Join(", ", turn.ActiveHero.AppliedEffects)}"); }

                var selectAbilityCommand = this.reader.ConsoleReadKey();
                //Checks if the commnad is valid.
                if (int.Parse(selectAbilityCommand) < 1 || int.Parse(selectAbilityCommand) > 3)
                {
                    this.writer.WriteLine(" Invalid command");
                    while (int.Parse(selectAbilityCommand) < 1 || int.Parse(selectAbilityCommand) > 3)
                    {
                        selectAbilityCommand = this.reader.ConsoleReadKey();
                        this.writer.ConsoleClear();
                        this.writer.WriteLine("Invalid command, I told you to choose other option!!! Try again. I will be wathcing you!");
                    }
                }

                var selectedAbility = this.commandProcessor.ProcessCommand(selectAbilityCommand);
                
                if (selectedAbility.OnCD == true)
                {
                    this.data.Log.AppendLine("Chosen ability is on cooldown, choose another");
                    this.writer.PrintOnPosition(Constants.LOG_ROW_POS, Constants.LOG_COL_POS, this.data.Log.ToString());
                    Console.SetCursorPosition(2, 9);
                    while (selectedAbility.OnCD == true)
                    {
                        selectAbilityCommand = this.reader.ConsoleReadKey();
                        this.writer.ConsoleClear();
                        this.writer.WriteLine("I told you to choose other option!!! Try again. I will be wathcing you!");
                        selectedAbility = this.commandProcessor.ProcessCommand(selectAbilityCommand);
                    }
                }

                turn.ActiveHero.UseAbility(selectedAbility);

                this.data.Log.AppendLine($"{turn.ActiveHero.Name} uses {selectedAbility.Name} and {selectedAbility.ToString()}.");
            }

            CheckIfGameIsOver();

        }

        private void UpdataLog()
        {
            this.writer.ConsoleClear();
            this.writer.PrintOnPosition(Constants.LOG_ROW_POS - 1, Constants.LOG_COL_POS, new String('-', Console.WindowWidth));
            this.writer.PrintOnPosition(Constants.LOG_ROW_POS, Constants.LOG_COL_POS, this.data.Log.ToString());

            this.writer.PrintOnPosition(0, 0, $"{this.terrainManager.Terrain.GetType().Name} set as terrain");
            this.writer.PrintOnPosition(0, 150, $" Turn: {turn.TurnNumber}", ConsoleColor.Red);
            this.writer.WriteLine(new String('-', Console.WindowWidth));
        }
        private void CheckIfGameIsOver()
        {
            if (turn.ActiveHero.Opponent.HealthPoints < 0)
            {
                this.writer.ConsoleClear();
                Console.Beep();
                this.writer.PrintOnPosition(0, 0, $"{turn.ActiveHero.Name.ToUpper()} WON!", ConsoleColor.Green);
                Thread.Sleep(5000);
                Console.Beep();
                this.endGame = true;
            }
        }
    }
}