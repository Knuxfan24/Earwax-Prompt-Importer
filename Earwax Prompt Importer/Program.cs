using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;

namespace Earwax_Prompt_Importer
{
    internal class Program
    {
        // Earwax Data Classes
        public class PromptFormatData
        {
            public List<EarwaxPrompt> Content = new();
        }
        public class EarwaxPrompt
        {
            [JsonProperty(Order = 1)]
            public int ID { get; set; }

            [JsonProperty(Order = 2)]
            public bool X { get; set; }

            [JsonProperty(Order = 3)]
            public string PromptAudio { get; set; }

            [JsonProperty(Order = 4)]
            public string Name { get; set; }

            public override string ToString() => Name;
        }

        static void Main(string[] args)
        {
            // Complain if we don't have a file to parse.
            if(args.Length == 0)
            {
                Console.WriteLine("Drag a txt file onto this program, where each line is a prompt to be converted.");
                Console.ReadKey();
                return;
            }

            // Split text file.
            var text = File.ReadAllLines(args[0]);

            // Set up the actual Data.
            PromptFormatData Data = new();

            for (int i = 0; i < text.Length; i++)
            {
                EarwaxPrompt prompt = new()
                {
                    ID = 24000000 + i,
                    X = false,
                    PromptAudio = $"custom_{i}",
                    Name = text[i]
                };

                Data.Content.Add(prompt);
            }

            // Write the JET file.
            JsonSerializerSettings serializerSettings = new()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            File.WriteAllText($"{Path.GetDirectoryName(args[0])}\\{Path.GetFileNameWithoutExtension(args[0])}.jet", JsonConvert.SerializeObject(Data, Formatting.Indented, serializerSettings));
        }
    }
}
