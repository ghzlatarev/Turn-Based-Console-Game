﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Team8Project.Common;
using Team8Project.Contracts;
using Team8Project.Core.Providers;
using Team8Project.IO;
using Team8Project.Models;
using Team8Project.Models.Terrains;

namespace Team8Project.Core
{
    public class GameEngine : IEngine
    {
        private const string INITIAL_MESSAGE = "Choose a character:\n 1.{0}\n 2.{1}\n 3.{2}\n 4.{3}";



        private static GameEngine instance;
        private readonly Factory factory;
        private TurnProcessor turn;
        private EffectManager effect;
        private ITerrain terrain;
        private CommandProcessor commandProcessor;
        private List<IHero> listHeros;

        private GameEngine()
        {
            this.factory = Factory.Instance;
            this.turn = TurnProcessor.Instance;
            this.effect = EffectManager.Instance;
            this.Reader = new ConsoleReader();
            this.Writer = new ConsoleWriter();
            this.commandProcessor = new CommandProcessor();
            this.listHeros = new List<IHero>();
        }

        public IReader Reader { get; set; }
        public IWriter Writer { get; set; }


        public void Run()
        {
            //Set Current Game Heroes
            Console.SetWindowSize(160, 40);

            this.Writer.ConsoleWriteLine(string.Format(INITIAL_MESSAGE, HeroClass.Warrior, HeroClass.Mage, HeroClass.Assasin, HeroClass.Cleric));
            this.Writer.ConsoleWriteLine(new String('-', Console.WindowWidth));
            string[] players = new string[2];
            this.Writer.ConsoleWrite("Player 1: ");
            players[0] = this.Reader.ConsoleReadLine();
            this.Writer.ConsoleWrite("Player 2: ");
            players[1] = this.Reader.ConsoleReadLine();
            this.Writer.ConsoleClear();

            this.listHeros = commandProcessor.ProcessCommand(players);
            turn.FirstHero = this.listHeros[0];
            turn.SecondHero = this.listHeros[1];

            turn.SetFirstTurnActiveHero();
            factory.CreateSpellBook(turn.FirstHero);
            factory.CreateSpellBook(turn.SecondHero);

            //choose terrain depending on user choice [0] [1] etc
            //if(userChoice == 0)
            //Get the only object available
            this.terrain = Jungle.Instance;
            //apply effect
            terrain.HeroEffect(turn.ActiveHero);
            terrain.HeroEffect(turn.ActiveHero.Opponent);
            Console.WriteLine("Initial terrain effects applied to both heroes");


            //START GAME
            while (true)
            {
                if (RandomProvider.Generate(1, 2) == 1)
                {
                    terrain.ContinuousEffect(turn.ActiveHero);
                    //TODO: needs message
                }
                else
                {
                    terrain.ContinuousEffect(turn.ActiveHero.Opponent);
                    //TODO: needs message
                }



                Act(turn.ActiveHero); //first hero move
                turn.EndAct();
                Writer.ConsoleWriteLine("------------------------------------------------------------------");
                Act(turn.ActiveHero); //second hero move
                turn.EndAct();
                Writer.ConsoleWriteLine("------------------------------------------------------------------");

                turn.NextTurn();
                Writer.ConsoleWriteLine("**********************************************************************");

                if (turn.TurnNumeber % 3 == 0)
                {
                    terrain.IsDay = (terrain.IsDay) ? false : true;
                    Console.WriteLine((terrain.IsDay) ? "Day has come" : "Night has come");
                }
            }
        }

        private void Act(IHero activeHero)
        {
            if ((turn.ActiveHero.AppliedEffects.Any(x => x.Type == EffectType.Incapacitated)))
            {
                Writer.ConsoleWriteLine($"{turn.ActiveHero.HeroClass} {turn.ActiveHero.Name} is incapacitated!. Cannot act!");

                turn.ActiveHero.AppliedEffects.Remove(turn.ActiveHero.AppliedEffects.First(x => x.Type == EffectType.Incapacitated));
            }
            else
            {
                //Console.WriteLine($" Turn: {turn.TurnNumeber}. {turn.ActiveHero.HeroClass.ToString()} { turn.ActiveHero.Name} is active. HP: {turn.ActiveHero.HealthPoints}");
                Console.WriteLine($" Turn: {turn.TurnNumeber}");


                effect.AtTurnStart(turn.ActiveHero);
                activeHero.AppliedEffects = activeHero.AppliedEffects.Where(e => e.CurrentStacks != 0).ToList();

                Console.WriteLine($"{turn.ActiveHero.HeroClass.ToString()} { turn.ActiveHero.Name} is active. HP: {turn.ActiveHero.HealthPoints}");

                if (turn.ActiveHero.AppliedEffects.Count == 0)
                {
                    Console.WriteLine("Applied effects: No effects.");
                }
                else
                {
                    Console.WriteLine($"Applied effects: {string.Join(", ", turn.ActiveHero.AppliedEffects)}");
                }

                Console.WriteLine($"{turn.ActiveHero.Name}'s abilities: ");

                int pos = 0;
                foreach (var ability in turn.ActiveHero.Abilities)
                {
                    pos++;
                    Writer.ConsoleWriteLine($"{pos}. {ability.Print()}");
                }

                string selectAbilityCommand = this.Reader.ConsoleReadKey();
                this.Writer.ConsoleWriteLine("");
                var selectedAbility = commandProcessor.ProcessCommand(selectAbilityCommand);

                while (selectedAbility.OnCD == true)
                {
                    Console.WriteLine("Chosen ability is on cooldown, choose another");
                    selectAbilityCommand = this.Reader.ConsoleReadKey();
                    selectedAbility = commandProcessor.ProcessCommand(selectAbilityCommand);
                }

                turn.ActiveHero.UseAbility(selectedAbility);
                Console.WriteLine($"{turn.ActiveHero.Name} uses {selectedAbility.Name} and {selectedAbility.ToString()}.");

                if (turn.ActiveHero.Opponent.HealthPoints < 0)
                {
                    Console.WriteLine($"{turn.ActiveHero.Name.ToUpper()} WON! ");
                    return;
                }
            }
        }

        public static GameEngine Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameEngine();
                }

                return instance;
            }
        }
    }
}
