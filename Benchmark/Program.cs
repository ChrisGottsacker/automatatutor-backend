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
            //String csv = "../../../../automatatutor-data/dfa/simple.csv";
            String csv = "../../../../automatatutor-data/dfa/dfa1.csv";
            using (StreamReader textReader = new StreamReader(csv))
            {
                System.IO.StreamWriter stats = new System.IO.StreamWriter("../../../../misc/stats-VS.csv");
                stats.WriteLine(
                    "id," +
                    "description," +
                    "running time (ms)," +
                    "num. changes," +
                    "num. student states," +
                    "num. teacher states," +
                    "num. characters"
                );

                String[] automata = readCSV(textReader);

                int exampleId = 0; // used to uniquely identify a student-teacher pair

                IEnumerator enumerator = automata.GetEnumerator();
                char[] trimmedCharacters = {'"'};
                bool hasNext = enumerator.MoveNext();
                while (hasNext)
                {
                    try
                    {
                        String description = ((String)enumerator.Current).Trim().Trim(trimmedCharacters);
                        enumerator.MoveNext();

                        String correctDFAXML = ((String)enumerator.Current).Trim().Trim(trimmedCharacters);
                        enumerator.MoveNext();

                        String attemptDFAXML = ((String)enumerator.Current).Trim().Trim(trimmedCharacters);
                        hasNext = enumerator.MoveNext();

                        // DEBUGGING
                        Console.Write("\nDESCRIPTION of example id " + exampleId + "\n");
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
                            Console.Write("\nFAILED PARSING CORRECT XML: \n>>>>>>>" + correctDFAXML + "<<<<<<<<<\n");
                            throw new Exception();
                        }

                        try
                        {
                            dfaAttemptDesc = XElement.Parse(attemptDFAXML);
                        }
                        catch (Exception)
                        {
                            Console.Write("\nFAILED PARSING CORRECT XML: \n>>>>>>>" + attemptDFAXML + "<<<<<<<<<\n");
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

                        // time how long it takes to compute the edit distance
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();
                        var feedback = DFAGrading.GetGrade(dfaCorrectPair.Second, dfaAttemptPair.Second, alphabet, solver, 2000, 100, FeedbackLevel.Minimal, true, false, false);
                        stopwatch.Stop();
                        Console.Write("\n" + stopwatch.Elapsed.Milliseconds);

                        bool attemptIsCorrect = false;
                        int numChanges = 0;
                        foreach (var feed in feedback.Second)
                        {
                            // a really stupid way to find out if the student DFA is equivalent to the correct DFA
                            try
                            {
                                if ( attemptIsCorrect = ((StringFeedback)feed).ToString().Equals("CORRECT!!")) { break; }
                            }
                            catch (Exception e) { }

                            try
                            {
                                numChanges = ((AutomataPDL.DFAEDFeedback)feed).getCount();
                                break;
                            }
                            catch (Exception e) { }
                        }

                        if (!attemptIsCorrect)
                        {
                            // write line to file
                            stats.WriteLine(
                                exampleId + "," +
                                description + "," +
                                stopwatch.Elapsed.Milliseconds + "," +
                                numChanges + "," +
                                dfaAttemptPair.Second.StateCount + "," +
                                dfaCorrectPair.Second.StateCount + "," +
                                alphabet.Count
                            );
                        }
                        exampleId++;
                    }
                    catch (IndexOutOfRangeException)
                    {

                    }

                    Console.Write("==================================================================\n");


                }

                // just so Visual Studio keeps the Console open
                Console.Read();

            }

           
        }

        static String[] readCSV(StreamReader reader)
        {
            //char[] delimiters = {',', '"'};
            String[] delimiters = {",", "\"\n\""};
            String[] automata = reader.ReadToEnd().Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            
            return automata;
        }
    }
}