using AutomataPDL;
using System;

using Microsoft.Automata;
using System.Xml.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Diagnostics;

namespace Timing
{
    class Program
    {
        static void Main(string[] args)
        {
            String path = "../../../../automatatutor-data/dfa/";
            //Console.WriteLine("Enter the name of csv file in " + path + " containing automata: ");
            //String csv = path + Console.ReadLine();
            String csv = "../../../../automatatutor-data/dfa/dfa1.csv";
            using (StreamReader textReader = new StreamReader(csv))
            {
                String[] automata = readCSV(textReader);

                
                IEnumerator enumerator = automata.GetEnumerator();
                bool done = false;
                enumerator.MoveNext();
                while (!done)
                {
                    try
                    {
                        skipWhitespace(enumerator);

                        String description = ((String)enumerator.Current).Trim();

                        enumerator.MoveNext();
                        skipWhitespace(enumerator);

                        String correctDFAXML = ((String)enumerator.Current).Trim();

                        enumerator.MoveNext();
                        skipWhitespace(enumerator);

                        String attemptDFAXML = ((String)enumerator.Current).Trim();

                        enumerator.MoveNext();
                        done = skipWhitespace(enumerator);

                        // DEBUGGING
                        Console.Write("\nDESCRIPTION \n");
                        Console.Write(description);
                        //Console.Write("\nCORRECT XML \n");
                        //Console.Write(correctDFAXML);
                        //Console.Write("\nATTEMPT XML \n");
                        //Console.Write(attemptDFAXML);

                        CharSetSolver solver = new CharSetSolver(BitWidth.BV64);

                        XElement dfaCorrectDesc = null;
                        XElement dfaAttemptDesc = null;
                        try
                        {
                            dfaCorrectDesc = XElement.Parse(correctDFAXML);
                        }
                        catch (Exception)
                        {
                            Console.Write("\nFAILED PARSING: \n\"" + correctDFAXML + "\"\n");
                            throw new Exception();
                        }

                        try
                        {
                            dfaAttemptDesc = XElement.Parse(attemptDFAXML);
                        }
                        catch (Exception)
                        {
                            Console.Write("\nFAILED PARSING: \n\"" + attemptDFAXML + "\"\n");
                            throw new Exception();
                        }

                        //Read input 
                        var dfaCorrectPair = DFAUtilities.parseDFAFromXML(dfaCorrectDesc, solver);
                        var dfaAttemptPair = DFAUtilities.parseDFAFromXML(dfaAttemptDesc, solver);
                        if (!dfaCorrectPair.First.SetEquals(dfaAttemptPair.First))
                        {
                            throw new System.ArgumentException();
                        }
                        var alphabet = dfaCorrectPair.First;
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();
                        DFAGrading.GetGrade(dfaCorrectPair.Second, dfaAttemptPair.Second, alphabet, solver, 2000, 100, FeedbackLevel.Minimal, true, false, false);
                        stopwatch.Stop();
                        Console.Write("\n" + stopwatch.Elapsed.Milliseconds);
                    }
                    catch (IndexOutOfRangeException)
                    {

                    }

                    Console.Write("==================================================================\n");


                }

                Console.Read();

            }

           
        }

        static String[] readCSV(StreamReader reader)
        {
            char[] delimiters = {',', '"'};
            String[] automata = reader.ReadToEnd().Split(delimiters);
            
            return automata;
        }

        static bool skipWhitespace(IEnumerator enumerator)
        {
            // let calling method know if end of input was reached
            bool outOfInput = false;
            while ( String.IsNullOrWhiteSpace(((String)enumerator.Current)) )
            {
                if (!enumerator.MoveNext())
                {
                    outOfInput = true;
                    break;
                }
            }
            //Console.Write(">>>>>>" + ((String)enumerator.Current).Trim() + "<<<<<<<<<");
            return outOfInput;
        }
    }
}