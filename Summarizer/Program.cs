using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Summarizer
{
    class Program
    {
        //KEY: word, VALUE: count
        static Dictionary<string, int> countOfWholeWords = new Dictionary<string, int>();
        static List<String> sentenceList = new List<string>();
        static List<List<String>> wordListInAllSentence = new List<List<string>>();

        //stopwords - stopwordsList
        static List<string> stopwordsList = new List<string>();


        static void Main(string[] args)
        {

            string filename_in = "infile.txt";
            string filename_stopwords = "stopwords.txt";

            float SF = 50f;
            while (true)
            {
                Console.WriteLine("Quit: Q ");
                try
                {
                    Console.Write("infilename: ");
                    string fstr = Console.ReadLine();
                    if (fstr!="")
                        filename_in = fstr;
                    else
                    {
                        filename_in = "infile.txt";
                    }
                    if (filename_in == "Q" || filename_in == "q") return;

                    Console.Write("SF: ");
                    string sfstr = Console.ReadLine();
                    if (sfstr == "Q" || sfstr == "q") return;

                    if (sfstr == "")
                    {
                        SF = 50f;
                    }
                    else
                    {
                        SF = float.Parse(sfstr);

                    }
                        

                    if(File.Exists(filename_in) && File.Exists(filename_stopwords))
                        break;
                    else
                    {
                        Console.Write("No file exist: " + filename_in + " or " + filename_stopwords);
                    }
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

           
            string filename_out = "summary.txt";
            

            println("infile name : " + filename_in);
            println("stopwords filename : " + filename_stopwords);

            char[] del_for_words = " ,./<>?;\':\"[]{}`~!@#$%^&*()_+|\\=-1234567890".ToArray();
            string[] del_for_sentences = { ". " };

            //
            string tempStr = "";
            FileStream fs = null;
            StreamReader sr = null;
            fs = new FileStream(filename_stopwords, FileMode.Open);
            sr = new StreamReader(fs);
            tempStr = sr.ReadToEnd().Replace("\n", " ").Replace("\r", " ");
            stopwordsList.AddRange(tempStr.Split(del_for_words, StringSplitOptions.RemoveEmptyEntries).Distinct());
            stopwordsList = stopwordsList.Select(x => x.ToLower()).ToList();

            //infile - sentenceList
            fs = new FileStream(filename_in, FileMode.Open);
            sr = new StreamReader(fs);
            tempStr = sr.ReadToEnd().Replace("\n", " ").Replace("\r", " ");
            sentenceList.AddRange(tempStr.Split(del_for_sentences, StringSplitOptions.RemoveEmptyEntries));
            //
            foreach (string sentence in sentenceList)
            {
                wordListInAllSentence.Add(sentence.Split(del_for_words, StringSplitOptions.RemoveEmptyEntries).Select(x => x.ToLower()).ToList());
            }

            //count of word
            List<string> wholeWordsList = new List<string>();
            wholeWordsList.AddRange(tempStr.Split(del_for_words, StringSplitOptions.RemoveEmptyEntries));
            wholeWordsList.RemoveAll(containsStopwords); //remove stopwords
            countOfWholeWords = wholeWordsList.GroupBy(x => x.ToLower()).ToDictionary(g => g.Key, g => g.Count());

            // sort wordCount dictionary by count
            countOfWholeWords = countOfWholeWords.OrderByDescending(pair => pair.Value).ToDictionary(x => x.Key, x => x.Value);

            List<List<string>> outSentences = new List<List<string>>();
            int summarizedWordLength = 0;
            int inputWordLength = wordListInAllSentence.Count;
            float sf = 0f;
            List<int> indexlist = new List<int>();
            //
            // looping all words
            //
            foreach (KeyValuePair<string, int> pair in countOfWholeWords)
            {
                Console.WriteLine("[{0}] : {1}", pair.Value, pair.Key);
            }
            foreach (KeyValuePair<string, int> pair in countOfWholeWords)
            {
                //Console.WriteLine("[{0}] : {1}", pair.Value, pair.Key);

                int maxcount = 0;
                int index = 0;
                string maxSentence = "";
                int maxIndex = 0;
                foreach (List<string> wordlist in wordListInAllSentence)
                {
                    int count = wordlist.Count(x => x == pair.Key);
                    if (count > maxcount)
                    {
                        maxSentence = sentenceList[index];
                        maxIndex = index;
                        maxcount = count;
                    }
                    index++;
                }
                if (maxcount > 0)
                {
                    summarizedWordLength += 1;
                    List<string> result = new List<string>();
                    int realIndex = maxIndex;
                    foreach (int alreadyId in indexlist)
                    {
                        if (realIndex >= alreadyId) //index
                            realIndex++;
                    }
                    indexlist.Add(realIndex);
                    indexlist.Sort();
                    
                    //summarizedWordLength += countOfWholeWords[pair.Key];
                    
                    //if (100f * summarizedWordLength / countOfWholeWords.Keys.Count >= SF)
                    //if (100f * summarizedWordLength / countOfWholeWords.Count >= SF)
                    {
                    //    break;
                    }
                    //sf = 100f * summarizedWordLength / countOfWholeWords.Keys.Count;

                    result.AddRange(new string[] { maxcount.ToString(), pair.Key, maxSentence, realIndex.ToString()});
                    outSentences.Add(result);

                    wordListInAllSentence.RemoveAt(maxIndex);
                    sentenceList.RemoveAt(maxIndex);

                }


                sf = 100f * summarizedWordLength / inputWordLength;
                if (sf >= SF)
                {
                    break;
                }
                
            }

            List<string> summay = new List<string>();
            foreach (List<string> ans in outSentences)
            {
                summay.Add(string.Join(",", ans));
                println(
                    ans[0] + " : " + 
                    ans[1].PadRight(13) + " : " + 
                    ans[2].Substring(0, Math.Min(50, ans[2].Length)) + "... (" + 
                    ans[3]+")");
            }
            println("actual SF = " + sf.ToString("0.00"));
            
            //save summary result
            System.IO.File.WriteAllLines(filename_out, summay);
            println("saved in file : " + filename_out);

            Console.ReadKey();
        }
        private static bool containsStopwords(string str)
        {
            return stopwordsList.Contains(str.ToLower());
        }

        private static void printUsage()
        {
            println("   -----------------------------------------------------------------------------------------------------");
            println("   |                                                                                                    |");
            println("   |   Summarizer.exe [-in <text filename>] [-sf <summarization factor>] [-st <stopwords filename>]     |");
            println("   |                                                                                                    |");
            println("   -----------------------------------------------------------------------------------------------------");
            Console.ReadKey();
        }

        private static void println(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
