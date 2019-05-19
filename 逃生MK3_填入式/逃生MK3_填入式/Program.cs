using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace 逃生MK3_填入式
{
    class Program
    {
        struct Node
        {
            public double x, y;
            public double distance;
            public double totalW, basicW;    //總權重 ,基本權重 ,人數權重
            public double people;
            public double tempPeople1, tempPeople2;   //正要移動過來的人 1表要到達目標節點  2表剛離開原始節點
            public Boolean exit;
            public int targetExit;
            public Boolean moved;
            public int hop;
        }

        static int mapHeight = 6, mapLenght = 6;    //地圖size
        static int pathLimit = 3;   //限制通過人數
        static double totalPeople = 0;


        static void Main(string[] args)
        {
            int loopCounter = 0;
            int[] RCounter;
            
            Node[,] node = new Node[mapHeight, mapLenght];

            //輸入總人數 & 總次數
            Console.Write("輸入逃生次數:");
            loopCounter = Convert.ToInt32(Console.ReadLine());
            RCounter = new int[loopCounter];

            Console.Write("輸入地圖總人數:");
            totalPeople = Convert.ToInt32(Console.ReadLine());

            //地圖初始化
            setExit(node);
            showDis(node);

            ///////權重初始化
            //map
            initialBasicW(node);
            showBW(node);

            for (int i = 0; i < loopCounter; i++)
            {
                //初始各點人數
                setPeople(node, totalPeople);
                mathTotalW(node, totalPeople);
                showP(node);
                showTW(node);
                printStar();

                //開跑
                int tempRCounter = runningMan(node, totalPeople);
                RCounter[i] = tempRCounter;
                Console.WriteLine("第 {0} 輪共跑 {1} 回合", i + 1, tempRCounter);

                //output to csv
                write2CSV(RCounter, loopCounter, totalPeople);
            }





            //end program
            Console.WriteLine("\npress any key to continue......");
            Console.ReadLine();
        }

        //print star
        static void printStar()
        {
            for (int i = 0; i < 20; i++)
                Console.Write("*");
            Console.WriteLine();
        }

        //出口初始化
        static void setExit(Node[,] n)
        {
            for (int i = 0; i < n.GetLength(0); i++)
            {
                for (int j = 0; j < n.GetLength(1); j++)
                {
                    n[i, j].exit = false;
                }
            }

            //設定出口
            n[0, 0].exit = true;
            n[2, 5].exit = true;
            n[5, 2].exit = true;

            //設定最近出口距離
            for (int i = 0; i < n.GetLength(0); i++)
            {
                for (int j = 0; j < n.GetLength(1); j++)
                {
                    if (n[i, j].exit == false)
                    {
                        findMinExit(n, i, j);
                    }
                }
            }
        }

        //找最近出口 & 設定距離
        static void findMinExit(Node[,] n, int ni, int nj)
        {
            double minDis = double.MaxValue;

            for (int i = 0; i < n.GetLength(0); i++)
            {
                for (int j = 0; j < n.GetLength(1); j++)
                {
                    if (n[i, j].exit == true)
                    {
                        double dis = Math.Abs(ni - i) + (Math.Abs(nj - j) * 2);

                        if (dis < minDis)
                        {
                            minDis = dis;
                            n[ni, nj].x = Math.Abs(nj - j) * 2;
                            n[ni, nj].y = Math.Abs(ni - i);
                            n[ni, nj].distance = dis;
                        }
                    }
                }
            }
        }

        //show 各點距離
        static void showDis(Node[,] n)
        {
            Console.WriteLine("All Node distance:");

            for (int i = 0; i < n.GetLength(0); i++)
            {
                for (int j = 0; j < n.GetLength(1); j++)
                {
                    if (n[i, j].exit == true)
                        Console.ForegroundColor = ConsoleColor.Green;
                    else
                        Console.ForegroundColor = ConsoleColor.Yellow;

                    Console.Write("[{0},{1}]\t", n[i, j].x, n[i, j].y);
                }
                Console.WriteLine();
            }

            Console.ForegroundColor = ConsoleColor.White;
        }

        //show 基本權重
        static void showBW(Node[,] n)
        {
            //距離加總
            double totalDis = 0;
            for (int i = 0; i < n.GetLength(0); i++)
            {
                for (int j = 0; j < n.GetLength(1); j++)
                {
                    totalDis += n[i, j].distance;
                }
            }

            Console.WriteLine("\nBasicW:");
            Console.WriteLine("總距離:{0}", totalDis);

            for (int i = 0; i < n.GetLength(0); i++)
            {
                for (int j = 0; j < n.GetLength(1); j++)
                {
                    if (n[i, j].exit == true)
                        Console.ForegroundColor = ConsoleColor.Green;
                    else
                        Console.ForegroundColor = ConsoleColor.Yellow;

                    Console.Write("{0:F2}\t", n[i, j].basicW);
                }
                Console.WriteLine();
            }

            Console.ForegroundColor = ConsoleColor.White;
        }

        //找最近的點加權重
        static int findNode2AddBW(Node[,] n, int ni, int nj, double totalDis)
        {
            double minW = int.MaxValue;
            int minI = 0, minJ = 0;

            //上
            if (ni - 1 >= 0 && n[ni - 1, nj].basicW < minW)
            {
                minW = n[ni - 1, nj].basicW;
                minI = ni - 1;
                minJ = nj;
            }

            //下
            if (ni + 1 < n.GetLength(0) && n[ni + 1, nj].basicW < minW)
            {
                minW = n[ni + 1, nj].basicW;
                minI = ni + 1;
                minJ = nj;
            }

            //左
            if (nj - 1 >= 0 && n[ni, nj - 1].basicW < minW)
            {
                minW = n[ni, nj - 1].basicW;
                minI = ni;
                minJ = nj - 1;
            }

            //右
            if (nj + 1 < n.GetLength(1) && n[ni, nj + 1].basicW < minW)
            {
                minW = n[ni, nj + 1].basicW;
                minI = ni;
                minJ = nj + 1;
            }

            double newW = minW + ((Math.Abs(ni - minI) + Math.Abs(nj - minJ) * 2) / totalDis) * (n[minI, minJ].hop + 1);

            if (newW == n[ni, nj].basicW)
                return 0;
            else
            {
                n[ni, nj].basicW = newW;
                n[ni, nj].hop = n[minI, minJ].hop + 1;
                return 1;
            }
        }

        //初始基本權重
        static void initialBasicW(Node[,] n)
        {
            //距離加總
            double totalDis = 0;
            for (int i = 0; i < n.GetLength(0); i++)
            {
                for (int j = 0; j < n.GetLength(1); j++)
                {
                    n[i, j].totalW = 0;
                    n[i, j].basicW = double.MaxValue;
                    n[i, j].people = 0;
                    n[i, j].hop = 0;
                    totalDis += n[i, j].distance;

                    if (n[i, j].exit == true)
                        n[i, j].basicW = 0;
                }
            }


            while (true)
            {
                int initialCounter = 0;

                for (int i = 0; i < n.GetLength(0); i++)
                {
                    for (int j = 0; j < n.GetLength(1); j++)
                    {
                        if (n[i, j].exit == true)
                            continue;
                        else
                            initialCounter += findNode2AddBW(n, i, j, totalDis);
                    }
                }

                if (initialCounter == 0)
                    break;
            }
        }

        //初始人數
        static void setPeople(Node[,] n, double totalPeople)
        {
            Random rand = new Random(Guid.NewGuid().GetHashCode());

            while (totalPeople > 0)
            {
                int randI = rand.Next(0, n.GetLength(0));
                int randJ = rand.Next(0, n.GetLength(1));

                if (n[randI, randJ].exit == false)
                {
                    n[randI, randJ].people++;
                    totalPeople--;
                }
            }
        }

        //show 各點人數
        static void showP(Node[,] n)
        {
            Console.WriteLine("\n人數:");
            for (int i = 0; i < n.GetLength(0); i++)
            {
                for (int j = 0; j < n.GetLength(1); j++)
                {
                    if (n[i, j].exit == true)
                        Console.ForegroundColor = ConsoleColor.Green;
                    else
                        Console.ForegroundColor = ConsoleColor.Yellow;

                    Console.Write("{0}\\{1}\\{2}\t", n[i, j].people, n[i, j].tempPeople1, n[i, j].tempPeople2);
                }
                Console.WriteLine();
            }

            Console.ForegroundColor = ConsoleColor.White;
        }

        //show 總加權
        static void showTW(Node[,] n)
        {
            Console.WriteLine("\n總加權:");
            for (int i = 0; i < n.GetLength(0); i++)
            {
                for (int j = 0; j < n.GetLength(1); j++)
                {
                    if (n[i, j].exit == true)
                        Console.ForegroundColor = ConsoleColor.Green;
                    else
                        Console.ForegroundColor = ConsoleColor.Yellow;

                    Console.Write("{0:F2}\t", n[i, j].totalW);
                }
                Console.WriteLine();
            }

            Console.ForegroundColor = ConsoleColor.White;
        }

        //總加權
        static void mathTotalW(Node[,] n, double totalPeo)
        {
            for (int i = 0; i < n.GetLength(0); i++)
            {
                for (int j = 0; j < n.GetLength(1); j++)
                {
                    if (n[i, j].exit == true)
                    {
                        n[i, j].totalW = 0;
                        continue;
                    }
                    else
                        n[i, j].totalW = n[i, j].basicW + (n[i, j].people + n[i, j].tempPeople1 + n[i, j].tempPeople2) / totalPeo;
                }
            }
        }

        //各點初始整理
        static void initialNode(Node[,] n)
        {
            for (int i = 0; i < n.GetLength(0); i++)
            {
                for (int j = 0; j < n.GetLength(1); j++)
                {
                    n[i, j].people += n[i, j].tempPeople1;
                    n[i, j].tempPeople1 = n[i, j].tempPeople2;
                    n[i, j].tempPeople2 = 0;
                    n[i, j].moved = false;

                    if (n[i, j].exit == true || n[i, j].people == 0)
                    {
                        n[i, j].people = 0;
                        n[i, j].moved = true;
                    }
                }
            }
        }

        //單點總加權
        static void mathSingleNodeTW(Node[,] n,int ni,int nj)
        {
            n[ni, nj].totalW = n[ni, nj].basicW + (n[ni, nj].people + n[ni, nj].tempPeople1 + n[ni, nj].tempPeople2) / totalPeople;
        }

        //找出路
        static void findWayOut(Node[,] n, int ni, int nj)
        {
            int[] limitCounter = new int[4];

            while(true)
            {
                Boolean endWhile = true;
                double minW = n[ni, nj].totalW;
                int minI = 0, minJ = 0, minWay = -1;

                //上
                if (ni - 1 >= 0 && n[ni - 1, nj].totalW < minW && limitCounter[0]<pathLimit)
                {
                    minW = n[ni - 1, nj].totalW;
                    minI = ni - 1;
                    minJ = nj;
                    minWay = 0;
                }

                //下
                if (ni + 1 < n.GetLength(0) && n[ni + 1, nj].totalW < minW && limitCounter[1]<pathLimit)
                {
                    minW = n[ni + 1, nj].totalW;
                    minI = ni + 1;
                    minJ = nj;
                    minWay = 1;
                }

                //左
                if (nj - 1 >= 0 && n[ni, nj - 1].totalW < minW && limitCounter[2]<pathLimit)
                {
                    minW = n[ni, nj - 1].totalW;
                    minI = ni;
                    minJ = nj - 1;
                    minWay = 2;
                }

                //右
                if (nj + 1 < n.GetLength(1) && n[ni, nj + 1].totalW < minW && limitCounter[3]<pathLimit)
                {
                    minW = n[ni, nj + 1].totalW;
                    minI = ni;
                    minJ = nj + 1;
                    minWay = 3;
                }

                //////////////////////////////////
                if(minW<n[ni,nj].totalW)
                {
                    n[ni, nj].people--;
                    endWhile = false;
                    limitCounter[minWay]++;

                    if (minWay <= 1)
                        n[minI, minJ].tempPeople1++;
                    else if (minWay >= 2)
                        n[minI, minJ].tempPeople2++;

                    mathSingleNodeTW(n, minI, minJ);
                    mathSingleNodeTW(n, ni, nj);

                    if (n[minJ, minJ].exit == true)
                        n[minI, minJ].totalW = 0;
                }

                //離開迴圈
                if (endWhile == true || n[ni,nj].people==0)
                    break;
            }

        }

        //逃生啦
        static int runningMan(Node[,] n,double totalPeo)
        {
            int RoundCounter = 0;

            while (true)
            {
                RoundCounter++;
                initialNode(n);

                while (true)
                {

                    //從最小權重節點開始找
                    double minW = double.MaxValue;
                    int minI = 0, minJ = 0;

                    for (int i = 0; i < n.GetLength(0); i++)
                    {
                        for (int j = 0; j < n.GetLength(1); j++)
                        {
                            if (n[i, j].moved == false && n[i, j].totalW < minW)
                            {
                                minW = n[i, j].totalW;
                                minI = i;
                                minJ = j;
                            }
                        }
                    }
                    //////////////////////////////////////////////////////////////////

                    //分配出路
                    findWayOut(n, minI, minJ);
                    n[minI, minJ].moved = true;
                    mathTotalW(n, totalPeo);

                    //判斷離開迴圈
                    int movedCounter = 0;
                    for (int i = 0; i < n.GetLength(0); i++)
                    {
                        for (int j = 0; j < n.GetLength(1); j++)
                            if (n[i, j].moved == false)
                                movedCounter++;
                    }

                    if (movedCounter == 0)
                        break;
                }

                //離開迴圈
                double peopleCounter = 0;

                for (int i = 0; i < n.GetLength(0); i++)
                {
                    for (int j = 0; j < n.GetLength(1); j++)
                        peopleCounter += (n[i, j].people + n[i, j].tempPeople1 + n[i, j].tempPeople2);
                }

                //Console.ReadLine();
                //showTW(n);
                showP(n);

                if (peopleCounter == 0)
                    break;
            }

            return RoundCounter;
        }

        //寫入至csv
        static void write2CSV(int[] RC, int LC, double PC)
        {
            FileStream fs = new FileStream("逃生結果_填入式.csv", FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default);
            sw.WriteLine("總回數,總人數");
            sw.WriteLine("{0},{1}\n\n", LC, PC);

            sw.WriteLine("回數,逃生回合數");
            for (int i = 0; i < RC.GetLength(0); i++)
            {
                sw.WriteLine("{0},{1}", i + 1, RC[i]);
            }

            sw.Flush();
            sw.Close();
            fs.Close();
        }
    }
}
