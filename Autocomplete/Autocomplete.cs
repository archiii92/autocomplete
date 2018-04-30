using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Autocomplete
{
    /// <summary>
    ///     Class for words autocompleting.
    /// </summary>
    class Autocomplete
    {
        /// <summary>
        ///     Name of Input file.
        /// </summary>
        readonly string _inputFileName;

        /// <summary>
        ///     Name of Output file.
        /// </summary>
        readonly string _outputFileName;

        /// <summary>
        ///     Start part of formatted output string.
        /// </summary>
        readonly string _startOutputString;

        /// <summary>
        ///     End part of formatted output string
        /// </summary>
        readonly string _endOutputString;

        /// <summary>
        ///     Maximum number of found autocomplete strings for input word.
        /// </summary>
        readonly int _maxAutocomleteStringsCount;

        /// <summary>
        ///     String that splits sections of the input file.
        /// </summary>
        readonly string _inputFilePartsBreaker;

        /// <summary>
        ///     Dictionary of acronym - strings of sentence from which one can obtain an acronym.
        /// </summary>
        readonly IDictionary<string, List<string>> _acronyms;

        /// <summary>
        ///     List of dictionary word, from which try get autocomplete for autocompletes words.
        /// </summary>
        readonly ICollection<string> _dictionary;

        /// <summary>
        ///     List of word for autocompleting.
        /// </summary>
        readonly ICollection<string> _autocomplete;

        /// <summary>
        ///     Autocomplete class constructor.
        /// </summary>
        /// <param name="inputFileName">Name of Input file</param>
        /// <param name="outputFileName">Name of Output file</param>
        /// <param name="inputFilePartsBreaker">String that splits sections of the input file</param>
        /// <param name="maxAutocomleteStringsCount">Maximum number of found autocomplete strings for input word</param>
        /// <param name="startOutputString">Start part of formatted output string</param>
        /// <param name="endOutputString">End part of formatted output string</param>
        public Autocomplete(
            string inputFileName,
            string outputFileName,
            string inputFilePartsBreaker = "===",
            int maxAutocomleteStringsCount = 10,
            string startOutputString = "[",
            string endOutputString = "]"
            )
        {
            _inputFileName = inputFileName;
            _outputFileName = outputFileName;
            _inputFilePartsBreaker = inputFilePartsBreaker;
            _maxAutocomleteStringsCount = maxAutocomleteStringsCount;
            _startOutputString = startOutputString;
            _endOutputString = endOutputString;

            _acronyms = new Dictionary<string, List<string>>();
            _dictionary = new HashSet< string>();
            _autocomplete = new HashSet<string>();
        }

        /// <summary>
        ///     Reads Input file. Makes autocomplete for each string in autocomplete list. Prints formatted result in Output file.
        /// </summary>
        public void MakeAutocomplete()
        {
            if (!CheckInputAndOutputFiles()) return;

            Console.WriteLine("Input and Output files exists\r\n");

            if (!ReadInputFile()) return;

            Console.WriteLine("Start autocompleting...\r\n");

            foreach (var autocompleteWord in _autocomplete)
            {
                List<string> hits = new List<string>();

                bool isAcronym = !autocompleteWord.Contains(" ");

                bool isAllLetterCapitalAcronym = isAcronym && Regex.IsMatch(autocompleteWord, @"[A-Z]");

                if (isAllLetterCapitalAcronym)
                {
                    CheckForAcronym(autocompleteWord.ToLower(), _acronyms, ref hits);

                    CheckForContains(autocompleteWord.ToLower(), _dictionary, ref hits);
                }
                else
                {
                    CheckForContains(autocompleteWord, _dictionary, ref hits);

                    if (isAcronym)
                    {
                        CheckForAcronym(autocompleteWord, _acronyms, ref hits);
                    }
                }

                if (hits.Any())
                {
                    WriteToOutputFile(autocompleteWord, hits);
                }
            }

            Console.WriteLine("\r\nAutocompleting is finished!\r\n");
        }

        /// <summary>
        ///     Checks what Input and Output files exists.
        /// </summary>
        /// <returns>A flag indicating whether files exist</returns>
        private bool CheckInputAndOutputFiles()
        {
            bool isFileExists = true;

            if (!File.Exists(_inputFileName))
            {
                Console.WriteLine("Input file does not exist!");
                isFileExists = false;
            }

            if (!File.Exists(_outputFileName))
            {
                Console.WriteLine("Output file does not exist!");
                isFileExists = false;
            }

            return isFileExists;
        }

        /// <summary>
        ///     Reads Input file. Checks it structure. 
        ///     Сaptures acronym - strings of sentence from which one can obtain an acronym dictionary
        ///     and lists of dictionary strings and strings to be autocomplete.
        /// </summary>
        /// <returns>A flag indicating whether a read was successful or not</returns>
        private bool ReadInputFile()
        {
            Console.WriteLine("Read Input file: {0} \r\n", _inputFileName);

            bool isDictionaryPart = true;
            bool isInputFileValid = false;

            Console.WriteLine("Reading dictionary strings...\r\n");

            using (StreamReader sr = new StreamReader(_inputFileName))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line != _inputFilePartsBreaker)
                    {
                        if (!CheckLine(line)) continue;

                        if (isDictionaryPart)
                        {
                            string[] splitted = line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                            // If line consists of two word and more - acronym can be made of it
                            if (splitted.Length > 1)
                            {
                                MakeAndSaveAcronym(line, splitted);
                            }

                            _dictionary.Add(line.ToLower());
                            Console.WriteLine(line);
                        }
                        else
                        {
                            // Autocomplete word must be two letters or longer
                            if (line.Length >= 2)
                            {
                                _autocomplete.Add(line);
                                Console.WriteLine(line);
                            }
                        }
                    }
                    else
                    {
                        if (isDictionaryPart)
                        {
                            // If we get to _inputFilePartsBreaker line for first time, then we finish reading dictionary part and starts read autocomplete part
                            isDictionaryPart = false;
                            Console.WriteLine("\r\nReading autocomplete strings...\r\n");
                        }
                        else
                        {
                            // If we get to _inputFilePartsBreaker line for second time, then we stop reading input file. It has correct structure.
                            isInputFileValid = true;
                            Console.WriteLine(
                                "\r\nThe structure of input file is correct and the reading is finished\r\n");
                            break;
                        }
                    }
                }
            }

            if (!isInputFileValid)
            {
                Console.WriteLine("The structure of input file is incorrect\r\n");
                return false;
            }

            if (!_dictionary.Any())
            {
                Console.WriteLine("The dictionary list is empty, check dictionary part of input file\r\n");
                return false;
            }

            if (!_autocomplete.Any())
            {
                Console.WriteLine("The autocomplete list is empty, check autocomplete part of input file\r\n");
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Makes acronym from <paramref name="splittedSentence"/> input string separated by words. 
        ///     Save it to acronym - strings of sentence from which one can obtain an acronym dictionary.
        /// </summary>
        /// <param name="initialString">Initial input string from which acronym will be made</param>
        /// <param name="splittedSentence">Initial input string separated by words and cleared of spaces</param>
        private void MakeAndSaveAcronym(string initialString, string[] splittedSentence)
        {
            string acronym = "";

            foreach (string word in splittedSentence)
            {
                acronym += word[0].ToString().ToLower();
            }

            if (_acronyms.ContainsKey(acronym))
            {
                List<string> foundSentences;
                if (_acronyms.TryGetValue(acronym, out foundSentences))
                {
                    foundSentences.Add(initialString);
                }
            }
            else
            {
                _acronyms.Add(
                    new KeyValuePair<string, List<string>>(acronym,
                        new List<string> { initialString }));
            }

            Console.WriteLine(initialString + " --> " + acronym);
        }

        /// <summary>
        ///     Writes <paramref name="hits"/> autocompleting results for <paramref name="autocompleteWord"/> string to Output file.
        /// </summary>
        /// <param name="autocompleteWord">String for which autocomplete words were found</param>
        /// <param name="hits">List of strings that are autocomplete for string</param>
        private void WriteToOutputFile(string autocompleteWord, List<string> hits)
        {
            using (StreamWriter outputFile = new StreamWriter(_outputFileName, true))
            {
                Console.WriteLine(FormatAutocomplete(autocompleteWord));
                outputFile.WriteLine(FormatAutocomplete(autocompleteWord));

                foreach (string hit in hits)
                {
                    Console.WriteLine(hit);
                    outputFile.WriteLine(hit);
                }
            }
        }

        /// <summary>
        ///     Checks that the <paramref name="line"/> string consists of a Latin letter, and each word does not exceed 50 characters.
        /// </summary>
        /// <param name="line">String to be validated</param>
        /// <returns>A flag indicating whether the string passed the validation</returns>
        private bool CheckLine(string line)
        {
            string[] splitted = line.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);

            if (splitted.Length == 0) return false;

            foreach (var word in splitted)
            {
                if (!Regex.IsMatch(word, @"^[a-zA-Z]+$") || word.Length > 50)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Checks for the presence of <paramref name="acronym"/> acronym in the <paramref name="acronyms"/> dictionary.
        /// </summary>
        /// <param name="acronym">Acronym, whose decoding we will look for</param>
        /// <param name="acronyms">Dictionary of all found acronyms</param>
        /// <param name="list">A list transferred by reference, in which all sentences associated with the acronym will be added</param>
        private void CheckForAcronym(string acronym, IDictionary<string, List<string>> acronyms,
            ref List<string> list)
        {
            if (list.Count < _maxAutocomleteStringsCount && acronyms.ContainsKey(acronym))
            {
                List<string> foundSentences;
                if (acronyms.TryGetValue(acronym, out foundSentences))
                {
                    int i = 0;
                    while (list.Count < _maxAutocomleteStringsCount && i < foundSentences.Count)
                    {
                        list.Add(foundSentences[i]);
                        i++;
                    }
                }
            }
        }

        /// <summary>
        ///     Checks if the <paramref name="word"/> string is part of another strings listed in <paramref name="dictionary"/> dictionary.
        /// </summary>
        /// <param name="word">String that we will search for in other lines</param>
        /// <param name="dictionary">List of all correct input strings</param>
        /// <param name="list">A list transferred by reference, in which all words and sentences containing the word will be added</param>
        private void CheckForContains(string word, ICollection<string> dictionary, ref List<string> list)
        {
            if (list.Count < _maxAutocomleteStringsCount)
            {
                List<string> lowerPriority = new List<string>();

                foreach (var dictionaryWord in dictionary)
                {
                    // Verifying that the word is part of dictionaryWord
                    if (Regex.IsMatch(dictionaryWord, word))
                    {
                        // Verifying that the dictionaryWord starts with word
                        if (Regex.IsMatch(dictionaryWord, @"(?:^|\s)" + word))
                        {
                            list.Add(dictionaryWord);

                            if (list.Count == _maxAutocomleteStringsCount) return;
                        }
                        else
                        {
                            // If not - put it in lowerPriority list
                            lowerPriority.Add(dictionaryWord);
                        }
                    }
                }

                int i = 0;
                while (list.Count < _maxAutocomleteStringsCount && i < lowerPriority.Count)
                {
                    list.Add(lowerPriority[i]);
                    i++;
                }
            }
        }

        /// <summary>
        ///     Formats <paramref name="autocomleteWord"/> string for output.
        /// </summary>
        /// <param name="autocomleteWord">String for which was found autocomplete words</param>
        /// <returns>Formatted <paramref name="autocomleteWord"/> string</returns>
        private string FormatAutocomplete(string autocomleteWord)
        {
            return $"{_startOutputString}{autocomleteWord}{_endOutputString}";
        }
    }
}
