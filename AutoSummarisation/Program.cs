using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoSummarisation
{
    class Program
    {
        //KEY: word, VALUE: count
        static Dictionary<string, int> keywordsWithItsCount = new Dictionary<string, int>();
        static List<String> sentenceList = new List<string>();
        static List<List<String>> wordArrayInWholeSentence = new List<List<string>>();

        //stopwords - stopwordsList
        static List<string> stopwordsList = new List<string>();

        static void Main(string[] args)
        {
            displayHelp();

            #region Define default values, 
            string filename_in = "file.txt";
            string filename_out = "out.txt";
            float SF = 50f;
            string filename_stopwords = "stopwords.txt";
            #endregion

            #region Input prompting
            while (true)
            {
                string argsline = Console.ReadLine();

                if (argsline.ToUpper() == "Q")
                    return;
                if (argsline.ToUpper() == "H")
                {
                    displayHelp();
                    continue;
                }

                args = argsline.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                if (args.Length != 2 && args.Length != 4 && args.Length != 6)
                {
                    display("Invalid parameters.");
                    continue;
                }



                switch (args.Length)
                {
                    case 0:
                        break;
                    case 2:
                        if (args[0] == "-i")
                            filename_in = args[1];
                        else if (args[0] == "-s")
                            filename_stopwords = args[1];
                        else if (args[0] == "-f")
                            SF = float.Parse(args[1]);
                        else
                        {
                            displayHelp();
                            continue;
                        }

                        break;
                    case 4:
                        if (args[0] == "-i" && args[2] == "-s")
                        {
                            filename_in = args[1];
                            filename_stopwords = args[3];
                        }
                        else if (args[0] == "-s" && args[2] == "-i")
                        {
                            filename_in = args[3];
                            filename_stopwords = args[1];
                        }
                        else
                        {
                            displayHelp();
                            continue;
                        }
                        break;
                    case 6:
                        int index = 0;
                        foreach (string arg in new string[] { args[0], args[2], args[4] })
                        {
                            switch (arg)
                            {
                                case "-i":
                                    filename_in = args[index + 1];
                                    break;
                                case "-s":
                                    filename_stopwords = args[index + 1];
                                    break;
                                case "-f":
                                    SF = float.Parse(args[index + 1]);
                                    break;
                                default:
                                    displayHelp();
                                    return;
                            }
                            index++;
                        }
                        continue;
                    default:
                        displayHelp();
                        continue;
                }
                break;
            }
            #endregion

            #region Display info user typed
            display("infile name : " + filename_in);
            display("stopwords filename : " + filename_stopwords);
            #endregion

            #region Check files exist, or exit program
            if (!File.Exists(filename_in) || !File.Exists(filename_stopwords))
            {
                display("No exist file.");
                Console.ReadKey();
                return;
            }
            #endregion
            ////////////////////////////////////////////////////////////////////////////
            //              
            //  MAIN PART   
            //
            ////////////////////////////////////////////////////////////////////////////

            #region Define variables for exploiding texts to every sentences and every words
            char[] del_for_words = " ,./<>?;\':\"[]{}`~!@#$%^&*()_+|\\=-——".ToArray();
            string[] del_for_sentences = { ". " };
            //
            //
            //
            string tempStr = "";
            FileStream fs = null;
            StreamReader sr = null;
            #endregion

            #region Read stopwords file

            fs = new FileStream(filename_stopwords, FileMode.Open);
            sr = new StreamReader(fs);
            // read file, and replace "New Line" String to SPACE
            tempStr = sr.ReadToEnd().Replace("\n", " ").Replace("\r", " ");
            // exploide the all stopwords text to each words, by just SPACE.
            stopwordsList.AddRange(tempStr.Split(new string[]{" "}, StringSplitOptions.RemoveEmptyEntries).Distinct());
            // LINQ, make sure all stopwords to Lower letters
            stopwordsList = stopwordsList.Select(x => x.ToLower()).ToList();
            
            #endregion

            #region Read text file

            fs = new FileStream(filename_in, FileMode.Open);
            sr = new StreamReader(fs);
            tempStr = sr.ReadToEnd().Replace("\n", " ").Replace("\r", " ");
            // exploide the all stopwords text to each words, by just del_for_sentences.
            sentenceList.AddRange(tempStr.Split(del_for_sentences, StringSplitOptions.RemoveEmptyEntries));
            //
            // exploiding every sentence to every words, looping sentence list 
            // so make the List array like as following:
            // { ["I", "am", "a", "boy"], 
            //   ...
            //   ["you", "are", "a", "girl"]
            //   ...
            //  }
            //
            foreach (string sentence in sentenceList)
            {
                wordArrayInWholeSentence.Add(sentence.Split(del_for_words, StringSplitOptions.RemoveEmptyEntries).Select(x => x.ToLower()).ToList());
            }
            #endregion

            #region Count Whold Word Occurrence
            List<string> wholeWordsList = new List<string>();
            //
            // exploiding all text to every words, like as following:
            // ["hello", "I", "go", "to", "school", "now", "what" "are", "you", "doing", "please", "hurry", ...] 
            //
            wholeWordsList.AddRange(tempStr.Split(del_for_words, StringSplitOptions.RemoveEmptyEntries));
            //
            // remove stopwords, will be removed "I" "to" "now" "what" "are" ...etc
            //
            wholeWordsList.RemoveAll(containsString);
            //
            // create dictionary [KEY, VALUE] = [word, count], like as following:
            // {["google", 2]
            //  ["alice", 15]
            //  ["train", 13]
            //  ["function", 7] ...}
            keywordsWithItsCount = wholeWordsList.GroupBy(x => x.ToLower()).ToDictionary(g => g.Key, g => g.Count());

            // sort this dictionary by value (count of occurrence), then like as following:
            // {["alice", 15]
            //  ["train", 13]
            //  ["function", 7]
            //  ["google", 2] ...}
            keywordsWithItsCount = keywordsWithItsCount.OrderByDescending(pair => pair.Value).ToDictionary(x => x.Key, x => x.Value);
            #endregion

            #region Print the count of occurrence of every words
            //
            // print, like as following:
            // alice (15)
            // bob (13)
            // ...
            foreach (string word in keywordsWithItsCount.Keys)
            {
                Console.WriteLine(word + " (" + keywordsWithItsCount[word] + ")");
            }
            #endregion

            #region Some variables initializing for summarizing
            List<List<string>> outSentences = new List<List<string>>();
            int summarizedWordLength = 0;
            int inputWordLength = wholeWordsList.Count;
            float sf = 0f;
            List<int> removedIdList = new List<int>();
            #endregion

            #region Summarizing....(MAIN LOGIC)
            //
            // looping all [word, count] Dictionary
            //
            foreach (KeyValuePair<string, int> keyword in keywordsWithItsCount)
            {
                int maxOccurrence = 0;
                int id = 0;
                string maxOccSentence = "";
                int maxId = 0;
                //
                // looping sentence with word array
                //
                foreach (List<string> wordArray in wordArrayInWholeSentence)
                {
                    //
                    // get count of the occurence of keyword in the sentence
                    //
                    int count = wordArray.Count(x => x == keyword.Key);
                    //
                    // save sentence that has max occurrence.
                    //
                    if (count > maxOccurrence)
                    {
                        maxOccSentence = sentenceList[id];
                        maxId = id;
                        maxOccurrence = count;
                    }
                    id++;
                }
                //
                // if saved, remove it from sentence list, and add it to outsentence list.
                //
                if (maxOccurrence > 0)
                {
                    List<string> totalStatistics = new List<string>(); //result to be showed us later
                    //
                    // just check the removed sentences ID
                    //
                    int removedId = maxId;
                    foreach (int alreadyId in removedIdList)
                    {
                        if (removedId >= alreadyId) //index
                            removedId++;
                    }
                    removedIdList.Add(removedId);
                    removedIdList.Sort();
                    //
                    // add the summarized length of key
                    //
                    summarizedWordLength += keywordsWithItsCount[keyword.Key];
                    //
                    // check the SF exceeds
                    //
                    if (100f * summarizedWordLength / keywordsWithItsCount.Keys.Count >= SF)
                    {
                        break;
                    }
                    sf = 100f * summarizedWordLength / keywordsWithItsCount.Keys.Count;
                    //
                    // save the result
                    //
                    totalStatistics.AddRange(new string[] { maxOccurrence.ToString(), keyword.Key, maxOccSentence, removedId.ToString() });
                    outSentences.Add(totalStatistics);
                    //
                    // remove the sentence that was summarized.
                    //
                    wordArrayInWholeSentence.RemoveAt(maxId);
                    sentenceList.RemoveAt(maxId);

                }

            }
            #endregion

            #region Display the result, just outSentences
            List<string> summay = new List<string>();
            foreach (List<string> ans in outSentences)
            {
                summay.Add(string.Join(",", ans));
                display(
                    ans[0] + " : " +
                    ans[1].PadRight(13) + " : " +
                    ans[2].Substring(0, Math.Min(50, ans[2].Length)) + "... (" +
                    ans[3] + ")");
            }

            display("actual Summ Factore = " + sf);
            #endregion

            #region Save the reult to the file 
            System.IO.File.WriteAllLines(filename_out, summay);
            display("saved in file : " + filename_out);
            #endregion

            Console.ReadKey();
        }
        private static bool containsString(string str)
        {
            return stopwordsList.Contains(str.ToLower());
        }

        private static void displayHelp()
        {
            display("   ========================================================================");
            display("   |  [-i <text file name>] [-f <sf value>] [-s <stopwords filename>]     |");
            display("   |  Q / q : quit,    H / h : Usage                                      |");
            display("   ========================================================================");
        }

        private static void display(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}