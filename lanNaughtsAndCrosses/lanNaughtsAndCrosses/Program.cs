using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Threading;

namespace lanNaughtsAndCrosses
{
    class Game
    {
        TcpClient Player1;
        TcpClient Player2;
        NetworkStream Player1Stream;
        NetworkStream Player2Stream;
        int[,] grid = new int[3, 3];
        bool finished;
        bool player1Turn;
        byte[] bytes = new byte[9];

        public Game(TcpClient p1, TcpClient p2)
        {
            player1Turn = true;
            finished = false;
            Player1Stream = Player1.GetStream();
            Player2Stream = Player2.GetStream();
        }
        

    }
    class Program
    {
        static void PrintGrid(int[,] grid)
        {
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    Console.Write(grid[y, x] + " ");
                }
                Console.WriteLine(y);
            }
            for (int y = 0; y < 3; y++) Console.Write(y + " ");
            Console.WriteLine();
        }

        static bool CheckForWin(int[,] grid)
        {
            // top row
            if (grid[0, 0] == grid[0, 1] && grid[0, 1] == grid[0, 2] && grid[0, 0] != 0) return true;
            // middle row
            if (grid[1, 0] == grid[1, 1] && grid[1, 1] == grid[1, 2] && grid[1, 0] != 0) return true;
            // bottom row
            if (grid[2, 0] == grid[2, 1] && grid[2, 1] == grid[2, 2] && grid[2, 0] != 0) return true;
            // left column
            if (grid[0, 0] == grid[1, 0] && grid[1, 0] == grid[2, 0] && grid[0, 0] != 0) return true;
            // middle column
            if (grid[0, 1] == grid[1, 1] && grid[1, 1] == grid[2, 1] && grid[0, 1] != 0) return true;
            // right column
            if (grid[0, 2] == grid[1, 2] && grid[1, 2] == grid[2, 2] && grid[0, 2] != 0) return true;
            // top left diagonal
            if (grid[0, 0] == grid[1, 1] && grid[1, 1] == grid[2, 2] && grid[0, 0] != 0) return true;
            // bottom left diagonal
            if (grid[2, 0] == grid[1, 0] && grid[1, 1] == grid[0, 2] && grid[2, 0] != 0) return true;
            return false;
        }
        static void Game(TcpClient Player1, TcpClient Player2)
        {
            bool player1Turn = true;
            bool done = false;
            int[,] grid = new int[3, 3];
            byte[] bytes = new byte[16];
            NetworkStream Player1Stream = Player1.GetStream();
            NetworkStream Player2Stream = Player2.GetStream();
            bytes = Encoding.ASCII.GetBytes("1");
            Player1Stream.Write(bytes, 0, 1);
            bytes = Encoding.ASCII.GetBytes("2");
            Player2Stream.Write(bytes, 0, 1);
            while (!done)
            {
                if (player1Turn)
                {
                    if (Player1Stream.DataAvailable)
                    {
                        try // NEVER trust user input
                        {
                            bytes = new byte[2];
                            int size = Player1Stream.Read(bytes, 0, 2);
                            string player1move = Encoding.ASCII.GetString(bytes);
                            int x = int.Parse(player1move[0].ToString());
                            int y = int.Parse(player1move[1].ToString());
                            if (grid[y, x] == 0)
                            {
                                grid[y, x] = 1;
                                Player2Stream.Write(bytes);
                                bytes[0] = (byte)'1';
                                bytes[1] = (byte)'0';
                                if (CheckForWin(grid))
                                {
                                    PrintGrid(grid);
                                    bytes[1] = (byte)'1'; // someone won, so you can stop now
                                    done = true;
                                }
                                Player1Stream.Write(bytes, 0, 2);
                                Player2Stream.Write(bytes, 0, 2);
                                player1Turn = false;
                            }
                            else
                            {
                                bytes = Encoding.ASCII.GetBytes("00"); // something screwed up - resend your input
                                Player1Stream.Write(bytes, 0, 1);
                            }
                            Player1Stream.Flush(); // don't care about anything else sent since then
                            Player2Stream.Flush();
                        }
                        catch
                        {
                            bytes = Encoding.ASCII.GetBytes("00");
                            Player1Stream.Write(bytes, 0, 1);
                        }
                    }
                }
                else
                {
                    if (Player2Stream.DataAvailable)
                    {
                        try // NEVER trust user input
                        {
                            bytes = new byte[2];
                            int size = Player2Stream.Read(bytes, 0, 2);
                            string player2move = Encoding.ASCII.GetString(bytes);
                            int x = int.Parse(player2move[0].ToString());
                            int y = int.Parse(player2move[1].ToString());
                            if (grid[y, x] == 0)
                            {
                                grid[y, x] = 2;
                                Player1Stream.Write(bytes);
                                bytes[0] = (byte)'1';
                                bytes[1] = (byte)'0';
                                if (CheckForWin(grid))
                                {
                                    bytes[1] = (byte)'1'; // someone won, so you can stop now
                                    done = true;
                                }
                                Player2Stream.Write(bytes, 0, 2);
                                Player1Stream.Write(bytes, 0, 2);
                                player1Turn = true;
                            }
                            else
                            {
                                bytes = Encoding.ASCII.GetBytes("00"); // something screwed up - resend your input
                                Player2Stream.Write(bytes, 0, 1);
                            }
                            Player1Stream.Flush(); // don't care about anything else sent since then
                            Player2Stream.Flush();
                        }
                        catch
                        {
                            bytes = Encoding.ASCII.GetBytes("00");
                            Player1Stream.Write(bytes, 0, 1);
                        }
                    }
                }
                if (!Player1.Connected || !Player2.Connected)
                {
                    Player1Stream.Close();
                    Player2Stream.Close();
                    Player1.Close();
                    Player2.Close();
                    Console.WriteLine(Thread.CurrentThread.Name + ": player disconnected - thread closing");
                    return;
                }
            }
        }

        static void StartServer()
        {
            bool done = false;
            TcpListener server = new TcpListener(IPAddress.Any, 89);
            server.Start();
            while (!done)
            {
                Console.WriteLine("listening for first player");
                TcpClient p1 = new TcpClient();
                while (!p1.Connected)
                    p1 = server.AcceptTcpClient();
                Console.WriteLine("listening for second player");
                TcpClient p2 = new TcpClient();
                while (!p2.Connected)
                    p2 = server.AcceptTcpClient();
                Thread t = new Thread(() => Game(p1, p2));
                t.Start();
            }
        }

        static void Main(string[] args)
        {
            
            byte[] bytes = Encoding.ASCII.GetBytes("this is a message");
            string s = Encoding.ASCII.GetString(bytes);
            Console.WriteLine(s);
            

            StartServer();
            
        }
    }
}
