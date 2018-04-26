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

            Console.WriteLine("Считываем исходные данные");

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
                                string[] splitted = line.Split(' ');

                                if (splitted.Length > 1)
                                {
                                    string acronym = "";

                                    foreach (string word in splitted)
                                    {
                                        acronym += word[0].ToString().ToLower();
                                    }

                                    if (acronyms.ContainsKey(acronym))
                                    {
                                        List<string> foundedSentences;
                                        if (acronyms.TryGetValue(acronym, out foundedSentences))
                                        {
                                            foundedSentences.Add(line);
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
            Console.WriteLine("Начинаем воркать");

            foreach (var autocompleteWord in autocomplete)
            {
                List<string> hits = new List<string>();

                bool isAcronym = autocompleteWord.Split(' ').Length == 1;

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
                    Console.WriteLine("[" + autocompleteWord + "]");

                    foreach (string hit in hits)
                    {
                        Console.WriteLine(hit);
                    }
                }
            }
            Console.ReadLine();
        }

        private static bool CheckLine(string line)
        {
            string[] splitted = line.Split(' ');

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

        private static void CheckAcronym(string acronym, IDictionary<string, List<string>> acronyms, ref List<string> list)
        {
            if (list.Count < 10 && acronyms.ContainsKey(acronym))
            {
                List<string> foundedSentences;
                if (acronyms.TryGetValue(acronym, out foundedSentences))
                {
                    int i = 0;

                    while (list.Count < 10 && i < foundedSentences.Count)
                    {
                        list.Add(foundedSentences[i]);

                        i++;
                    }
                }
            }
        }

        private static void CheckConsist(string word, ICollection<string> dictionary, ref List<string> list)
        {
            List<string> lowerPriority = new List<string>();

            foreach (var dictionaryWord in dictionary)
            {
                if (list.Count < 10 && Regex.IsMatch(dictionaryWord, word))
                {
                    if (Regex.IsMatch(dictionaryWord, @"(?:^|\s)" + word))
                    {
                        list.Add(dictionaryWord);
                    }
                    else
                    {
                        lowerPriority.Add(dictionaryWord);
                    }
                }
            }

            if (list.Count < 10)
            {
                int i = 0;
                while (list.Count < 10 && i < lowerPriority.Count)
                {
                    list.Add(lowerPriority[i]);
                    i++;
                }
            }
        }
    }
}
