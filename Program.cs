﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using ArmelloLogTools.Armello;
using MoreLinq.Extensions;
using Timer = System.Timers.Timer;

namespace ArmelloLogTools
{
    class Program
    {



        private static LogReader _reader;
        private static Ui _ui;
        private static Interpreter _interpreter;


        private static IEnumerable<Event> FindLastGame(IList<Event> events)
        {
            for (var i = events.Count - 1; i >= 0; i--)
            {
                if (events[i].Type == EventType.LoadGame)
                {
                    return events.Skip(i);
                }
            }

            throw new Exception("Cannot find a game");
        }

        private static void Main(string[] args)
        {
            var logFile = args.Length > 0 ? args[0] : LogFile.LatestLogFile();

            Console.WriteLine($"Using log file : {logFile}");

            _reader = new LogReader(logFile);
            _interpreter = new Interpreter();
            _ui = new Ui();
            
            UpdateEventsAndUi(true);

            using (var timer = new Timer(5000))
            {
                timer.AutoReset = true;

                timer.Elapsed += (sender, eventArgs) => { UpdateEventsAndUi(); };

                timer.Start();

                ExitLoop();
            }
        }

        private static void UpdateEventsAndUi(bool lastGame = false)
        {
            var events = Parser.ParseLines(_reader.ReadLines());

            if (lastGame)
            {
                events = FindLastGame(events).ToList();
            }

            _interpreter.ProcessEvents(events);
            _ui.Update(_interpreter.Players.Select(pair => pair.Value));
        }

        private static void ExitLoop()
        {
            Console.WriteLine("Press escape to quit");
            while (true)
            {
                Thread.Sleep(1);

                if (!Console.KeyAvailable) continue;

                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape) break;
            }
        }
    }
}