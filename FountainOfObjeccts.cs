IntroductoryText.Text();
Color.Write("""
    "Small" -> 4*4 World
    "Medium" -> 6*6 World
    "Large" -> 8*8 World

    Hey Player Choose The World Size..
    """ , ColorOptions.Neutral);
FountainOfObjects fountainOfObjects = new FountainOfObjects();
fountainOfObjects.ExecuteGameCycle();

 class World
{
    public Coordinate size = null!;
   public void SetWorldSize()
    { 
        var coordinate = (Console.ReadLine()!.ToLower()) switch
        {
            ("small") => (new Coordinate(4, 4)),
            ("medium") => (new Coordinate(6, 6)),
            ("large") => (new Coordinate(8, 8))
        };
        Console.Clear();
        size = coordinate;
    }
}
class FountainOfObjects
{
    readonly World world;
    public BaseMap basemap;
    readonly Player player;
    readonly PlayerEnvironmentManager PlayerManager;
    readonly PlayerEntitiesManager dangerManager;
    readonly DangerMap dangerMap;
    readonly BowAndArrow Arrow;
    public void ExecuteGameCycle()
    {
        InitializeGame();
        while (true)
        {
            PlayerManager.CheckIfPlayerIsAtEntranceRoom(); 
            dangerManager.CheckPlayerHazardNearby();
            dangerManager.HandlePlayerIsOnDanger(); if ((player.Status == PlayerStatus.Died)) return;
            Arrow.ShowArrowNumber();
            Color.Write($"You are in the room at {player.Location}", ColorOptions.Neutral);
            Console.Write("What do you want to do?: ");
            player.Commands(Console.ReadLine()!); 
            Arrow.SetArrowTrajectory();
            dangerManager.IfArrowHit();
            Arrow.AfterArrowShooted();
            PlayerManager.CheckPlayerOutOfBounds(); if ((player.Status == PlayerStatus.Died)) return;
            PlayerManager.CheckForWinCondition(); if ((player.Status == PlayerStatus.Win)) return;
            PlayerManager.CheckIfPlayerIsNearFountainRoom(); 
        }
        void InitializeGame()
        {
            basemap.InitializeBaseGrid();
            dangerMap.InitializeSpecialTiles();
        }
    }
    public FountainOfObjects()
    {
        world = new World();
        world.SetWorldSize(); // Very Important To Intilize First
        basemap = new BaseMap(new Coordinate(world.size.X , world.size.Y));
        dangerMap = new DangerMap(basemap);
        player = new Player(basemap);
        Arrow = player.Arrow;
        PlayerManager = player.PlayerEnvironmentManager;
        //PlayerManager = new PlayerManager(player , gridInitializer); Note--> Cannot use this otherwise IsFountainEnabled would be different due to different instances(State desynchroninization)
        dangerManager = new PlayerEntitiesManager(player , basemap , dangerMap , Arrow);

    }
}
static class IntroductoryText
{
   public static void Text()
    {
        Console.Clear();
        Color.Write("""
        You enter the Cavern of Objects, a maze of rooms filled with dangerous pits in search 
        of the Fountain of Objects. 
        Light is visible only in the entrance, and no other light is seen anywhere in the caverns. 
        You must navigate the Caverns with your other senses. 
        Find the Fountain of Objects, activate it, and return to the entrance. 

        1) Look out for pits. You will feel a breeze if a pit is in an adjacent room. If you enter a room with a pit, you will die

        2) Maelstroms are 
        violent forces of sentient wind. Entering a room with one could transport you to any other location in the caverns. 
        You will be able to hear their growling and groaning in nearby rooms.

        3)Amaroks roam the caverns. Encountering one is certain death, but you can smell their rotten stench in nearby rooms

        4)You carry with you a bow and a quiver of arrows. You can use them to shoot monsters in the caverns but be warned: 
        you have a limited supply
        The number of arrows can be counted by counting the "|" symbols.
        """, ColorOptions.Neutral);

        Color.Write("""
        Press "North" "South" "East" "West" To Move
        Press "Shoot North" "Shoot South" "Shoot East" "Shoot West" To Shoot
        Press "Enable fountain" "Disable Fountain" To Enable or Disable Fountain
        """, ColorOptions.SunShine);

        Color.Write("""
        Press 'HELP' To Bring The Help Command Again! 
        Press 'ENTER' To Leave The Menue
        """, ColorOptions.Alert);
        while (true)
        {
            if (Console.ReadKey().Key == ConsoleKey.Enter) { Console.Clear(); return; }
        }
        
    }
       
}
class BowAndArrow 
{
    public byte ArrowNumber = 5;
    protected Player Player;
    public ArrowDirection ArrowDirection = ArrowDirection.None;
    public List<Coordinate> ArrowTrajactory = new List<Coordinate>();
    public BowAndArrow(Player player)
    {
        Player = player;
    }
    public void SetArrowTrajectory() 
    {
        if (BeforeArrowShoot()) return;
        for (int i = 1; i < 11; i++) // 10 Should be enough for this project  
        {
            if (ArrowDirection == ArrowDirection.North) ArrowTrajactory.Add(Player.Location with { X = Player.Location.X + i });
            if (ArrowDirection == ArrowDirection.South) ArrowTrajactory.Add(Player.Location with { X = Player.Location.X - i });

            if (ArrowDirection == ArrowDirection.East) ArrowTrajactory.Add(Player.Location with { Y = Player.Location.Y + i });
            if (ArrowDirection == ArrowDirection.West) ArrowTrajactory.Add(Player.Location with { Y = Player.Location.Y - i });
        }
    }
    private bool BeforeArrowShoot()
    {
        if (ArrowNumber == 0 && ArrowDirection != ArrowDirection.None) { Color.Write("You Ran Out Of Arrows", ColorOptions.Alert); return true; }
        return false;
    }
    public void AfterArrowShooted()
    {
        int arrowNum = 1;
        foreach (Coordinate i in ArrowTrajactory)
        {
            Debug.Write($"""Arrow is in {i} {arrowNum++}""" , ColorOptions.Debug);//DEBUGING

        }

        if (ArrowTrajactory.Count != 0)
        {
            ArrowDirection = ArrowDirection.None;
            ArrowTrajactory.Clear();
            if (ArrowNumber != 0) ArrowNumber--;
        }
    }
    public void ShowArrowNumber()
    {
        if (ArrowNumber == 0) return;
        for (int i = 0; i < ArrowNumber; i++)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("|"); // didnt use color class as it would print '|' in different lines 
            Console.ForegroundColor = ConsoleColor.White;
        }
        Console.WriteLine();
    }
}
class Player //do not interact with world interactions at all!.
{
    public BowAndArrow Arrow;
    public Coordinate Location = new Coordinate(0, 0);
    public PlayerStatus Status = PlayerStatus.Playing;
    public PlayerEnvironmentManager PlayerEnvironmentManager;
    public void Commands(string command)
    {
        Console.WriteLine("-------------------------------");
        switch (command.ToLower())
        {
            //================Movement======================
            case "north":
                Location = Location with { X = Location.X + 1 };
                break;
            case "south":
                Location = Location with { X = Location.X - 1 };
                break;
            case "east":
                Location = Location with { Y = Location.Y + 1 };
                break;
            case "west":
                Location = Location with { Y = Location.Y - 1 };
                break;

            //---------SpecialCommands-----------------
            case "enable fountain":
                PlayerEnvironmentManager.EnableFountainIfPossible();
                break;
            case "disable fountain":
                PlayerEnvironmentManager.DisableFountainIfEnabled();
                break;

            default:
                Color.Write("Unknown command", ColorOptions.Alert);
                break;

            //---------ShootCommands---------------
            case "shoot north":
                Arrow.ArrowDirection = ArrowDirection.North;
                break;
            case "shoot south":
                Arrow.ArrowDirection = ArrowDirection.South;
                break;
            case "shoot east":
                Arrow.ArrowDirection = ArrowDirection.East;
                break;
            case "shoot west":
                Arrow.ArrowDirection = ArrowDirection.West;
                break;
            //--------Help----------------
            case "help":
                IntroductoryText.Text();
                break;
        }
    }
    public  List<Coordinate> GetAdjacentCells()
    {
        return new List<Coordinate>
            {
            Location with { X = Location.X + 1 },
            Location with { X = Location.X - 1 },
            Location with { Y = Location.Y + 1 },
            Location with { Y = Location.Y - 1 }
            };
    }
    public Player(BaseMap gridInitializer)
    {
        PlayerEnvironmentManager = new PlayerEnvironmentManager(this, gridInitializer);
        Arrow = new BowAndArrow(this); 
        Location = new Coordinate(0, 0); // Starting position
    }
}

class PlayerEnvironmentManager // handle interactions between the player and the environment
{
    protected bool IsFountainEnable { get; set; }
    private readonly Player Player; 
    protected readonly BaseMap Grid;

   public void EnableFountainIfPossible() /// why here- This method involves both player choice and interaction with the game world,if it was only player choice player class might be better 
    {
        if (Player.Location == Grid.FountainCordinate)
        {
            IsFountainEnable = true;
            Color.Write("The Fountain is now enabled.", ColorOptions.Success);
        }
        else
        {
            Color.Write("You must in Fountain room to enable it.", ColorOptions.Alert);
        }
    }
   public void DisableFountainIfEnabled() // working 
    {
        if (IsFountainEnable)
        {
            Color.Write("The Fountain is now Disabled.", ColorOptions.Success);
            IsFountainEnable = false;
        }
        else
        {
            Color.Write("Fountain must be enabled first to disable it!.", ColorOptions.Alert);
        }
    }
    public void CheckPlayerOutOfBounds() // working 
    {
        if (Grid.BaseCoordinatesList.Contains(Player.Location) == false)
        {
            Color.Write("You fell of the map", ColorOptions.Goodbye);
            if (Player.Location.X < 0 || Player.Location.Y < 0)
            {
                Color.Write("Player cannot be in negative coordinates" , ColorOptions.Warning);
            }
            Player.Status = PlayerStatus.Died;
        }
    }
    public void CheckIfPlayerIsNearFountainRoom()
    {
        if (Player.Location == Grid.FountainCordinate && IsFountainEnable) Color.Write("You hear the rushing waters from the Fountain of Objects. It has been reactivated!", ColorOptions.Success);//working 
        else if (Player.Location == Grid.FountainCordinate) // working 
        {
            Color.Write("You hear Water Dripping in this room. The Fountain of Objects is here! ", ColorOptions.Water);
        }
    }
    public void CheckIfPlayerIsAtEntranceRoom() // working 
    {
        if (Player.Location == new Coordinate(0, 0) && !IsFountainEnable) Color.Write("You see light coming from the cavern entrance", ColorOptions.SunShine);
    }
    public void CheckForWinCondition() 
    {
        if (Player.Location == new Coordinate(0, 0) && IsFountainEnable)
        {
            Player.Status = PlayerStatus.Win;
            Color.Write("The Fountain of Objects has been reactivated, and you have escaped with your life! \r\nYou win!", ColorOptions.Success);
        }
    }

    public PlayerEnvironmentManager(Player player , BaseMap grid)
    {
        Player = player;
        Grid = grid;
        IsFountainEnable = false;
    }
}
class PlayerEntitiesManager  //handle interactions between the player and other entities 
{
    private readonly Player Player;
    protected readonly BaseMap BaseGrid;
    protected readonly DangerMap DangerGrid;
    protected readonly BowAndArrow Arrow;
    public PlayerEntitiesManager(Player player, BaseMap baseGrid, DangerMap dangerGrid , BowAndArrow bowAndArrow) 
    { 
        Player = player; BaseGrid = baseGrid; DangerGrid = dangerGrid; Arrow = bowAndArrow;
    }

    public void CheckPlayerHazardNearby()
    {
        foreach (var coordinates in DangerGrid.DangerDictionary)
        {
            for (int i = 0; i < Player.GetAdjacentCells().Count; i++) //4
            {
                if (coordinates.Value.Contains(Player.GetAdjacentCells()[i]) == true) 
                {
                    switch (coordinates.Key)
                    {
                        case DangerOption.Pits:
                            Color.Write("You feel a draft. There is a pit in a nearby room", ColorOptions.Warning);
                            break;

                        case DangerOption.Maelstroms:
                            Color.Write("You hear the growling and groaning of a maelstrom nearby", ColorOptions.Warning);
                            break;

                        case DangerOption.Amaroks:
                            Color.Write("You can smell the rotten stench of an amarok in a nearby room", ColorOptions.Warning);
                            break;
                    }
                } 
            }
        }
           
    }
    public void HandlePlayerIsOnDanger()
    {
        int dictionaryIndex; // gets the index which caused the trigger in the dictionary 
        if (Player.Status == PlayerStatus.Died) { return; }
        foreach (var coordinates in DangerGrid.DangerDictionary) 
        {

            if (coordinates.Value.Contains(Player.Location) == true) // cannot write the false termenating condition as it would end the method as soon as the pit condition does not match even if malestorm would match 
            { 
               HandleAmaroksCollision();
               HandlePitCollision();
               HandleMaelstromCollision();

                void HandlePitCollision()
                {
                    if (coordinates.Key == DangerOption.Pits)
                    {
                        Color.Write("You fell in pit", ColorOptions.Goodbye);
                        Player.Status = PlayerStatus.Died;
                    }
                }
                void HandleMaelstromCollision()
                {
                    if (coordinates.Key == DangerOption.Maelstroms)
                    {
                        GetInteractedDangerIndex();
                        Debug.Write($" Index of {coordinates.Key} in the dictionary was {dictionaryIndex}", ColorOptions.Debug); //DEBUGING 

                        for (int j = 1; j < 2; j++)
                        {
                            // Shiting Malestorm 
                            if (BaseGrid.BaseCoordinatesList.Contains(coordinates.Value[dictionaryIndex] with { X = coordinates.Value[dictionaryIndex].X - j })) 
                                coordinates.Value[dictionaryIndex] = coordinates.Value[dictionaryIndex] with { X = coordinates.Value[dictionaryIndex].X - j }; // Subtracted 1

                            // Shiting PLayer 
                            if (BaseGrid.BaseCoordinatesList.Contains(Player.Location with { X = Player.Location.X + j }))
                                Player.Location = Player.Location with { X = Player.Location.X + j }; // Added 1
                            

                            for (int k = 1; k < 3; k++)
                            {
                                // Shiting Malestorm 
                                if (BaseGrid.BaseCoordinatesList.Contains(coordinates.Value[dictionaryIndex] with { Y = coordinates.Value[dictionaryIndex].Y - k/k }))
                                    coordinates.Value[dictionaryIndex] = coordinates.Value[dictionaryIndex] with {Y = coordinates.Value[dictionaryIndex].Y - k/k }; // Subtracted 1 + 1 if not out of bounds 
                                Debug.Write($" Malestorm Has been shifted to: {coordinates.Value[dictionaryIndex]}", ColorOptions.Debug);

                                // Shiting PLayer
                                if (BaseGrid.BaseCoordinatesList.Contains(Player.Location with { Y = Player.Location.Y + k / k }) == false) return;
                                Player.Location = Player.Location with { Y = Player.Location.Y + k / k }; // Add 1 + 1 if not out of bounds
                            }
                            Console.Beep();

                            Color.Write("You Have been shifted by the Malestorms", ColorOptions.Alert);
                            HandlePlayerIsOnDanger(); // if fallen into pit/eneny  killed by enemy  
                        }
                    }
                }
                void HandleAmaroksCollision()
                {
                    if (coordinates.Key == DangerOption.Amaroks)
                    {
                        Color.Write("You were killed by Amarocks", ColorOptions.Goodbye);
                        Player.Status = PlayerStatus.Died;
                    }
                }
            }
            void GetInteractedDangerIndex() //should be placed in whichever HandelCollision method which you wanna find the index of the danger 
            {
                int i;
                for (i = 0; i < coordinates.Value.Count; i++)
                {
                    Coordinate dangerCoordinate = coordinates.Value[i];
                    if (dangerCoordinate == Player.Location) break; // garenteed to have danger coordinate 
                }
                dictionaryIndex = i;
            }
        } 
    }
    public void IfArrowHit()
    {
        int dictionaryIndex = 0;
        Debug.Write($"Arrow Traveled {Arrow.ArrowTrajactory.Count} Blocks", ColorOptions.Debug);
        foreach (var coordinates in DangerGrid.DangerDictionary)
        {
            for (int i = 0; i < Arrow.ArrowTrajactory.Count; i++) 
            {
                if (coordinates.Value.Contains(Arrow.ArrowTrajactory[i]) == true)
                {
                    if (coordinates.Value.Contains(Player.Location)){ return; } // to not make arrow location and player location is same 
                    if (coordinates.Key == DangerOption.Amaroks)
                    {
                        GetInteractedDangerIndex();
                        Debug.Write($" Index of {coordinates.Key} in the dictionary was {dictionaryIndex}", ColorOptions.Debug); //DEBUGING
                        Color.Write("You killed Amarocks", ColorOptions.Success);
                        Debug.Write($" Amarocks was at: {coordinates.Value[dictionaryIndex]}" , ColorOptions.Debug);//DEBUGING
                        coordinates.Value.RemoveAt(dictionaryIndex);

                    }
                    if (coordinates.Key == DangerOption.Maelstroms)
                    {
                        GetInteractedDangerIndex();
                        Debug.Write($" Index of {coordinates.Key} in the dictionary was {dictionaryIndex}", ColorOptions.Debug); //DEBUGING
                        Color.Write("You killed Maelstroms", ColorOptions.Success);
                        Debug.Write($" Amarocks was at: {coordinates.Value[dictionaryIndex]}", ColorOptions.Debug);//DEBUGING
                        coordinates.Value.RemoveAt(dictionaryIndex);
                    }
                    void GetInteractedDangerIndex() //should be placed in whichever HandelCollision method which you wanna find the index of the danger 
                    {
                        int i;
                        for (i = 0; i < coordinates.Value.Count; i++)
                        {
                            Coordinate dangerCoordinate = coordinates.Value[i];
                            Console.WriteLine(i + "" + coordinates.Value[i]);
                            foreach ( var currnetArrowCoordinate in  Arrow.ArrowTrajactory)
                            {
                                dictionaryIndex = i;
                                if (dangerCoordinate == currnetArrowCoordinate) { return; }
                            }
                        }
                    }
                }
            }
        }
    }
}

class BaseMap
{
    readonly Random Random = new Random();
    public Coordinate FountainCordinate;
    public Coordinate MaxBaseCoordinate { get; private set; }
    public List<Coordinate> BaseCoordinatesList = new List<Coordinate>();

    public List<Coordinate> ForbittenAreas()
    {
       return new List<Coordinate>() { FountainCordinate, new Coordinate(0, 0) };
    }
    public void InitializeBaseGrid()
    {
        for (int BaseGridColumb = 0; BaseGridColumb <= MaxBaseCoordinate.X; BaseGridColumb++)
        {
            for (int BaseGridRow = 0; BaseGridRow <= MaxBaseCoordinate.Y; BaseGridRow++)
            {
                Coordinate currentCoordinate = new Coordinate(BaseGridColumb, BaseGridRow);
                BaseCoordinatesList.Add(currentCoordinate);
               // Console.Write(currentCoordinate); //TO SHOW GRID (Helpful for Debbugging)
            }
             //Console.WriteLine(); // New line after each row
        }
        Console.WriteLine("--------------------------------------------");
        // Console.WriteLine();
    }
    public BaseMap(Coordinate gridSize)
    {
        MaxBaseCoordinate = gridSize;// Initialize with default values (0)
        FountainCordinate = new Coordinate(Random.Next(0, MaxBaseCoordinate.X + 1), (Random.Next(0, MaxBaseCoordinate.Y + 1)));
    }
}

class DangerMap
{

    readonly Random Random = new Random();
    public Dictionary<DangerOption, List<Coordinate>> DangerDictionary = new Dictionary<DangerOption, List<Coordinate>>();
    private readonly BaseMap baseMap;
    void InitializeSpecialTiles(DangerOption tileType, Coordinate coordinate)
    {
        if (!DangerDictionary.ContainsKey(tileType)) //cannot have two same keys 
        {
            DangerDictionary[tileType] = new List<Coordinate>();
        }
        DangerDictionary[tileType].Add(coordinate); // modiefying existing key
    }
    public void InitializeSpecialTiles()
    {
        int j = 0;
        List<DangerOption> weapons = Enum.GetValues(typeof(DangerOption)).Cast<DangerOption>().ToList();
        int gridSide = 0;

        Color.Write("""
            Select "1" for Single PLayer Mode
            Select "2" for Dual Player Mode 
            """ ,ColorOptions.SunShine);
        Debug.Write($"Fountain Coordinates: {baseMap.FountainCordinate}" , ColorOptions.Debug);//DEBUGING

        string IsSinglePLayer = Console.ReadLine()!;
        List<Coordinate> generatedNumbers = new List<Coordinate>();
        foreach (DangerOption item in weapons)
        {
            int no = 0;
            if (baseMap.MaxBaseCoordinate == new Coordinate(4, 4))
            {
                if (item == DangerOption.Pits) no = 1;
                if (item == DangerOption.Maelstroms) no = 1;
                if (item == DangerOption.Amaroks) no = 1;
                gridSide = baseMap.MaxBaseCoordinate.X; //4*4
            }
            if (baseMap.MaxBaseCoordinate == new Coordinate(6, 6))
            {
                if (item == DangerOption.Pits) no = 2;
                if (item == DangerOption.Maelstroms) no = 1;
                if (item == DangerOption.Amaroks) no = 2;
                gridSide = baseMap.MaxBaseCoordinate.X; // 6*6
            }
            if (baseMap.MaxBaseCoordinate == new Coordinate(8, 8))
            {
                if (item == DangerOption.Pits) no = 4;
                if (item == DangerOption.Maelstroms) no =2;
                if (item == DangerOption.Amaroks) no = 3;
                gridSide = baseMap.MaxBaseCoordinate.X; // **8
            }
            TileSetup();
            void TileSetup()
            {
                for (int i = 0; i < no; i++) // Make sure if a Pit requires 2 times to be intilize it can be 
                {
                    switch (IsSinglePLayer)
                    {
                        case "1":

                            // create for loop for intilizing speacial tiles and fix Contains method 
                                Coordinate randomCoordiante = new Coordinate(Random.Next(0, gridSide+1), Random.Next(0, gridSide+1));
                                if (generatedNumbers.Contains(randomCoordiante)) { i--; Debug.Write($" {randomCoordiante} Has Been Intilized Before Intilizing Anothor..", ColorOptions.Debug); continue; }  // Check if the number is unique
                                if (baseMap.ForbittenAreas().Contains(randomCoordiante)) { i--; Debug.Write($" CANNOT be {randomCoordiante} here",ColorOptions.Debug); continue; } // check if the number does not lie on Already intilized areas 
                                generatedNumbers.Add(randomCoordiante);
                                InitializeSpecialTiles(item, generatedNumbers[j]);
                                Debug.Write($"{item} tile initialized at: ({generatedNumbers[j]})" , ColorOptions.Debug); //DEBUGING
                            j++;
                                break;

                        case "2":

                            Color.Write($"Enter new Coordinate for {item}:{i+1} for your oponent", ColorOptions.Neutral);
                            Color.Write($"""First type "X" Coordinate Then Press "Enter" Then type "Y" Coordinate""", ColorOptions.Warning);
                            Coordinate SpeacialCoordiante = new Coordinate(Convert.ToInt32(Console.ReadLine()), Convert.ToInt32(Console.ReadLine())); Console.Clear();
                            if (baseMap.ForbittenAreas().Contains(SpeacialCoordiante)) //HOW ABOUT MAKING A LIST OF FORBITTEN AREAS (RESERVED AREAS LIST)                                                                                                                          
                            {
                                Color.Write($"New Coordinate Cannot be at Origin or fountain{baseMap.FountainCordinate}!", ColorOptions.Alert);
                                i--;
                                continue;
                            }
                            InitializeSpecialTiles(item, SpeacialCoordiante);
                            break;
                    }
                
                }

            }

        }
    }
    public DangerMap(BaseMap basemap)
    {
        this.baseMap = basemap; 
    }
}
class Color
{
    public static void Write(string message, ColorOptions color)
    {
        Console.ForegroundColor = color switch
        {
            ColorOptions.Alert => ConsoleColor.Red,
            ColorOptions.Water => ConsoleColor.Blue,
            ColorOptions.SunShine => ConsoleColor.Yellow,
            ColorOptions.Success => ConsoleColor.Green,
            ColorOptions.Goodbye => ConsoleColor.Magenta,
            ColorOptions.Neutral => ConsoleColor.Cyan,
            ColorOptions.Warning => ConsoleColor.DarkYellow,
            _ => ConsoleColor.White,
        };
        Console.WriteLine(message);
        Default();
    }
    private static void Default() { Console.ForegroundColor = ConsoleColor.White; }
}
class Debug
{
    public static void Write(string message, ColorOptions color)
    {
        Console.ForegroundColor = color switch
        {
            ColorOptions.Debug => ConsoleColor.DarkCyan,
            _ => ConsoleColor.White,
        };
        Console.Write("DEBUGGING TEXT: ");
        Console.WriteLine(message);
        Default();
    }
    private static void Default() { Console.ForegroundColor = ConsoleColor.White; }
}
enum ColorOptions { Success, Alert, Neutral, Goodbye, Water, SunShine, Warning , Debug }
enum DangerOption { Pits, Maelstroms, Amaroks }
enum PlayerStatus { Win, Playing, Died }
enum ArrowDirection { North, South, East, West , None }
public record Coordinate(int X, int Y)
{
    public int X = X; public int Y = Y;
}
