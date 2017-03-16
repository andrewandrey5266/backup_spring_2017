using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Drawing;

namespace ClosedType
{
    public class Pack
    {
        public int[] Supply { get; set; }
        public int[] Demand { get; set; }
        public int[,] Matrix { get; set; }
        public Pack() { }
        public Pack(int supplyLength, int demandLength, int matrixLengthx, int matricLengthy)
        {
            Supply = new int[supplyLength];
            Demand = new int[demandLength];
            Matrix = new int[matricLengthy, matrixLengthx];
        }
    }
    class Solver
    {
        public static Pack PotentialsMethod(Pack closed, Pack optimized)
        {
            closed = new Pack
            {
                Matrix = closed.Matrix.Clone() as int[,],
                Demand = new int[closed.Demand.Length].ToList().Select(x=> -666).ToArray(),
                Supply = new int[closed.Supply.Length].ToList().Select(x => -666).ToArray()
            };
            closed.Demand[0] = 0;
            var result = Solver.BuildPotentials(closed, optimized);
            if (Solver.CheckForOptimality(result))            
                Solver.RedistribSupplies(result,optimized);            

            return null;
        }
        private static Pack BuildPotentials(Pack init, Pack optimized)
        {
            bool quit = true;
            do 
            {
                quit = true;
                for(int i  = 0; i < optimized.Matrix.GetLength(0); i++)
                {
                    for(int j = 0; j < optimized.Matrix.GetLength(1); j++)
                    {
                        if(optimized.Matrix[i,j] != 0)
                        {
                            //quit = false;
                            if (init.Demand[i] != -666)
                            {
                                quit = false;
                                optimized.Matrix[i, j] = 0;
                                init.Supply[j] = init.Matrix[i, j] - init.Demand[i]; 
                            }
                            if (init.Supply[j] != -666)
                            {
                                quit = false;
                                optimized.Matrix[i, j] = 0;
                                init.Demand[i] = init.Matrix[i, j] - init.Supply[j] ;

                            }
                        }
                    }
                }
               
            } while (!quit) ;

            PrintMatrix(init.Matrix);
            PrintArray(init.Demand);
            PrintArray(init.Supply);
            return init;
        }
        private static bool CheckForOptimality(Pack pack)
        {
            for (int i = 0; i < pack.Matrix.GetLength(0); i++)
            {
                for (int j = 0; j < pack.Matrix.GetLength(1); j++)
                {
                    if (pack.Matrix[i, j] - pack.Supply[j] - pack.Demand[i] < 0)
                        return false;
                }
            }
            return true;
        }
        private static Pack RedistribSupplies(Pack init, Pack optimized)
        {
            var initcopy = init.Matrix.Clone() as int[];
           

            do
            {
                int x, y;
                int max = int.MaxValue;
                // origin of root
                for(int i = 0; i < init.Matrix.GetLength(0); i++)
                {
                    for(int j = 0; j < init.Matrix.GetLength(1); j++)
                    {
                        if (init.Matrix[i, j] < 0 && init.Matrix[i, j] * -1 > max)
                        {
                            max = init.Matrix[i, j];
                            x = i; y = j;
                        }

                    }
                }
                // shape of root
                for (int i )


            } while (!CheckForOptimality(init));

            return init;
        }
        public static Pack VogelsMethod(int[,] initMatrix, int[] supply, int[] demand)
        {
            var result = new Pack(supply.Length, demand.Length, initMatrix.GetLength(1), initMatrix.GetLength(0));
            var crossed = new bool[initMatrix.GetLength(0), initMatrix.GetLength(1)];
           
            CopyArray(supply, result.Supply);
            CopyArray(demand, result.Demand);


            while (crossed.Cast<bool>().ToList().IndexOf(false) != -1 && result.Supply.ToList().Max() > 0)
            {
                int minI = -1;
                int minJ = -1;

                FindCell(result,initMatrix, crossed, ref minJ, ref minI);
                               
                if (result.Supply[minI] > result.Demand[minJ])
                {
                    result.Matrix[minI, minJ] = result.Demand[minJ];
                    result.Supply[minI] -= result.Demand[minJ];
                    result.Demand[minJ] = 0;
                    //cross
                    for (int i = 0; i < crossed.GetLength(0); i++)
                        crossed[i, minJ] = true;
                }
                else if (result.Demand[minJ] > result.Supply[minI])
                {
                    result.Matrix[minI, minJ] = result.Supply[minI];
                    result.Demand[minJ] -= result.Supply[minI];
                    result.Supply[minI] = 0;

                    //cross
                    for (int i = 0; i < crossed.GetLength(1); i++)
                        crossed[minI, i] = true;
                }
                else if (result.Supply[minI] == result.Demand[minJ])
                {
                    result.Matrix[minI, minJ] = result.Demand[minJ];
                    result.Supply[minI] = 0;
                    result.Demand[minJ] = 0;

                    //cross
                    for (int i = 0; i < crossed.GetLength(0); i++)
                        crossed[i, minJ] = true;

                    for(int i = 0; i < crossed.GetLength(1); i++)
                        crossed[minI, i] = true;
                }
            }

           

            return result;
        }
        private static void FindCell(Pack matrix, int[,] initMatrix, bool[,] crossed, ref int minJ, ref int minI)
        {
            int[] _supply = new int[matrix.Demand.Length];
            int[] _demand = new int[matrix.Supply.Length];

            //rows
            for (int i = 0; i < matrix.Matrix.GetLength(0); i++)
            {
                var row = GetRow(initMatrix, i, crossed);
                if (row.Length >= 2)
                {
                    var mins = row.OrderBy(e => e).Take(2).ToList();
                    _supply[i] = mins.Zip(mins.Skip(1), (x, y) => y - x).First();
                }
                else if (row.Length == 1)
                    _supply[i] = row[0];

                else
                    _supply[i] = int.MinValue;
                
            }
            for (int i = 0; i < matrix.Matrix.GetLength(1); i++)
            {
                var column = GetColumn(initMatrix, i, crossed);
                if (column.Length >= 2)
                {
                    var mins = column.OrderBy(e => e).Take(2).ToList();
                    _demand[i] = mins.Zip(mins.Skip(1), (x, y) => y - x).First();
                }
                else if (column.Length == 1)
                    _demand[i] = column[0];

                else
                    _demand[i] = int.MinValue;

            }

            int _supplyMax = _supply.Max();
            int _demandMax = _demand.Max();

            if (_supplyMax > _demandMax)
            {
                minI = _supply.ToList().IndexOf(_supplyMax);
                var row = GetRow(initMatrix, minI, crossed);

                minJ = GetRow(initMatrix, minI, null).ToList().IndexOf(row.Min());
            }
            if(_demandMax > _supplyMax)
            {
                minJ = _demand.ToList().IndexOf(_demandMax);
                var column = GetColumn(initMatrix, minJ, crossed);

                minI = GetColumn(initMatrix, minJ, null).ToList().IndexOf(column.Min());
            }
            if(_demandMax == _supplyMax)
            {
                minI = _supply.ToList().IndexOf(_supplyMax);
                var row = GetRow(initMatrix, minI, crossed);

                minJ = _demand.ToList().IndexOf(_demandMax);
                var column = GetColumn(initMatrix, minJ, crossed);

                if (row.Min() <= column.Min())
                    minJ = GetRow(initMatrix, minI, null).ToList().IndexOf(row.Min());
                else
                    minI = GetColumn(initMatrix, minJ, null).ToList().IndexOf(column.Min());
            }
            //crossed[minI, minJ] = true;
        }
        private static int[] GetColumn(int[,] matrix, int index, bool[,] crossed)
        {
            var result = new List<int>();
            if (crossed == null) crossed = new bool[matrix.GetLength(0), matrix.GetLength(1)];

            for (int i = 0; i < matrix.GetLength(1); i++)
                if (crossed[i, index] == false)
                    result.Add(matrix[i, index]);

            return result.ToArray();
        }
        private static int[] GetRow(int[,] matrix, int index, bool[,] crossed)
        {
            var result = new List<int>();
            if (crossed == null) crossed = new bool[matrix.GetLength(0), matrix.GetLength(1)];

            for (int i = 0; i < matrix.GetLength(0); i++)
                if (crossed[index, i] == false)
                    result.Add(matrix[index, i]);

            return result.ToArray();
        }
        public static Pack MinimalElementMethod(int[,] initMatrix, int[] supply, int[] demand)
        {
            var result = new Pack(supply.Length, demand.Length, initMatrix.GetLength(1), initMatrix.GetLength(0));
            CopyArray(supply, result.Supply);
            CopyArray(demand, result.Demand);

            while (result.Supply.ToList().Max() > 0)
            {
                int min = int.MaxValue;
                int minJ = -1;
                int minI = -1;                

                for (int i = 0; i < initMatrix.GetLength(0); i++)
                {
                    for (int j = 0; j < initMatrix.GetLength(1); j++)
                    {
                        if (initMatrix[i, j] < min && result.Supply[i] != 0 && result.Demand[j] != 0)
                        {
                            min = initMatrix[i, j];
                            minJ = j;
                            minI = i;
                        }
                    }
                }

                //if (minJ == -1) break;

                if(result.Supply[minI] > result.Demand[minJ])
                {
                    result.Matrix[minI,minJ] = result.Demand[minJ];
                    result.Supply[minI] -= result.Demand[minJ];    
                    result.Demand[minJ] = 0;
                }
                else if (result.Demand[minJ]> result.Supply[minI] )
                {
                    result.Matrix[minI, minJ] = result.Supply[minI];
                    result.Demand[minJ] -= result.Supply[minI];
                    result.Supply[minI] = 0;
                }
                else if (result.Supply[minI] == result.Demand[minJ])
                {
                    result.Matrix[minI, minJ] = result.Demand[minJ];
                    result.Supply[minI] = 0;
                    result.Demand[minJ] = 0;
                }
            }

            return result;
        }
        public static Pack NorthWestMethod(int[,] initMatrix, int[] supply, int[] demand)
        {
            //pdf = new string[initMatrix.GetLength(0) + 1, initMatrix.GetLength(1) + 1];
            //for (int i = 0; i < pdf.GetLength(0); i++)
            //{
            //    for (int j = 0; j < pdf.GetLength(1); j++)
            //    {
            //        pdf[i, j] = "";
            //    }
            //}
            //for (int i = 0; i < demand.Length; i++)
            //{
            //    pdf[pdf.GetLength(0) - 1, i] = demand[i].ToString();
            //}
            //for (int j = 0; j < supply.Length; j++)
            //{
            //    pdf[j, pdf.GetLength(1) - 1] = supply[j].ToString();
            //}

            var result = new Pack(supply.Length, demand.Length, initMatrix.GetLength(1), initMatrix.GetLength(0));
            CopyArray(supply, result.Supply);
            CopyArray(demand, result.Demand);

            int shiftX = 0;          
            for (int i = 0; i < initMatrix.GetLength(0); i++)
            {
                for (int j = shiftX; j < initMatrix.GetLength(1); j++)
                {
                    if (j == 0) shiftX = 0;

                    if (result.Supply[i] > result.Demand[j])
                    {
                        result.Matrix[i, j] = result.Demand[j];
                        //pdf[i, j] = result.Matrix[i, j].ToString();
                        //pdf[i, pdf.GetLength(1) - 1] += " / " + (result.Demand[i] - result.Matrix[i, j]).ToString();
                        //pdf[pdf.GetLength(0) - 1, j] += " / " + (result.Supply[i] - result.Matrix[i, j]).ToString();

                        result.Supply[i] -= result.Demand[j];
                        result.Demand[j] = 0;

                        
                    }
                    else if (result.Demand[j] > result.Supply[i])
                    {
                        result.Matrix[i, j] = result.Supply[i];
                        //pdf[i, j] = result.Matrix[i, j].ToString();
                        //pdf[i, pdf.GetLength(1) - 1] += " / " + (result.Demand[i] - result.Matrix[i, j]).ToString();
                        //pdf[pdf.GetLength(0) - 1, j] += " / " + (result.Supply[i] - result.Matrix[i, j]).ToString();

                        result.Demand[j] -= result.Supply[i];
                        result.Supply[i] = 0;
                        shiftX = j;

               
                        break;
                    }
                    else if (result.Demand[j] == result.Supply[i])
                    {
                        result.Matrix[i, j] = result.Supply[i];
                        //pdf[i, j] = result.Matrix[i, j].ToString();
                        //pdf[i, pdf.GetLength(1) - 1] += " / " + (result.Demand[i] - result.Matrix[i, j]).ToString();
                        //pdf[pdf.GetLength(0) - 1, j] += " / " + (result.Supply[i] - result.Matrix[i, j]).ToString();

                        result.Supply[i] = 0;
                        result.Demand[j] = 0;
                        shiftX = j + 1;

                        
                        break;

                    }
                }                
            }
            return result;
        }
        public static Pack CloseMatrix(int [,] initMatrix, int[] supply, int[] demand)
        {
            var result = new Pack();

            int supplySum = supply.Sum();
            int demandSum = demand.Sum();

            if(supplySum > demandSum)
            {
                result.Matrix = new int[initMatrix.GetLength(0), initMatrix.GetLength(1) + 1];
                result.Demand = new int[demand.Length + 1];
                result.Supply = supply.Clone() as int[];

                CopyMatrix(initMatrix, result.Matrix);
                CopyArray(demand, result.Demand);

                result.Demand[result.Demand.Length - 1] = supplySum - demandSum;
            }
            if(demandSum > supplySum)
            {
                result.Matrix = new int[initMatrix.GetLength(0) + 1, initMatrix.GetLength(1)];
                result.Supply = new int[supply.Length + 1];
                result.Demand = demand.Clone() as int[];

                CopyMatrix(initMatrix, result.Matrix);
                CopyArray(supply, result.Supply);

                result.Supply[result.Supply.Length - 1] = demandSum - supplySum;

            }

            return result;
        }
        public static void PrintMatrixColored(int[,] matrix)
        {
            var consoleBackgroundColor = Console.BackgroundColor;
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                var row = "";
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    if (matrix[i, j] != 0) Console.BackgroundColor = ConsoleColor.DarkBlue;
                    Console.Write(matrix[i, j] + "\t");
                    Console.BackgroundColor = consoleBackgroundColor;
                    Console.Write("|");
                }
                
                Console.WriteLine("\n__________________________________________");
            }
            Console.WriteLine();
        }
        public static void PrintMatrix(int[,] matrix)
        {          
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                var row = "";
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    row += matrix[i, j] + "\t|";
                }
                Console.WriteLine(row);
                Console.WriteLine("__________________________________________");
               
            }
            Console.WriteLine();         
        }
        public static void PrintArray(int[] array)
        {
            Console.WriteLine(string.Join(" ", array));
            Console.WriteLine();
        }
        private static void CopyMatrix(int[,] from, int[,] to)
        {
            for (int i = 0; i < from.GetLength(0); i++)
                for (int j = 0; j < from.GetLength(1); j++)
                    to[i, j] = from[i, j];
        }
        private static void CopyArray(int[] from, int[] to)
        {
            Array.Copy(from, to, from.Length);
        }
        public static void PdfPrintMatrix(Document doc, Pack init)
        {
          
            PdfPTable table = new PdfPTable(init.Matrix.GetLength(1) + 1);
         
            for (int i = 0; i < init.Matrix.GetLength(0); i++)
            {
              
                for (int j = 0; j < init.Matrix.GetLength(1); j++)
                {
                    table.AddCell(new PdfPCell
                    {
                        Phrase = new Phrase(init.Matrix[i, j].ToString()),
                        HorizontalAlignment = 1
                    });
                }
                table.AddCell(new PdfPCell
                {
                    Phrase = new Phrase(init.Supply[i].ToString()),
                    HorizontalAlignment = 1
                });

            }
            for (int i = 0; i < init.Demand.Length; i++)
                table.AddCell(new PdfPCell
                {
                    Phrase = new Phrase(init.Demand[i].ToString()),
                    HorizontalAlignment = 1,
                    
                });

            doc.Add(table);
         

        }
        public static void PdfPrintMatrix(Document doc, string[,] matrix)
        {

            PdfPTable table = new PdfPTable(matrix.GetLength(1));

            for (int i = 0; i < matrix.GetLength(0); i++)
            {

                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    var k = matrix[i, j].ToString();
                    table.AddCell(new PdfPCell
                    {
                        Phrase = new Phrase(k),
                        HorizontalAlignment = 1
                    });
                }

            }
            doc.Add(table);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var init = new Pack
            {
                Supply = new int[] { 500, 500, 720, 480 },
                Demand = new int[] { 456, 848, 224, 464, 240 },

                Matrix = new int[,]{
                                    { 5,4,5,8,7 },
                                    { 8,10,14,7,6 },
                                    { 9,13,9,8,6},
                                    { 15,4,11,15,14 }
                }
            };


            //CreatePdf(init);
            var closed = Solver.CloseMatrix(init.Matrix, init.Supply, init.Demand);

            Wrapper("Initial matrix", init, null);
            Wrapper("Closed matrix", init, Solver.CloseMatrix);
            Wrapper("Northwest method solution", closed, Solver.NorthWestMethod);
            Wrapper("Minimal element solution", closed, Solver.MinimalElementMethod);
            Wrapper("Vogel's method", closed, Solver.VogelsMethod);

            Solver.PotentialsMethod(closed, Solver.NorthWestMethod(closed.Matrix, closed.Supply, closed.Demand));


            Console.ReadKey();
        }

        //private static void CreatePdf(Pack init)
        //{
        //    Document doc = new Document();

        //    PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream("file1.pdf", FileMode.Create));
        //    doc.Open();
        //    doc.Add(new Paragraph("Task"));
        //    Solver.PdfPrintMatrix(doc, init);

        //    //closed
        //    doc.Add(new Paragraph("Closed matrix"));
        //    var closed = Solver.CloseMatrix(init.Matrix, init.Supply, init.Demand);
        //    Solver.PdfPrintMatrix(doc, closed);
        //    //northwest method
        //    doc.Add(new Paragraph("Northwest method"));

        //    string[,] result = null;
        //    Solver.NorthWestMethod(closed.Matrix, closed.Supply, closed.Demand,ref result);
        //    Solver.PdfPrintMatrix(doc, result);
        //    //min elem method


        //    //vogel's method
        //    doc.Close();

        //}

        static void Wrapper(string title, Pack init, Func<int[,], int[], int[], Pack> SolveMethod)
        {          
            Console.WriteLine("\t\t\t" + title + "\n");
            Pack result = null;

            if (SolveMethod != null)
                result = SolveMethod(init.Matrix, init.Supply, init.Demand);
            if (SolveMethod == null)
                result = new Pack
                {
                    Demand = init.Demand.Clone() as int[],
                    Supply = init.Supply.Clone() as int[],
                    Matrix = init.Matrix.Clone() as int[,]
                };

            Console.WriteLine("Matrix:");
            Solver.PrintMatrixColored(result.Matrix);

            Console.WriteLine("Demand:");
            Solver.PrintArray(result.Demand);

            Console.WriteLine("Supply:");
            Solver.PrintArray(result.Supply);
        }
    }
}
