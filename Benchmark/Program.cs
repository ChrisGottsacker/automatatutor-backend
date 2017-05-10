using AutomataPDL;
using System;

using Microsoft.Automata;
using System.Xml.Linq;
using System.IO;
using System.Text;
using System.Collections;

namespace Timing
{
    class Program
    {
        static void Main(string[] args)
        {
            // Console.WriteLine("Enter the path to a csv file containing automata: ");
            // String csv = Console.ReadLine();
            String csv = "../../../../automatatutor-data/dfa/simple.csv";
            using (StreamReader textReader = new StreamReader(csv))
            {
                String[] automata = readCSV(textReader);

                
                IEnumerator enumerator = automata.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    try
                    {
                        skipWhitespace(enumerator);


                        String description = (String)enumerator.Current;
                        enumerator.MoveNext();

                        if (!skipWhitespace(enumerator))
                        {
                            enumerator.MoveNext();
                        }

                        String correctDFAXML = (String)enumerator.Current;
                        enumerator.MoveNext();
                        if (!skipWhitespace(enumerator))
                        {
                            enumerator.MoveNext();
                        }

                        String attemptDFAXML = (String)enumerator.Current;

                        //Console.Write(description);
                        //Console.Write(correctDFAXML);
                        //Console.Write(attemptDFAXML);

                        CharSetSolver solver = new CharSetSolver(BitWidth.BV64);

                        XElement dfaCorrectDesc = XElement.Parse(correctDFAXML);
                        XElement dfaAttemptDesc = XElement.Parse(attemptDFAXML);

                        //Read input 
                        var dfaCorrectPair = DFAUtilities.parseDFAFromXML(dfaCorrectDesc, solver);
                        var dfaAttemptPair = DFAUtilities.parseDFAFromXML(dfaAttemptDesc, solver);
                        if (!dfaCorrectPair.First.SetEquals(dfaAttemptPair.First))
                        {
                            Console.WriteLine("Alphabets not the same, (or they are and this comparison doesn't make sense).");
                            throw new System.ArgumentException();
                        }
                        var alphabet = dfaCorrectPair.First;
                        DFAGrading.GetGrade(dfaCorrectPair.Second, dfaAttemptPair.Second, alphabet, solver, 2000, 100, FeedbackLevel.Minimal, true, false, false);
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
            bool skipped = false;
            while ( String.IsNullOrWhiteSpace(((String)enumerator.Current).Trim()) )
            {
                skipped = true;
                if (!enumerator.MoveNext())
                {
                    throw new IndexOutOfRangeException();
                }
            }
            //Console.Write(">>>>>>" + ((String)enumerator.Current).Trim() + "<<<<<<<<<");
            return skipped;
        }
    }
}