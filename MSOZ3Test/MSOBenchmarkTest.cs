﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Microsoft.Automata;
using Microsoft.Automata.Z3;
using Microsoft.Z3;
using MSOZ3;
using System.Diagnostics;

using SFAz3 = Microsoft.Automata.SFA<Microsoft.Z3.FuncDecl, Microsoft.Z3.Expr, Microsoft.Z3.Sort>;
using STz3 = Microsoft.Automata.ST<Microsoft.Z3.FuncDecl, Microsoft.Z3.Expr, Microsoft.Z3.Sort>;
using Rulez3 = Microsoft.Automata.Rule<Microsoft.Z3.Expr>;
using STBuilderZ3 = Microsoft.Automata.STBuilder<Microsoft.Z3.FuncDecl, Microsoft.Z3.Expr, Microsoft.Z3.Sort>;
using System.IO;

namespace PDLTest
{
    [TestClass]
    public class MSOBenchmarkTest
    {

        [TestMethod]
        public void NElements()
        {
            var solver = new CharSetSolver(BitWidth.BV64);  //new solver using ASCII encoding

            //ex1 p1,p2,p3,p4,p5:
            //    p1<p2 & p2<p3 & p3<p4 & p4<p5 &
            //    A = {p1,p2,p3,p4,p5};

            List<char> alph = new List<char> { };
            HashSet<char> al = new HashSet<char>(alph);
            StringBuilder sb = new StringBuilder();

            Stopwatch timer = new Stopwatch();
            int nestedquant = 20;
            int test_length = nestedquant + 1;
            for (int i = 2; i < test_length; i++)
            {
                MSOFormula formula = new MSOLess("p1", "p2");
                for (int j = 2; j < i; j++)
                {
                    formula = new MSOAnd(formula, new MSOLess("p" + j, "p" + (j + 1)));
                }
                for (int j = 1; j <= i; j++)
                {
                    formula = new MSOExistsFO("p" + j, formula);
                }
                //var WS1S = formula.ToWS1S(solver);

                Assert.IsTrue(formula.CheckUseOfVars());
                //if (i > 0)
                //{
                //    var dfa = WS1S.getDFA(al, solver);
                //    var acc = 0L;
                //    int tt = 25;
                //    for (int k = 0; k < tt; k++)
                //    {
                //        timer.Reset();
                //        timer.Start();
                //        dfa = WS1S.getDFA(al, solver);
                //        timer.Stop();
                //        acc += timer.ElapsedMilliseconds;
                //    }
                //    Console.WriteLine(i + " vars: " + acc / tt + " ms");
                //    //var test = solver.Convert(@"^(a){" + i + @",}$");

                //    //Assert.IsTrue(dfa.IsEquivalentWith(test, solver));

                //    //string file = "../../../PDLTest/DotFiles/el" + i;
                //    //solver.SaveAsDot(dfa, "aut", file);
                //}
                runFormula(formula, solver.True, solver, sb, 1);

            }

            Console.WriteLine(sb);
        }

        static int c = 1;
        private static void runFormula(MSOFormula phi, BvSet al, CharSetSolver solver, StringBuilder sb, int maxc)
        {
            Stopwatch sw = new Stopwatch();

            //Run mine
            var dfa = phi.getDFA(al, solver);
            solver.SaveAsDot(dfa, "c" + c, @"c:/temp/n" + c++);
            sw.Restart();
            for (int i = 0; i < maxc; i++)
            {
                dfa = phi.getDFA(al, solver);
            }
            sw.Stop();
            sb.AppendFormat("{0},", sw.ElapsedMilliseconds / maxc);

            string filepath = @"tmp.mona";
            System.IO.StreamWriter file = new System.IO.StreamWriter(filepath);
            file.WriteLine(phi.ToMonaString(new List<string>(), solver));
            file.Close();

            var psi = new ProcessStartInfo
            {
                FileName = @"c:/cygwin/home/lorisdan/Mona/mona.exe",
                Arguments = "" + filepath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };
            long time = 0;
            var to = false;
            for (int i = 0; i < maxc; i++)
            {
                var process = Process.Start(psi);
                var result = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                System.IO.StreamWriter ff = new System.IO.StreamWriter(@"c:/temp/out.txt");
                ff.WriteLine(result);
                ff.Close();
                //Console.WriteLine(result);
                result = result.Remove(result.Length - 1);
                var a = result.Split(':');
                if (a[a.Length - 1].Contains("***") || a[a.Length - 1].Contains("too"))
                {
                    to = true;
                    break;
                }
                else
                {
                    var b = a[a.Length - 6].Split('.', '\n');
                    var cs = int.Parse(b[1]);
                    var secs = int.Parse(b[0]);
                    time += 1000 * secs + cs * 10;
                }
            }

            sb.AppendFormat("{0}", to ? "TO" : (time / maxc).ToString());
            sb.AppendLine();
        }


    }
}