using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Autocomplete
{
    class Program
    {
        static void Main()
        { 
            List<string> dictionary = new List<string>();
            List<string> autocomplete = new List<string>();
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
                bool foundMatch = false;
                List<string> hitsOnConsist = new List<string>();

                foreach (var dictionaryWord in dictionary)
                {
                    if (Regex.IsMatch(dictionaryWord, autocompleteWord + @"(\S+)")) //@"(?:^|\s)" + 
                    {
                        //if (!foundMatch)
                        //{
                        //    Console.WriteLine("[" + autocompleteWord + "]");
                        //    foundMatch = true;
                        //}

                        //Console.WriteLine(dictionaryWord);

                        hitsOnConsist.Add(dictionaryWord);
                        foundMatch = true;

                        //Console.WriteLine(dictionaryWord + " --> " + autocompleteWord);
                    }
                }

                if (foundMatch)
                {
                    Console.WriteLine("[" + autocompleteWord + "]");

                    List<string> lowerPriority = new List<string>();

                    foreach (string hits in hitsOnConsist)
                    {
                        if (Regex.IsMatch(hits, @"(?:^|\s)" + autocompleteWord))
                        {
                            Console.WriteLine(hits);
                        }
                        else
                        {
                            lowerPriority.Add(hits);
                        }
                    }

                    foreach (string hits in lowerPriority)
                    {
                        Console.WriteLine(hits);
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
    }
}
