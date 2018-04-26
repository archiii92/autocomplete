using System;
using System.Collections.Generic;
using System.IO;

namespace Autocomplete
{
    class Program
    {
        static void Main()
        { 
            List<string> dictionary = new List<string>();
            List<string> autocomplete = new List<string>();
            bool isDictionaryFill = true;

            using (StreamReader reader = new StreamReader("Input.txt"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line != "===")
                    {
                        if (isDictionaryFill)
                        {
                            dictionary.Add(line);
                            Console.WriteLine(line);
                        }
                        else
                        {
                            autocomplete.Add(line);
                            Console.WriteLine(line);
                        }
                    }
                    else
                    {
                        isDictionaryFill = false;
                    }
                }
            }
            Console.ReadLine();
        }
    }
}
