using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Autocomplete
{
    class Program
    {
        static void Main()
        { 
            ICollection<string> dictionary = new List<string>();
            ICollection<string> autocomplete = new List<string>();
            IDictionary<string, List<string>> acronyms = new Dictionary<string, List<string>>();

            bool isDictionaryFill = true;

            Console.WriteLine("Read input data");

            using (StreamReader reader = new StreamReader("Input.txt"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line != "===")
                    {
                        if (CheckLine(line))
                        {
                            if (isDictionaryFill)
                            {
                                string[] splitted = line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                                if (splitted.Length > 1)
                                {
                                    string acronym = "";

                                    foreach (string word in splitted)
                                    {
                                        acronym += word[0].ToString().ToLower();
                                    }

                                    if (acronyms.ContainsKey(acronym))
                                    {
                                        List<string> foundSentences;
                                        if (acronyms.TryGetValue(acronym, out foundSentences))
                                        {
                                            foundSentences.Add(line);
                                        }
                                    }
                                    else
                                    {
                                        acronyms.Add(new KeyValuePair<string, List<string>>(acronym, new List<string> { line }));
                                    }
                                    Console.WriteLine(line + " --> " + acronym);
                                }

                                dictionary.Add(line.ToLower());
                                Console.WriteLine(line);
                            }
                            else
                            {
                                if (line.Length >= 2)
                                {
                                    autocomplete.Add(line);
                                    Console.WriteLine(line);
                                }
                            }
                        }
                    }
                    else
                    {
                        isDictionaryFill = false;
                        Console.WriteLine("============");
                    }
                }
            }
            Console.WriteLine("Start working...");

            foreach (var autocompleteWord in autocomplete)
            {
                List<string> hits = new List<string>();

                bool isAcronym = !autocompleteWord.Contains(" ");

                bool isAllLetterCapitalAcronym = isAcronym && Regex.IsMatch(autocompleteWord, @"[A-Z]");

                if (isAllLetterCapitalAcronym)
                {
                    CheckAcronym(autocompleteWord.ToLower(), acronyms, ref hits);

                    if (hits.Count < 10)
                    {
                        CheckConsist(autocompleteWord.ToLower(), dictionary, ref hits);
                    }
                }
                else
                {
                    CheckConsist(autocompleteWord, dictionary, ref hits);

                    if (isAcronym && hits.Count < 10)
                    {
                        CheckAcronym(autocompleteWord, acronyms, ref hits);
                    }
                }

                if (hits.Any())
                {
                    using (StreamWriter outputFile = new StreamWriter("Output.txt"))
                    {
                        Console.WriteLine("[" + autocompleteWord + "]");
                        outputFile.WriteLine("[" + autocompleteWord + "]");

                        foreach (string hit in hits)
                        {
                            Console.WriteLine(hit);
                            outputFile.WriteLine(hit);
                        }
                    }
                }
            }
            Console.WriteLine("Press any key to continue");
            Console.ReadLine();
        }

        /// <summary>
        ///     Checking that the string consists of a Latin letter, and each word does not exceed 50 characters
        /// </summary>
        /// <param name="line">String to be validated</param>
        /// <returns>Flag indicating that the string has been validated</returns>
        private static bool CheckLine(string line)
        {
            string[] splitted = line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in splitted)
            {
                if (Regex.IsMatch(word, @"^[a-zA-Z]+$") && word.Length <= 50)
                {
                    continue;
                }
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Check for the presence of acronym in the dictionary
        /// </summary>
        /// <param name="acronym">Acronym, whose decoding we will look for</param>
        /// <param name="acronyms">Dictionary of all found acronyms</param>
        /// <param name="list">A list transferred by reference, in which all sentences associated with the acronym will be added</param>
        private static void CheckAcronym(string acronym, IDictionary<string, List<string>> acronyms, ref List<string> list)
        {
            if (list.Count < 10 && acronyms.ContainsKey(acronym))
            {
                List<string> foundSentences;
                if (acronyms.TryGetValue(acronym, out foundSentences))
                {
                    int i = 0;

                    while (list.Count < 10 && i < foundSentences.Count)
                    {
                        list.Add(foundSentences[i]);

                        i++;
                    }
                }
            }
        }

        /// <summary>
        ///     Check if the string is part of another string
        /// </summary>
        /// <param name="word"></param>
        /// <param name="dictionary">List of all correct input strings</param>
        /// <param name="list">A list transferred by reference, in which all words and sentences containing the word will be added</param>
        private static void CheckConsist(string word, ICollection<string> dictionary, ref List<string> list)
        {
            List<string> lowerPriority = new List<string>();

            foreach (var dictionaryWord in dictionary)
            {
                if (Regex.IsMatch(dictionaryWord, word))
                {
                    if (Regex.IsMatch(dictionaryWord, @"(?:^|\s)" + word))
                    {
                        list.Add(dictionaryWord);

                        if (list.Count == 10) break;
                    }
                    else
                    {
                        lowerPriority.Add(dictionaryWord);
                    }
                }
            }

            int i = 0;
            while (list.Count < 10 && i < lowerPriority.Count)
            {
                list.Add(lowerPriority[i]);
                i++;
            }
        }
    }
}
