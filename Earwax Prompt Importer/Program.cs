using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Speech.Synthesis;

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

                // Create the WAV for the user to manually make into OGGs (src: https://docs.microsoft.com/en-us/dotnet/api/system.speech.synthesis.speechsynthesizer.setoutputtowavefile?view=netframework-4.8)
                // Initialize a new instance of the SpeechSynthesizer.  
                SpeechSynthesizer synth = new();

                // Configure the audio output.   
                synth.SetOutputToWaveFile($@"{Path.GetDirectoryName(args[0])}\custom_{i}.wav");

                // Build a prompt, strip out the control tags.  
                PromptBuilder builder = new();
                string tts = text[i].Replace("<ANY>", "this player");
                tts = tts.Replace("<i>", "");
                tts = tts.Replace("</i>", "");
                builder.AppendText(tts);

                // Speak the string asynchronously.  
                synth.SpeakAsync(builder);

                //synth.Dispose();

                // Convert WAV to OGG
                if (File.Exists($@"{Path.GetDirectoryName(args[0])}\custom_{i}.wav"))
                {
                    using (Process process = new())
                    {
                        process.StartInfo.FileName = $"\"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\\ExternalResources\\oggenc2.exe\"";
                        process.StartInfo.Arguments = $"\"{Path.GetDirectoryName(args[0])}\\custom_{i}.wav\"";
                        //process.StartInfo.WorkingDirectory = $"\"{Path.GetDirectoryName(args[0])}\"";
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.CreateNoWindow = true;

                        process.Start();
                        process.BeginOutputReadLine();
                        process.WaitForExit();
                    }

                    //File.Delete($@"{Path.GetDirectoryName(args[0])}\custom_{i}.wav");
                }
            }


            // Write the JET file.
            JsonSerializerSettings serializerSettings = new()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            File.WriteAllText($"{Path.GetDirectoryName(args[0])}\\{Path.GetFileNameWithoutExtension(args[0])}.jet", JsonConvert.SerializeObject(Data, Formatting.Indented, serializerSettings));

            string[] dumbJSONWorkaround = File.ReadAllLines($"{Path.GetDirectoryName(args[0])}\\{Path.GetFileNameWithoutExtension(args[0])}.jet");
            for (int i = 0; i < dumbJSONWorkaround.Length; i++)
            {
                dumbJSONWorkaround[i] = dumbJSONWorkaround[i].Replace("promptAudio", "PromptAudio");
            }
            File.WriteAllLines($"{Path.GetDirectoryName(args[0])}\\{Path.GetFileNameWithoutExtension(args[0])}.jet", dumbJSONWorkaround);
        }
    }
}
