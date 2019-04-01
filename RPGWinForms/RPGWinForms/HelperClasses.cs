using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.Windows.Forms;
using RPGWinForms;
using System.Threading.Tasks;

namespace RPGv2
{
    internal class HelperClasses
    {
        public static Form1 form = new Form1();

        [STAThread]
        public static void MainProgram()
        {
            Task.Run(() => Application.Run(form));
            StateManager sm = new StateManager();
            bool done = false;
            while (!done)
            {
                switch (sm.GetState())
                {
                    case "start":
                        if (Start(done))
                            done = true;
                        break;
                    default:
                        Console.Clear();
                        Console.WriteLine("A state error has occured. State: {0}", sm.GetState());
                        break;
                }
            }
        }

        private static readonly Random random = new Random();
        private static readonly object syncLock = new object();

        public static int RandomNumber(int min, int max)
        {
            lock (syncLock)
            { // synchronize
                return random.Next(min, max);
            }
        }
        public static bool Start(bool done)
        {
            Console.Clear();
            int inp = MultipleChoice(false, "Start", "Modify JSON files", "Exit");
            switch (inp)
            {
                case 0:
                    Game.StartGame();
                    return false;
                case 1:
                    string[] jsonFiles = Directory.GetFiles("Dependencies");
                    for (int i = 0; i < jsonFiles.Length; i++)
                        jsonFiles[i] = jsonFiles[i].Remove(0, 13);
                    inp = MultipleChoice(false, jsonFiles);
                    JArray arr = JArray.Parse(File.ReadAllText(string.Format(@"Dependencies\{0}", jsonFiles[inp])));
                    int inpIndex = inp;
                    string path = string.Format(@"Dependencies\{0}", jsonFiles[inp]);
                    inp = MultipleChoice(false, "Add", "Modify");
                    List<string> options = new List<string>();
                    JObject o;
                    string newVal;
                    switch (inp)
                    {
                        case 0:
                            options.Clear();
                            o = JObject.Parse(arr[0].ToString());
                            foreach (JProperty jp in o.Properties())
                                options.Add(jp.Name);
                            for (int i = 0; i < options.Count; i++)
                            {
                                Console.Write("Value of {0}: ", options[i]);
                                newVal = Console.ReadLine();
                                if (o[options[i]].Type == JTokenType.String)
                                    o[options[i]] = newVal;
                                if (o[options[i]].Type == JTokenType.Integer)
                                    o[options[i]] = int.Parse(newVal);
                                if (o[options[i]].Type == JTokenType.Float)
                                    o[options[i]] = float.Parse(newVal);
                                if (o[options[i]].Type == JTokenType.Boolean)
                                    o[options[i]] = bool.Parse(newVal);
                            }
                            arr.Add(o);
                            File.WriteAllText(path, arr.ToString());
                            break;
                        case 1:
                            foreach (JObject obj in arr)
                                options.Add((string)obj["Name"]);
                            inp = MultipleChoice(false, options.ToArray());
                            options.Clear();
                            o = JObject.Parse(arr[inp].ToString());
                            foreach (JProperty jp in o.Properties())
                                options.Add(jp.Name);
                            List<string> optionsTemp = new List<string>(options);
                            for (int i = 0; i < optionsTemp.Count; i++)
                                optionsTemp[i] = string.Format("{0} ({1})", optionsTemp[i], o[optionsTemp[i]]);
                            inp = MultipleChoice(false, optionsTemp.ToArray());
                            Console.Write("Enter new value: ");
                            newVal = Console.ReadLine();
                            if (o[options[inp]].Type == JTokenType.String)
                                o[options[inp]] = newVal;
                            if (o[options[inp]].Type == JTokenType.Integer)
                                o[options[inp]] = int.Parse(newVal);
                            if (o[options[inp]].Type == JTokenType.Float)
                                o[options[inp]] = float.Parse(newVal);
                            arr[inpIndex] = o;
                            File.WriteAllText(path, arr.ToString());
                            break;
                        default:
                            break;
                    }
                    return false;
                case 2:
                    return true;
                default:
                    break;
            }
            return false;

        }

        public static int MultipleChoice(bool canCancel, params string[] options)
        {
            const int startX = 0;
            const int startY = 0;
            const int optionsPerLine = 1;
            const int spacingPerLine = 14;

            int currentSelection = 0;

            ConsoleKey key;

            Console.CursorVisible = false;
            do
            {
                Console.Clear();

                for (int i = 0; i < options.Length; i++)
                {
                    Console.SetCursorPosition(startX + (i % optionsPerLine) * spacingPerLine, startY + i / optionsPerLine);

                    if (i == currentSelection)
                        Console.ForegroundColor = ConsoleColor.Red;

                    Console.Write(options[i]);

                    Console.ResetColor();
                }

                key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.LeftArrow:
                        {
                            if (currentSelection % optionsPerLine > 0)
                                currentSelection--;
                            break;
                        }
                    case ConsoleKey.RightArrow:
                        {
                            if (currentSelection % optionsPerLine < optionsPerLine - 1)
                                currentSelection++;
                            break;
                        }
                    case ConsoleKey.UpArrow:
                        {
                            if (currentSelection >= optionsPerLine)
                                currentSelection -= optionsPerLine;
                            break;
                        }
                    case ConsoleKey.DownArrow:
                        {
                            if (currentSelection + optionsPerLine < options.Length)
                                currentSelection += optionsPerLine;
                            break;
                        }
                    case ConsoleKey.Escape:
                        {
                            if (canCancel)
                                return -1;
                            break;
                        }
                }
            } while (key != ConsoleKey.Enter);

            Console.CursorVisible = true;

            Console.Clear();
            return currentSelection;
        }
    }

    public class Map
    {
        Image img;
        List<Tile> tiles = new List<Tile>();

        internal List<Tile> Tiles { get => tiles; set => tiles = value; }
        public Image Img { get => img; set => img = value; }

        public int GetLandCount()
        {
            int count = 0;
            foreach (Tile t in Tiles.ToArray())
                if (t.Type == "L")
                    count++;
            return count;
        }

        public Map()
        {
            string[] mapFiles = Directory.GetFiles("Dependencies\\Maps");
            string mapFile = mapFiles[new Random().Next(mapFiles.Length)];
            Img = Image.FromFile(mapFile);
        }

        public Map(int maxX, int maxY)
        {
            string[] mapFiles = Directory.GetFiles("Dependencies\\Maps");
            string mapFile = mapFiles[new Random().Next(mapFiles.Length)];
            Img = Image.FromFile(mapFile);
        }

        public Map(string s)
        {
            Img = Image.FromFile(s);
            Bitmap bmp = new Bitmap(Img);
            Console.Clear();
            Console.WriteLine("Loading Map...");
            Console.ReadKey();
            for (int y = 0; y <= Img.Height - 1; y++)
                for (int x = 0; x <= Img.Width - 1; x++)
                {
                    Color clr = bmp.GetPixel(x, y);
                    int g = clr.G;
                    int b = clr.B;
                    if (g > b)
                    {
                        Tiles.Add(new Tile(x, y, "L"));
                    }
                    else
                    {
                        Tiles.Add(new Tile(x, y, "W"));
                    }
                }
        }

        public List<Tile> GetTiles() => Tiles;

        public void OutputMap()
        {
            int currY = 0;
            Tile currTile;
            for (int i = 0; i < Tiles.Count; i++)
            {
                currTile = Tiles[i];
                Console.Write(currTile.Type);
                if (currTile.Y != currY && currTile.X == Img.Width - 1)
                {
                    Console.WriteLine();
                    currY = currTile.Y;
                }
            }
        }

        public Tile FindTile(int x, int y)
        {
            foreach (Tile t in Tiles)
            {
                if (t.X == x && t.Y == y)
                    return t;
            }
            return Tiles[0];
        }

        public void InitFaction(Faction fac)
        {
            Tile t;
            do
            {
                t = Tiles[HelperClasses.RandomNumber(0, Tiles.Count - 1)];
            } while (t.Occ != null || t.Type == "W");
            fac.Loc.X = t.X;
            fac.Loc.Y = t.Y;
            t.Type = "F";
            t.Occ = fac;
            Bitmap bmp = new Bitmap(Img);
            bmp.SetPixel(fac.Loc.X, fac.Loc.Y, Color.Red);
            Img = bmp;
        }
    }

    public class Tile
    {
        int x;
        int y;
        string type;
        Faction occ;

        public Tile()
        {

        }

        public Tile(int xCoord, int yCoord, string t)
        {
            X = xCoord;
            Y = yCoord;
            Type = t;
        }

        public override string ToString()
        {
            return String.Format("X: {0}\nY: {1}\nType: {2}", X, Y, Type);
        }

        public int X { get => x; set => x = value; }
        public int Y { get => y; set => y = value; }
        public string Type { get => type; set => type = value; }
        public Faction Occ { get => occ; set => occ = value; }
    }

    internal class StateManager
    {
        private string state = "start";
        public StateManager()
        {

        }
        public string GetState()
        {
            return state;
        }

        public void SetState(string s) { state = s; }
    }

    public class Race
    {
        string name;
        int intelligence;
        int baseAtt;
        int baseDef;
        int pop;

        public string Name { get => name; set => name = value; }

        public Race(int index, int p)
        {
            pop = p;
            JArray array = JArray.Parse(File.ReadAllText(@"Dependencies\race.json"));
            JObject obj = JObject.Parse(array[index].ToString());
            Name = (string)obj["Name"];
            intelligence = (int)obj["Intelligence"];
            baseAtt = (int)obj["BaseAttack"];
            baseDef = (int)obj["BaseDefense"];
        }
        public Race(string n, int p)
        {
            pop = p;
            JArray array = JArray.Parse(File.ReadAllText(@"Dependencies\race.json"));
            foreach (JObject obj in array)
            {
                if (obj["Name"].ToString() == n)
                {
                    Name = (string)obj["Name"];
                    intelligence = (int)obj["Intelligence"];
                    baseAtt = (int)obj["BaseAttack"];
                    baseDef = (int)obj["BaseDefense"];
                }
            }
        }
        public int[] GetVals() => new int[] { intelligence, baseAtt, baseDef, pop };
        public static int RacesAmount() => JArray.Parse(File.ReadAllText(@"Dependencies\race.json")).Count;
    }

    class War
    {
        private int length = 0;
        private string name;
        private int startYear;
        private Faction side1;
        private Faction side2;
        private bool onGoing = true;



        public War(int year, Faction s1, Faction s2)
        {
            string[] warNames = File.ReadAllLines(@"Dependencies\WarNames.txt");
            Name = warNames[new Random().Next(warNames.Length)];
            StartYear = year;
            Side1 = s1;
            Side2 = s2;
        }

        public int Length { get => length; set => length = value; }
        public string Name { get => name; set => name = value; }
        public int StartYear { get => startYear; set => startYear = value; }
        public bool OnGoing { get => onGoing; set => onGoing = value; }
        internal Faction Side1 { get => side1; set => side1 = value; }
        internal Faction Side2 { get => side2; set => side2 = value; }
    }

    class EventVar
    {
        string name;
        string desc;
        int chance;
        int defChance;
        int rate;

        public EventVar(string n, string d, int c, int r)
        {
            Name = n;
            Desc = d;
            Chance = c;
            DefChance = c;
            Rate = r;
        }

        public void ChanceCheck()
        {
            if (Chance <= 0)
                Chance = DefChance;
        }

        public string Name { get => name; set => name = value; }
        public string Desc { get => desc; set => desc = value; }
        public int Chance { get => chance; set => chance = value; }
        public int Rate { get => rate; set => rate = value; }
        public int DefChance { get => defChance; set => defChance = value; }
    }

    class EventList
    {
        List<EventVar> events = new List<EventVar>();

        public EventList()
        {
            JArray eventArray = JArray.Parse(File.ReadAllText(@"Dependencies\events.json"));
            foreach (JObject o in eventArray)
            {
                events.Add(new EventVar((string)o["Name"], (string)o["Desc"], (int)o["Chance"], (int)o["Rate"]));
            }
        }

        internal List<EventVar> Events { get => events; set => events = value; }

        public void ChangeChance(string e, int val)
        {
            foreach (EventVar v in Events)
            {
                if (v.Name == e)
                {
                    v.Chance = val;
                    return;
                }
            }

        }
    }

    class Event
    {
        EventVar chosen;

        public Event(EventList el)
        {
            int chanceTotal = 0;
            List<EventVar> events = el.Events;
            foreach (EventVar ev in events)
                chanceTotal += ev.Chance;
            int num = HelperClasses.RandomNumber(0, chanceTotal);
            List<int> minMax = new List<int>();
            minMax.Add(0);
            for (int i = 1, j = 0; j < events.Count; i++)
            {
                if (i % 2 == 1)
                {
                    minMax.Add(events[j].Chance + minMax[i - 1] - 1);
                    j++;
                }
                else
                    minMax.Add(minMax[i - 1] + 1);
            }
            for (int i = 0; i < minMax.Count - 1; i++)
            {
                if (num >= minMax[i] && num <= minMax[i + 1])
                {
                    Chosen = events[i / 2];
                }
            }

        }

        internal EventVar Chosen { get => chosen; set => chosen = value; }
    }

    class WarEvent
    {
        string name;
        string desc;


        public WarEvent()
        {
            JArray events = JArray.Parse(File.ReadAllText(@"Dependencies\warevents.json"));
            int chanceTotal = 0;
            foreach (JObject o in events)
                chanceTotal += (int)o["Chance"];
            int num = HelperClasses.RandomNumber(0, chanceTotal);
            List<int> minMax = new List<int>();
            minMax.Add(0);
            for (int i = 1, j = 0; j < events.Count; i++)
            {
                if (i % 2 == 1)
                {
                    minMax.Add((int)events[j]["Chance"] + minMax[i - 1] - 1);
                    j++;
                }
                else
                    minMax.Add(minMax[i - 1] + 1);
            }
            for (int i = 0; i < minMax.Count - 1; i++)
            {
                if (num >= minMax[i] && num <= minMax[i + 1])
                {
                    JObject eventObj = JObject.Parse(events[i / 2].ToString());
                    Name = (string)eventObj["Name"];
                    Desc = (string)eventObj["Desc"];
                }
            }

        }



        public string Name { get => name; set => name = value; }
        public string Desc { get => desc; set => desc = value; }
    }
    class HistoricalEvent
    {
        string name;
        int year;

        public HistoricalEvent(string n, int y)
        {
            Name = n;
            Year = y;
        }

        public override string ToString()
        {
            return String.Format("{0}\nYear: {1}", name, year);
        }

        public string Name { get => name; set => name = value; }
        public int Year { get => year; set => year = value; }
    }
    class Point
    {
        int x;
        int y;
        string type;

        public Point(int xCoord, int yCoord)
        {
            X = xCoord;
            Y = yCoord;
        }

        public double Distance(Point p) => Math.Sqrt(Math.Pow(p.X - X, 2) + Math.Pow(p.Y - Y, 2));

        public int X { get => x; set => x = value; }
        public int Y { get => y; set => y = value; }
        public string Type { get => type; set => type = value; }
    }

    public class Faction
    {
        string name;
        Race race;
        int pop = 0;
        double popSeverity;
        List<string> advances = new List<string>();
        List<War> wars = new List<War>();
        List<HistoricalEvent> historicalEvents = new List<HistoricalEvent>();
        Point loc = new Point(0, 0);

        public int Pop { get => pop; set => pop = value; }
        internal Race Race { get => race; set => race = value; }
        public string Name { get => name; set => name = value; }
        internal List<HistoricalEvent> HistoricalEvents { get => historicalEvents; set => historicalEvents = value; }
        internal List<War> Wars { get => wars; set => wars = value; }
        public List<string> Advances { get => advances; set => advances = value; }
        public double PopSeverity { get => popSeverity; set => popSeverity = value; }
        internal Point Loc { get => loc; set => loc = value; }

        public Faction(Race r, string n)
        {
            Name = n;
            Race = r;
        }

        public Faction(Race r)
        {
            string[] factionNames = File.ReadAllLines(@"Dependencies\FactionNames.txt");
            Name = factionNames[new Random().Next(factionNames.Length)];
            Race = r;
        }

        public void AddPop(int add) { Pop += add; }

        public override string ToString()
        {
            return String.Format("Name: {0}\nRace: {1}\nPop: {2}\nSeverity: {3}", name, race.Name, pop, popSeverity);
        }
    }

    class History
    {
        List<Race> races = new List<Race>();
        List<Faction> factions = new List<Faction>();
        Map map;

        internal List<Race> Races { get => races; set => races = value; }
        internal List<Faction> Factions { get => factions; set => factions = value; }
        internal Map Map { get => map; set => map = value; }
    }

    internal class Player
    {
        private string name;
        private string cla;
        private string race;
        private string faction;
        private int att;
        private int matk;
        private int def;
        private int mdef;
        private int intel;
        private double money;
        private double luck;
        private double eva;
        public Player(int slot)
        {
            JArray saves = JArray.Parse(File.ReadAllText(@"Dependencies\player.json"));
            JObject save = JObject.Parse(saves[slot - 1].ToString());
            if (string.IsNullOrEmpty(save["Name"].ToString()))
            {
                CreateCharacter(slot, saves);
            }
        }

        public string Name { get => name; set => name = value; }
        public string Cla { get => cla; set => cla = value; }
        public string Race { get => race; set => race = value; }
        public string Faction { get => faction; set => faction = value; }

        public void CreateCharacter(int slot, JArray arr)
        {
            JObject save = JObject.Parse(arr[slot - 1].ToString());
            Console.Write("Enter Name: ");
            Name = Console.ReadLine();
            save["Name"] = Name;
            int inp = HelperClasses.MultipleChoice(false, "Mage", "Warrior", "Rogue");
            switch (inp)
            {
                case 1:
                    Cla = "Mage";
                    att = 2;
                    matk = 7;
                    def = 1;
                    mdef = 5;
                    intel = 9;
                    money = 0;
                    luck = 4;
                    eva = 3;
                    break;
                case 2:
                    Cla = "Warrior";
                    att = 9;
                    matk = 2;
                    def = 5;
                    mdef = 3;
                    intel = 4;
                    money = 0;
                    luck = 2;
                    eva = 2;
                    break;
                case 3:
                    Cla = "Rogue";
                    att = 6;
                    matk = 4;
                    def = 2;
                    mdef = 3;
                    intel = 6;
                    money = 0;
                    luck = 8;
                    eva = 8;
                    break;
                default:
                    break;
            }
            save["Class"] = Cla;
            save["Attack"] = att;
            save["Defense"] = def;
            save["Magic Attack"] = matk;
            save["Magic Defense"] = mdef;
            save["Intelligence"] = intel;
            save["Money"] = money;
            save["Luck"] = luck;
            save["Evasion"] = eva;
            Console.WriteLine((string)save["Name"]);
            arr[slot - 1] = JArray.Parse(save.ToString());
            File.WriteAllText(@"Dependencies\player.json", arr.ToString());
        }
    }

    class DefaultRestore
    {
        private static string eventJson;

        public static void BackupEvent()
        {
            eventJson = File.ReadAllText(@"Dependencies\events.json");
        }

        public static void SetEventDefault()
        {

            File.WriteAllText(@"Dependencies\events.json", eventJson);
        }
    }

    internal class Sword
    {
        private readonly string name;
        private readonly int att;
        private readonly int def;
        private readonly int buyPrice;
        private readonly int sellPrice;
        private readonly int rarity;
        public Sword(int index)
        {
            JArray array = JArray.Parse(File.ReadAllText(@"Dependencies\sword.json"));
            JObject obj = JObject.Parse(array[index].ToString());
            name = (string)obj["Name"];
            att = (int)obj["Attack"];
            def = (int)obj["Defense"];
            buyPrice = (int)obj["Buy Price"];
            sellPrice = (int)obj["Sell Price"];
            rarity = (int)obj["Rarity Level"];
        }
        public Sword(string n)
        {
            JArray array = JArray.Parse(File.ReadAllText(@"Dependencies\sword.json"));
            foreach (JObject obj in array)
            {
                if (obj["Name"].ToString() == n)
                {
                    name = (string)obj["Name"];
                    att = (int)obj["Attack"];
                    def = (int)obj["Defense"];
                    buyPrice = (int)obj["Buy Price"];
                    sellPrice = (int)obj["Sell Price"];
                    rarity = (int)obj["Rarity Level"];
                }
            }
            if (string.IsNullOrEmpty(name))
                throw new InvalidOperationException("Unable to find sword: " + n);
        }

        public override string ToString()
        {
            string output = "";
            string.Format(output, "Name: {0}\nAttack: {1}\nDefense: {2}\nBuy Price:{3}\n Sell Price: {4}\nRarity Level: {5}", name, att, def, buyPrice, sellPrice, rarity);
            return output;
        }

        public string GetName()
        {
            return name;
        }

        public int[] GetVals()
        {
            return new int[] { att, def, buyPrice, sellPrice };
        }
    }
}
