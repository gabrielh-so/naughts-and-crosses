using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace lanNaughtsAndCrossesClient
{
    class Program
    {
        static bool isTurn;
        static bool gameWon = false;
        static NetworkStream ns;
        static int[,] grid;
        static byte[] inputBytes;
        static string inputString;
        static char currentPlayer;

        static void ReadInput(int charNum)
        {
            byte[] bytes = new byte[16];
            while (!ns.DataAvailable)
            {
                Thread.Sleep(100);
            }
            ns.Read(bytes, 0 , charNum);
            inputString = Encoding.ASCII.GetString(bytes);
        }
        static void SendInput(string message)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(message);
            ns.Write(bytes);
        }

        static void PrintGrid()
        {
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    Console.Write(grid[y,x] + " ");
                }
                Console.WriteLine(y);
            }
            for (int y = 0; y < 3; y++) Console.Write(y + " ");
            Console.WriteLine();
        }

        static void MakeMove()
        {
            bool moveDone = false;
            while (!moveDone) {
                PrintGrid();
                int x = -1;
                int y = -1;
                bool success = false;
                while (x < 0 || x > 2)
                {
                    Console.WriteLine("enter x value");
                    success = int.TryParse(Console.ReadLine(), out x);
                    if (!success) x = -1;
                }
                success = false;
                while (y < 0 || y > 2)
                {
                    Console.WriteLine("enter y value");
                    success = int.TryParse(Console.ReadLine(), out y);
                    if (!success) y = -1;
                }
                string output = x.ToString() + y.ToString();
                SendInput(output);
                ReadInput(2);
                if (inputString[0] == '1')
                {
                    grid[y, x] = 1;
                    moveDone = true;
                }
                else Console.WriteLine("invalid input sent to server - reenter"); // this shouldn't run anyway
            }
        }
        static void recieveOpponentMove()
        {
            PrintGrid();
            ReadInput(2);
            int x = int.Parse(inputString[0].ToString());
            int y = int.Parse(inputString[1].ToString());
            grid[y, x] = 2;
            ReadInput(2);
        }

        static void Main(string[] args)
        {
            bool done = false;

            while (!done)
            {
                try
                {
                    Console.WriteLine("trying to connect to server");
                    TcpClient client = new TcpClient("192.168.1.24", 89);
                    Console.WriteLine("connected to server! waiting for opponent");
                    ns = client.GetStream();
                    grid = new int[3, 3];
                    ReadInput(1);
                    isTurn = inputString[0] == '1';
                    currentPlayer = inputString[0];
                    while (!gameWon)
                    {
                        if (isTurn) MakeMove();
                        else recieveOpponentMove();
                        if (inputString[1] == '1')
                        {
                            gameWon = true;
                            done = true;
                            if (isTurn)
                            {
                                Console.WriteLine("you win :)");
                            }
                            else
                            {
                                Console.WriteLine("you lose :(");
                            }
                        }
                        isTurn = !isTurn;
                    }
                    ns.Close();
                    client.Close();
                }
                catch
                {
                    Console.WriteLine("server is unreachable :(");
                }
            }
            
        }
    }
}
