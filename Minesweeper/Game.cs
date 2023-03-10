using System;

namespace Game{
    public class GameThread
    {
        static Tile[,] grid;
        static int[] cursorCoord = new int[2]{0,0};
        static int flagCount = 0;
        static void Main(string[] args)
        {
            Start();
            while (true)
            {
                ConsoleKeyInfo cursor = Console.ReadKey();
                Update(cursor);
            }
        }
        static void Start()
        {
            Console.Clear();
            flagCount = 40;
            grid = new Tile[16,16];
            List<Tile> bombs = new List<Tile>();
            while (bombs.ToArray().Length < 40)
            {
                Random rCoord = new Random();
                Tile toAdd = new Tile(true,rCoord.Next(0,15),rCoord.Next(0,15),ref grid);
                bool exists = false;
                foreach (Tile bomb in bombs)
                {
                    if (bomb.GetCoords()[0] == toAdd.GetCoords()[0] && bomb.GetCoords()[1] == toAdd.GetCoords()[1])
                        exists = true;
                }
                if (!exists) {
                    bombs.Add(toAdd);
                    grid[toAdd.GetCoords()[0],toAdd.GetCoords()[1]] = toAdd;
                }
            }

            // Fill all other tiles
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                    grid[x,y] = grid[x,y] == null ? new Tile(false, x, y, ref grid) : grid[x,y];
            }

            // Find place to start
            bool empty = false;
            Random findEmpty = new Random();
            while (!empty)
            {
                int rX = findEmpty.Next(2, 13);
                int rY = findEmpty.Next(2, 13);
                if (grid[rX,rY].GetTileType() == ' ') 
                {
                    grid[rX,rY].Hit(ref grid);
                    empty = true;
                }
            }

            Draw(0,0, grid, false);
            Console.SetCursorPosition(0,0);
            Update(new ConsoleKeyInfo());
        }

        static void Update(ConsoleKeyInfo cursor)
        {
            int x = cursorCoord[0];
            int y = cursorCoord[1];
            bool hitBomb = false;
            switch(cursor.Key){
                case ConsoleKey.UpArrow:
                    y = y > 0 ? y-1 : y;
                    break;
                case ConsoleKey.DownArrow:
                    y = y < 15 ? y+1 : y;
                    break;
                case ConsoleKey.LeftArrow:
                    x = x > 0 ? x-1 : x;
                    break;
                case ConsoleKey.RightArrow:
                    x = x < 15 ? x+1 : x;
                    break;
                case ConsoleKey.Enter:
                    if (grid[x,y].Hit(ref grid))
                        hitBomb = true;
                    break;
                case ConsoleKey.End:
                    grid[x,y].Flag(ref flagCount);
                    break;
                case ConsoleKey.Escape:
                    Start();
                    break;
            }
            cursorCoord = new int[2]{x,y};
            Draw(0,0, grid, hitBomb);
            Console.SetCursorPosition(x,y);
            if (hitBomb)
            {
                while(Console.ReadKey().Key != ConsoleKey.Escape);
                Start();
            }
        }

        static void Draw(int cursorX, int cursorY, Tile[,] grid, bool hitBomb)
        {
            Console.SetCursorPosition(0,0);
            Tile[,] pixelGrid = new Tile[16,16];
            if (hitBomb){
                for (int x = 0; x < 16; x++){
                    for(int y = 0; y < 16; y++){
                        grid[x,y].ForceDisplay();
                    }
                }
            }
            for (int x = 0; x < 16; x++)
                for(int y = 0; y < 16; y++){
                    pixelGrid[y,x] = grid[x,y];
                }
            for (int y = 0; y < 16; y++){
                for (int x = 0; x < 16; x++){
                    if (x == cursorCoord[0] && y == cursorCoord[1])
                        pixelGrid[y,x].Draw(true);
                    else
                        pixelGrid[y,x].Draw(false);
                }
                Console.Write("\n");
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Black;
            }
            if (flagCount > 0)
                Console.Write("\n Flags Left: {0}", flagCount.ToString("D2"));
            else
            {
                Console.Write("\n ");
                Console.BackgroundColor = ConsoleColor.Red;
                Console.Write("No flags left!");
            }
        }
    }

    public class Tile
    {
        private char type;
        private bool display;
        private bool flag;
        private int count;
        public int[] coord = new int[2];
        public static IDictionary<char, ConsoleColor> cols = new Dictionary<char, ConsoleColor>(){
            {'b',ConsoleColor.Black},
            {'f',ConsoleColor.Red},
            {'1',ConsoleColor.Blue},
            {'2',ConsoleColor.DarkGreen},
            {'3',ConsoleColor.DarkRed},
            {'4',ConsoleColor.Magenta},
            {'5',ConsoleColor.DarkMagenta},
            {'6',ConsoleColor.Cyan},
            {'7',ConsoleColor.Black},
            {'8',ConsoleColor.Gray},
            {' ', ConsoleColor.White},
            {'X',ConsoleColor.Red},
            {'/',ConsoleColor.Green}
        };

        public Tile(bool isBomb, int x, int y, ref Tile[,] grid)
        {
            coord = new int[2]{x,y};
            if (isBomb)
            {
                type = 'b';
            }
            else
            {
                type = ' ';
                count = 0;
                for (int xOff = x-1; xOff < x+2; xOff++)
                {
                    for (int yOff = y-1; yOff < y+2; yOff++)
                    {
                        try{if (grid[xOff, yOff]?.type == 'b'){
                            count += 1;}}catch {}
                    }
                }
                if (count > 0)
                {
                    type = count.ToString()[0];
                }
            }
        }
        public void Draw(bool cursor)
        {
            Console.BackgroundColor = cursor ? ConsoleColor.DarkYellow : ConsoleColor.Green;
            if (flag)
            {
                Console.ForegroundColor = cols['f'];
                Console.Write('F');
                return;
            }
            if (display)
            {
                Console.ForegroundColor = cols[type];
                Console.BackgroundColor = cursor ? ConsoleColor.DarkYellow : ConsoleColor.White;
                Console.BackgroundColor = type == '/' ? ConsoleColor.DarkGreen : (type == 'X' ? ConsoleColor.DarkRed : Console.BackgroundColor);
                Console.BackgroundColor = type == 'b' ? ConsoleColor.DarkYellow : Console.BackgroundColor;
                Console.Write(type);
                return;
            }
            Console.Write(' ');
        }
        public bool Hit(ref Tile[,] grid)
        {
            if (!flag && type == 'b')
                return true;
            if (type == ' ')
                Empty(ref grid);
            if (!flag)
                display = true;
            return false;
        }
        public void Empty(ref Tile[,] grid)
        {
            display = true;
            for (int xOff = coord[0]-1; xOff < coord[0]+2; xOff++)
            {
                for (int yOff = coord[1]-1; yOff < coord[1]+2; yOff++)
                {
                    try{if (type == ' ' && !grid[xOff, yOff].HasDisplayed()){
                        grid[xOff, yOff].Empty(ref grid);}}catch {}
                }
            }
        }
        public void Flag(ref int fCount)
        {
            flag = display ? false : !flag;
            if (!display)
                fCount = flag ? fCount-1 : fCount+1;
        }

        public char GetTileType()
        {
            return type;
        }
        public int[] GetCoords()
        {
            return coord;
        }
        public bool HasDisplayed()
        {
            return display;
        }
        public void ForceDisplay()
        {
            display = true;
            type = type == 'b' && flag ? '/' : (type != 'b' && flag ? 'X' : type);
            flag = false;
        }
    }
}