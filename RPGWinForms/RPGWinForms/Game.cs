using System;
using System.Collections.Generic;
using System.Diagnostics;
using RPGWinForms;

namespace RPGv2
{
    internal class Game
    {
        public static void StartGame()
        {
            int yearInput;
            do { Console.Write("Enter years of history: "); } while (!int.TryParse(Console.ReadLine(), out yearInput));
            Stopwatch sw = new Stopwatch();
            sw.Start();
            History h = StartHistory(yearInput);
            Console.Clear();
            Console.WriteLine("Elapsed time: {0}", sw.Elapsed.Milliseconds / 1000.0);
            bool done = false;
            while (!done)
            {
                Console.Write(">");
                string[] inp = Console.ReadLine().Split(' ');
                string input = "";
                for (int i = 1; i < inp.Length; i++)
                {
                    if (i != inp.Length - 1)
                        input += inp[i] + " ";
                    else
                        input += inp[i];
                }
                Console.WriteLine(input);
                switch (inp[0])
                {
                    case "map":
                        h.Map.OutputMap();
                        break;
                    case "get":
                        foreach (Faction fac in h.Factions)
                            if (fac.Name == input)
                            {
                                Console.WriteLine(fac.ToString());
                            }
                        break;
                    case "hist":
                        foreach (Faction fac in h.Factions)
                            if (fac.Name == input)
                            {
                                foreach (HistoricalEvent he in fac.HistoricalEvents.ToArray())
                                    Console.WriteLine(he.ToString());
                            }
                        break;
                    case "list":
                        foreach (Faction f in h.Factions)
                            Console.WriteLine(f.ToString());
                        break;
                    case "clear":
                        Console.Clear();
                        break;
                    case "restart":
                        StartGame();
                        break;
                    default:
                        done = true;
                        break;
                }
            }
            Console.ReadKey();
        }
        public static History StartHistory(int years)
        {
            const bool DISPLAY = true;
            EventList el = new EventList();
            History h = new History();
            h.Map = new Map(@"Dependencies\Maps\NewMap.png");

            Console.Clear();
            if (DISPLAY)
            {
                Console.Clear();
                Console.WriteLine("Year: ");
                Console.WriteLine("Event: ");
            }
            List<Race> races = new List<Race>();
            List<Faction> factions = new List<Faction>();
            for (int i = 0; i < Race.RacesAmount(); i++)
            {
                races.Add(new Race(i, new Random().Next(30000)));
            }
            
            Random rand = new Random();
            for (int i = 0; i < races.Count; i++)
            {
                Race race = races[i];
                int[] vals = race.GetVals();
                factions.Add(new Faction(races[i], "Main City: " + race.Name));
                int mainCityInd = factions.Count - 1;
                for (int j = 0; j <= vals[3]; j++)
                {
                    int num = rand.Next(100);
                    if (num < 80)
                    {
                        factions[mainCityInd].Pop++;
                        num = 101;
                    }
                    if (num < 95)
                    {
                        bool done = false;
                        Random rand2 = new Random();
                        while (!done)
                        {
                            int num2 = rand2.Next(factions.Count);
                            Faction f = factions[num2];
                            if (f.Race == race)
                            {
                                done = true;
                                factions[num2].Pop++;
                            }
                        }
                        num = 101;
                    }
                    if (num < 100)
                    {
                        bool exists = false;
                        Faction f = new Faction(race);
                        for (int k = 0; k < factions.Count; k++)
                        {
                            if (factions[k].Name == f.Name)
                            {
                                exists = true;
                                factions[k].Pop++;
                            }
                        }
                        if (!exists)
                        {
                            factions.Add(new Faction(race));
                            factions[factions.Count - 1].Pop++;
                        }
                    }
                }
            }
            
            int totalPeople = 0;
            double averagePopSeverity = 0;
            while (factions.Count >= h.Map.GetLandCount())
            {
                double avePop = 0;
                foreach (Faction f in factions.ToArray())
                {
                    avePop += f.Pop;
                }
                avePop /= factions.Count;
                foreach (Faction f in factions.ToArray())
                {
                    if (f.Pop < avePop - avePop / 2)
                        factions.Remove(f);
                }
            }
            foreach(Faction fac in factions.ToArray())
            {
                h.Map.InitFaction(fac);
            }
            for (int i = 0; i <= years; i++)
            {
                if (DISPLAY)
                {
                    Console.SetCursorPosition(6, 0);
                    Console.Write("                ");
                    Console.SetCursorPosition(6, 0);
                    Console.Write(i);
                }
                totalPeople = 0;
                averagePopSeverity = 0;
                foreach (Faction f in factions)
                    totalPeople += f.Pop;
                foreach (Faction f in factions)
                {
                    f.PopSeverity = (double)f.Pop / totalPeople;
                    averagePopSeverity += f.PopSeverity;
                }
                averagePopSeverity /= factions.Count;
                #region events
                int chainAmount = 0;
                for (int j = 0; j < factions.Count; j++)
                {
                    Faction f = factions[j];
                    do
                    {
                        Event newEvent = new Event(el);
                        EventVar e = newEvent.Chosen;
                        if (DISPLAY)
                        {
                            Console.SetCursorPosition(7, 1);
                            Console.Write("                ");
                            Console.SetCursorPosition(7, 1);
                            Console.Write(e.Name);
                        }
                        switch (e.Name)
                        {
                            #region none
                            case "None":
                                break;
                            #endregion
                            #region chain event
                            case "Chain Event":
                                chainAmount += 3;
                                break;
                            #endregion
                            #region famine
                            case "Famine":
                                int deathChance = new Random().Next(70);
                                Random rando = new Random();
                                f.Pop -= Convert.ToInt32(f.Pop * (deathChance / 100.0));
                                if (deathChance < 10)
                                {
                                    f.HistoricalEvents.Add(new HistoricalEvent("slight famine", i));
                                    break;
                                }
                                if (deathChance < 30)
                                {
                                    f.HistoricalEvents.Add(new HistoricalEvent("mild famine", i));
                                    break;
                                }
                                if (deathChance < 70)
                                {
                                    f.HistoricalEvents.Add(new HistoricalEvent("severe famine", i));
                                    break;
                                }
                                if (deathChance < 100)
                                {
                                    f.HistoricalEvents.Add(new HistoricalEvent("extreme famine", i));
                                    break;
                                }
                                break;
                            #endregion
                            #region popup
                            case "Population Up":
                                int percentUp = new Random().Next(1, 20);
                                f.Pop += Convert.ToInt32(f.Pop * (percentUp / 100.0));
                                break;
                            #endregion
                            #region popdown
                            case "Population Down":
                                int percentDown = new Random().Next(1, 10);
                                f.Pop += Convert.ToInt32(f.Pop * (percentDown / 100.0));
                                break;
                            #endregion
                            case "War Declaration":
                                bool canFind = false;
                                for (int k = 0; k < factions.Count; k++)
                                {
                                    if (factions[k].Race != f.Race && factions[k] != f && f.Pop > factions[k].Pop / 2 && f.Pop < factions[k].Pop * 2)
                                    {
                                        canFind = true;
                                    }
                                }
                                if (!canFind)
                                    break;
                                bool doneFinding = false;
                                Faction opp = new Faction(new Race(0, 0));
                                while (!doneFinding)
                                {
                                    int num = rand.Next(factions.Count);
                                    opp = factions[num];
                                    if (opp.Race != f.Race && opp != f && f.Pop > opp.Pop / 2 && f.Pop < opp.Pop * 2)
                                        doneFinding = true;
                                }
                                f.Wars.Add(new War(i, f, opp));
                                break;
                            case "Discovery":
                                break;
                            case "New Faction":
                                int breakOff = HelperClasses.RandomNumber(1, f.Pop);
                                Faction newFaction = new Faction(f.Race);
                                newFaction.Pop += breakOff;
                                f.Pop -= breakOff;
                                factions.Add(newFaction);
                                newFaction.HistoricalEvents.Add(new HistoricalEvent("Broke off from " + f.Name, i));
                                h.Map.InitFaction(newFaction);
                                if (DISPLAY)
                                {
                                    Console.SetCursorPosition(0, 3);
                                    Console.Write("                                                                      ");
                                    Console.SetCursorPosition(0, 3);
                                    Console.Write("{0} has been created!", newFaction.Name);
                                }
                                break;
                            #region default
                            default:
                                Console.Clear();
                                Console.WriteLine("An unknown event has occured, event name: {0}", e.Name);
                                Console.ReadKey();
                                break;
                                #endregion
                        }
                        chainAmount--;
                    } while (chainAmount > 0);
                    #region pophandling
                    int popNum = Convert.ToInt32(f.PopSeverity * 100000);
                    if (popNum > 0)
                    {
                        int avePopNum = Convert.ToInt32(averagePopSeverity * 100000);
                        int popRand = HelperClasses.RandomNumber(0, popNum);
                        int avePopRand = HelperClasses.RandomNumber(0, avePopNum);
                        if (popRand > avePopNum)
                        {
                            foreach (EventVar ev in el.Events)
                            {
                                ev.Chance += ev.Rate;
                                ev.ChanceCheck();
                            }
                        }
                        else
                            foreach (EventVar ev in el.Events)
                                ev.Chance = ev.DefChance;
                    }
                    #endregion

                }
                #endregion
                #region warhandling
                for (int j = 0; j < factions.Count; j++)
                {
                    Faction f = factions[j];
                    bool inWar = false;
                    List<War> wars = new List<War>();
                    foreach (War w in f.Wars)
                        if (w.OnGoing)
                        {
                            inWar = true;
                            wars.Add(w);
                        }
                    if (inWar)
                    {
                        for (int k = 0; k < wars.Count; k++)
                        {
                            Faction warWith = wars[k].Side2;
                            WarEvent we = new WarEvent();
                            int num1;
                            int num2;
                            switch (we.Name)
                            {
                                case "None":
                                    wars[k].Length++;
                                    break;
                                case "Attack":
                                    for (int l = 0; l < f.Pop + warWith.Pop; l++)
                                    {
                                        num1 = HelperClasses.RandomNumber(0, f.Race.GetVals()[2] + (f.Race.GetVals()[1] / 2) + f.Race.GetVals()[0]);
                                        num2 = HelperClasses.RandomNumber(0, warWith.Race.GetVals()[1] + (warWith.Race.GetVals()[2] / 2) + warWith.Race.GetVals()[0]);
                                        if (num1 >= num2)
                                            warWith.Pop--;
                                        else
                                            f.Pop--;
                                    }
                                    wars[k].Length++;
                                    break;
                                case "Defend":
                                    for (int l = 0; l < f.Pop + warWith.Pop; l++)
                                    {
                                        num1 = HelperClasses.RandomNumber(0, warWith.Race.GetVals()[1] + (warWith.Race.GetVals()[2] / 2) + warWith.Race.GetVals()[0]);
                                        num2 = HelperClasses.RandomNumber(0, f.Race.GetVals()[2] + (f.Race.GetVals()[1] / 2) + f.Race.GetVals()[0]);
                                        if (num1 >= num2)
                                            f.Pop--;
                                        else
                                            warWith.Pop--;
                                    }
                                    wars[k].Length++;
                                    break;
                                case "End War":
                                    wars[k].OnGoing = false;
                                    f.HistoricalEvents.Add(new HistoricalEvent(string.Format("At war with {0} for {1} years", wars[k].Side2, wars[k].Length), i));
                                    break;
                                default:
                                    Console.Clear();
                                    Console.WriteLine("An unknown event has occured, name: {0}", we.Name);
                                    Console.ReadKey();
                                    break;
                            }
                        }
                    }
                }
                if (i == years - 1)
                {
                    foreach (Faction fac in factions.ToArray())
                    {
                        List<War> wars = new List<War>(fac.Wars);
                        if (wars.Count != 0)
                        {
                            foreach (War w in wars.ToArray())
                            {
                                if (!w.OnGoing)
                                    wars.Remove(w);
                            }
                            foreach(War w in wars)
                            {
                                fac.HistoricalEvents.Add(new HistoricalEvent(String.Format("At war with {0} since {1}", w.Side2, w.StartYear), w.StartYear));
                            }
                        }
                    }
                }
                bool exists;
                foreach (Tile t in h.Map.Tiles)
                {
                    exists = false;
                    foreach(Faction fac in factions)
                    {
                        if (fac == t.Occ || t.Type == "W")
                            exists = true;
                    }
                    if(!exists)
                    {
                        t.Occ = null;
                        t.Type = "L";
                    }
                }
                for (int i1 = 0; i1 < factions.Count; i1++)
                {
                    Faction f = factions[i1];
                    if (f.Pop <= 0)
                    {
                        factions.Remove(f);
                        if (DISPLAY)
                        {
                            Console.SetCursorPosition(0, 2);
                            Console.Write("                                                                      ");
                            Console.SetCursorPosition(0, 2);
                            Console.Write("{0} has fallen!", f.Name);
                        }
                    }
                }
                #endregion
                HelperClasses.form.LoadMap(h.Map);
            }

            h.Races = races;
            h.Factions = factions;
            return h;
        }
    }
}
