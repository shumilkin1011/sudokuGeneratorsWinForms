using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Generator16x16_WF
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }



    public enum DifficultyLevel { EASY_9x9 = 30, MEDIUM_9x9 = 25, HARD_9x9 = 22, NORMAL_16x16 = 122 };
    public enum SudokuSize { SIZE_9x9 = 9, SIZE_16x16 = 16 };
    [Serializable]
    public struct LevelInfo
    {
        public int[] tiles;
        public SudokuSize size;
        public DifficultyLevel difficulty;
        public int[] closedTiles;
        public long elapsedTime;
        public long mistakes;


        //public DifficultyLevel Difficulty { get { return difficulty; } set{ difficulty = value; } }
        //public SudokuSize Size { get { return size; } set { size = value; } }
        //public int[] Tiles { get { return tiles; } set { tiles = value; } }
        //public int[] ClosedTiles { get { return closedTiles; } set { closedTiles = value; } }
        //public long ElapsedTime { get { return elapsedTime; } set { elapsedTime = value; } }
        //public long Mistakes { get { return mistakes; } set { mistakes = value; } }
    }

    public class Generator
    {
        private int N;
        private int[,] field;
        private int[,] completedField;
        private DifficultyLevel difficulty;
        System.IO.StreamWriter file;

        public int[,] Field
        {
            get { return field; }
        }

        public int[,] CompletedField
        {
            get { return completedField; }
        }

        public int Size
        {
            get { return N; }
        }

        string path;

        public string FilePath
        {
            get { return path; }
        }

        Random rnd = new Random();
        private bool find = false;

        public Generator() { }

        public void Generate(DifficultyLevel lvl, SudokuSize sudokuSize, int _count)
        {
            path = "LEVELS_" + lvl + "_" + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Millisecond;
            file = new System.IO.StreamWriter(path, true);
            difficulty = lvl;
            this.N = (int)sudokuSize;


            int counter = 0;
            int count = _count;
            while (counter < count)
            {
                while (!find)
                {
                    field = new int[N, N];
                    completedField = new int[N, N];
                    GenerateField();
                    completedField = (int[,])field.Clone();
                    CloseTiles_2();

                }

                WriteToFile();
                find = false;
                counter++;

            }
            file.Close();
        }

        private void WriteToFile()
        {

            LevelInfo info = new LevelInfo();
            info.difficulty = this.difficulty;
            info.size = (SudokuSize)this.N;
            info.closedTiles = new int[N * N];
            info.tiles = new int[N * N];
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    info.tiles[i * N + j] = this.completedField[i, j];
                    info.closedTiles[i * N + j] = this.field[i, j];
                }
            }

            string s = Newtonsoft.Json.JsonConvert.SerializeObject(info);
            file.WriteLine(s);
            file.Flush();
        }


        private bool CheckRules(int i, int j, int num)
        {

            if (CheckRow(i, num) && CheckColumn(j, num) && CheckBox(i, j, num)) return true;
            else return false;

        }

        bool CheckRow(int i, int num)
        {
            for (int j = 0; j < N; j++)
            {
                if (field[i, j] == num) return false;
            }
            return true;

        }

        bool CheckColumn(int j, int num)
        {
            for (int i = 0; i < N; i++)
            {
                if (field[i, j] == num) return false;
            }
            return true;
        }

        bool CheckBox(int i, int j, int num)
        {
            int temp = 3;
            if (N == (int)SudokuSize.SIZE_9x9) temp = 3;
            else if (N == (int)SudokuSize.SIZE_16x16) temp = 4;

            int boxCoordX = (i) / temp;
            int boxCoordY = (j) / temp;

            int startRow = boxCoordX * temp;
            int startColumn = boxCoordY * temp;

            for (int k = startRow; k < startRow + temp; k++)
            {
                for (int m = startColumn; m < startColumn + temp; m++)
                {
                    if (field[k, m] == num) return false;
                }
            }
            return true;
        }

        private bool TryNumber(int i, int j)
        {
            List<int> numbers = new List<int> { };
            for (int k = 1; k <= N; k++) numbers.Add(k);
            while (numbers.Count > 0)
            {
                int num = numbers[rnd.Next(0, numbers.Count)];
                numbers.Remove(num);
                if (CheckRules(i, j, num))
                {
                    field[i, j] = num;
                    return true;
                }
            }
            return false;

        }


        int solve(int i, int j, int[,] cells, int count /*initailly called with 0*/)
        {
            if (i == N)
            {
                i = 0;
                if (++j == N)
                    return 1 + count;
            }
            if (cells[i, j] != 0)  // skip filled cells
                return solve(i + 1, j, cells, count);
            // search for 2 solutions instead of 1
            // break, if 2 solutions are found
            for (int val = 1; val <= N && count < 2; ++val)
            {
                if (CheckRules(i, j, val))
                {
                    cells[i, j] = val;
                    // add additional solutions
                    count = solve(i + 1, j, cells, count);
                }
            }
            cells[i, j] = 0; // reset on backtrack
            return count;

        }


        private void CloseTiles_2()
        {
            int[,] tried = new int[N, N];
            int counter = 0;
            int difficult = N * N;
            int temp = 0;
            while (counter < (N * N) && difficult >= (int)difficulty)
            {
                Console.WriteLine("counter " + counter);
                Console.WriteLine("diff " + difficult);
                Console.WriteLine();
                int i = rnd.Next(0, N);
                int j = rnd.Next(0, N);

                if (tried[i, j] == 0)
                {
                    counter++;
                    tried[i, j] = 1;
                    temp = field[i, j];
                    field[i, j] = 0;
                    difficult--;

                    if (solve(0, 0, field, 0) != 1)
                    {
                        field[i, j] = temp;
                        difficult++;
                    }
                }
            }
            if (difficult <= (int)difficulty) find = true;

            Console.WriteLine("FIELD");
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    Console.Write(completedField[i, j] + " "); ;
                }
                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("PUZZLE");
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    Console.Write(field[i, j] + " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
            Console.WriteLine("Сложность: " + difficult);

        }
        /*private void CloseTiles()
        {
            Console.WriteLine("test4");
            int actuallyClosed = 0;
            int closed = N * N - (int)difficulty;
            int tries = 0;
            while (actuallyClosed < closed)
            {

                int rand = rnd.Next(0, N * N);
                int i = rand / N;
                int j = rand - N * i;

                int temp = field[i, j];
                int test = 0;
                if ((field[i, j] != 0) && ( (test =CheckSolutions(i, j)) == 1))
                {
                    actuallyClosed++;
                    Console.WriteLine("ТЕСТ" + actuallyClosed + "/" + closed + "\nSolutions=" + test);
                    Console.WriteLine("Открыто" + (N*N-actuallyClosed) + "/" + (N*N));
                    for (int c = 0; c < N; c++)
                    {
                        for (int a = 0; a < N; a++)
                        {
                            Console.Write("{0,2} ", field[c, a]);
                        }
                        Console.WriteLine();
                        Console.WriteLine();
                    }
                    tries = 0;

                }
                else
                {
                    field[i, j] = temp;

                    tries++;
                    if(tries > 2500)
                    {
                        if(actuallyClosed > 40)
                        {
                            if (actuallyClosed >= 50) s.Write("!!!WIN!!!----->>>");
                            s.WriteLine(actuallyClosed);
                            s.Flush();

                        }
                        find = false;
                        return;
                    }

                }
            }
            find = true;
            for(int i =0;i<N;i++)
            {
                for(int j = 0;j<N;j++)
                {
                    Console.Write(field[i, j] + " ");
                }
                Console.WriteLine();
                Console.WriteLine();
            }
        }*/

        private void GenerateField()
        {
            int[,] temp = (int[,])field.Clone();
            int k = 0;
            while (k < (N * N))
            {
                int i = k / N;
                int j = k - N * i;

                if (!TryNumber(i, j))
                {
                    temp[i, j]++;
                    int koef = temp[i, j] / 6;
                    for (int c = k; c >= (k - 1 - koef); c--)
                    {
                        i = c / N;
                        j = c - N * i;
                        field[i, j] = 0;
                    }

                    k = k - 1 - koef;
                    if (k < 0) k = 0;
                }
                else
                {
                    k++;
                }
            }
        }
    }


}
