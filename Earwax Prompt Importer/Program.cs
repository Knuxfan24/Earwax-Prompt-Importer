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
        // Earwax Prompt Data Classes
        public class PromptFormatData
        {
            /// <summary>
            /// List of prompts. Placed this like this so the JSON writes correctly.
            /// </summary>
            [JsonProperty(Order = 1, PropertyName = "content")]
            public List<EarwaxPrompt> Content = new();
        }
        public class EarwaxPrompt
        {
            /// <summary>
            /// ID number, not sure what this is used for, as it doesn't seem to be incremental?
            /// </summary>
            [JsonProperty(Order = 1, PropertyName = "id")]
            public int ID { get; set; }

            /// <summary>
            /// Whether this prompt should be removed if Family Friendly mode is on.
            /// </summary>
            [JsonProperty(Order = 2, PropertyName = "x")]
            public bool Explicit { get; set; }

            /// <summary>
            /// The sound file to play for this prompt.
            /// </summary>
            [JsonProperty(Order = 3, PropertyName = "PromptAudio")]
            public string PromptAudio { get; set; }

            /// <summary>
            /// The actual text of this prompt.
            /// </summary>
            [JsonProperty(Order = 4, PropertyName = "name")]
            public string Prompt { get; set; }

            /// <summary>
            /// Makes the VS Debuger show the Name variable rather than just Earwax_Prompt_Importer.EarwaxPrompt for this entry.
            /// </summary>
            public override string ToString() => Prompt;
        }

        // Earwax Audio Data Classes
        public class AudioFormatData
        {
            /// <summary>
            /// Unknown, is set to 1234.
            /// </summary>
            [JsonProperty(Order = 1, PropertyName = "episodeid")]
            public int Episode { get; set; }

            /// <summary>
            /// List of audio selections.
            /// </summary>
            [JsonProperty(Order = 2, PropertyName = "content")]
            public List<EarwaxAudio> Content = new();
        }
        public class EarwaxAudio
        {
            /// <summary>
            /// Whether this sound should be removed if Family Friendly mode is on.
            /// </summary>
            [JsonProperty(Order = 1, PropertyName = "x")]
            public bool Explicit { get; set; }

            /// <summary>
            /// The actual text of this sound.
            /// </summary>
            [JsonProperty(Order = 2, PropertyName = "name")]
            public string Name { get; set; }

            /// <summary>
            /// Unknown, seems to be the same as Name?
            /// </summary>
            [JsonProperty(Order = 3, PropertyName = "short")]
            public string ShortName { get; set; }

            /// <summary>
            /// ID number, used to select the sound file to play.
            /// </summary>
            [JsonProperty(Order = 4, PropertyName = "id")]
            public int ID { get; set; }

            /// <summary>
            /// The categories this sound is in, not sure what this is used for beyond achievements.
            /// </summary>
            [JsonProperty(Order = 5, PropertyName = "categories")]
            public List<string> Categories = new();

            /// <summary>
            /// Makes the VS Debuger show the Name variable rather than just Earwax_Prompt_Importer.EarwaxAudio for this entry.
            /// </summary>
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

            // Check if this is an audio list.
            // TODO, way to mark a sound as explicit for closer emulation of original features?
            if (text[0] == "[Audio]")
            {
                // Create the directories to store things in.
                // TODO, maybe see about checking if these already exist and asking for permission to delete them.
                Directory.CreateDirectory($"{Path.GetDirectoryName(args[0])}\\CustomEarwaxSounds");
                Directory.CreateDirectory($"{Path.GetDirectoryName(args[0])}\\CustomEarwaxSounds\\Oggs");
                Directory.CreateDirectory($"{Path.GetDirectoryName(args[0])}\\CustomEarwaxSounds\\SpectrumDummies");

                // Set up the actual Data.
                AudioFormatData AudioData = new()
                {
                    Episode = 1234
                };

                // Loop through our list of sounds.
                for (int i = 1; i < text.Length; i++)
                {
                    // Check this line actually contains the split character.
                    if (text[i].Contains('|'))
                    {
                        // Split the string on the split character
                        string[] split = text[i].Split('|');

                        // Check the specified file exists.
                        if (File.Exists(split[0]))
                        {
                            Console.WriteLine($"Converting '{split[0]}' to Earwax sound.");

                            EarwaxAudio audio = new()
                            {
                                Explicit = false,
                                Name = split[1],
                                ShortName = split[1],
                                ID = 24000000 + i
                            };
                            AudioData.Content.Add(audio);

                            // If this file's already an OGG, then copy it, if not then convert it.
                            // TODO, test more formats than just WAVs.
                            if (Path.GetExtension(split[0]) == ".ogg")
                            {
                                File.Copy(split[0], $"{Path.GetDirectoryName(args[0])}\\CustomEarwaxSounds\\Oggs\\{audio.ID}.ogg", true);
                            }
                            else
                            {
                                using (Process process = new())
                                {
                                    process.StartInfo.FileName = $"\"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\\ExternalResources\\oggenc2.exe\"";
                                    process.StartInfo.Arguments = $"-o \"{Path.GetDirectoryName(args[0])}\\CustomEarwaxSounds\\Oggs\\{audio.ID}.ogg\" \"{split[0]}\"";
                                    process.StartInfo.UseShellExecute = false;
                                    process.StartInfo.RedirectStandardOutput = true;
                                    process.StartInfo.CreateNoWindow = true;

                                    process.Start();
                                    process.BeginOutputReadLine();
                                    process.WaitForExit();
                                }
                            }

                            // Sloppily create a dummy spectrum file so the game doesn't hang.
                            List<string> DummySpectrum = new();
                            DummySpectrum.Add("{");
                            DummySpectrum.Add("\t\"Refresh\":23,");
                            DummySpectrum.Add("\t\"Frequencies\":[");
                            DummySpectrum.Add("\t\t{\"left\":[ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],");
                            DummySpectrum.Add("\t\t \"right\":[ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]}");
                            DummySpectrum.Add("\t],");
                            DummySpectrum.Add("\t\"Peak\":100");
                            DummySpectrum.Add("}");
                            File.WriteAllLines($"{Path.GetDirectoryName(args[0])}\\CustomEarwaxSounds\\SpectrumDummies\\{audio.ID}.jet", DummySpectrum);
                        }
                    }

                    File.WriteAllText($"{Path.GetDirectoryName(args[0])}\\CustomEarwaxSounds\\{Path.GetFileNameWithoutExtension(args[0])}.jet", JsonConvert.SerializeObject(AudioData, Formatting.Indented));
                }
            }

            // Check if this is a prompt list.
            // TODO, way to mark a prompt as explicit for closer emulation of original features?
            if (text[0] == "[Prompts]")
            {
                // Create a directory to store things.
                // TODO, maybe see about checking if this already exists and asking for permission to delete it.
                Directory.CreateDirectory($"{Path.GetDirectoryName(args[0])}\\CustomEarwaxPrompts");

                // Set up the actual Data.
                PromptFormatData Data = new();

                // Loop through each entry in our text file.
                for (int i = 1; i < text.Length; i++)
                {
                    Console.WriteLine($"Converting '{text[i]}' to prompt.");

                    EarwaxPrompt prompt = new()
                    {
                        ID = 24000000 + i,
                        Explicit = false,
                        PromptAudio = $"custom_{i}",
                        Prompt = text[i]
                    };

                    Data.Content.Add(prompt);

                    // Create the WAV for the user to manually make into OGGs (src: https://docs.microsoft.com/en-us/dotnet/api/system.speech.synthesis.speechsynthesizer.setoutputtowavefile?view=netframework-4.8)
                    // Initialize a new instance of the SpeechSynthesizer.  
                    SpeechSynthesizer synth = new();

                    // Configure the audio output.   
                    synth.SetOutputToWaveFile($@"{Path.GetDirectoryName(args[0])}\CustomEarwaxPrompts\custom_{i}.wav");

                    // Build a prompt, strip out the control tags.  
                    PromptBuilder builder = new();
                    string tts = text[i].Replace("<ANY>", "this player");
                    tts = tts.Replace("<i>", "");
                    tts = tts.Replace("</i>", "");
                    builder.AppendText(tts);

                    // Speak the sound to our WAV file.
                    synth.Speak(builder);

                    // Dispose of the builder so we can actually delete the WAV afterwards.
                    synth.Dispose();

                    // Convert WAV to OGG
                    if (File.Exists($@"{Path.GetDirectoryName(args[0])}\CustomEarwaxPrompts\custom_{i}.wav"))
                    {
                        using (Process process = new())
                        {
                            process.StartInfo.FileName = $"\"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\\ExternalResources\\oggenc2.exe\"";
                            process.StartInfo.Arguments = $"\"{Path.GetDirectoryName(args[0])}\\CustomEarwaxPrompts\\custom_{i}.wav\"";
                            process.StartInfo.UseShellExecute = false;
                            process.StartInfo.RedirectStandardOutput = true;
                            process.StartInfo.CreateNoWindow = true;

                            process.Start();
                            process.BeginOutputReadLine();
                            process.WaitForExit();
                        }

                        // Remove the now useless WAV.
                        File.Delete($@"{Path.GetDirectoryName(args[0])}\CustomEarwaxPrompts\custom_{i}.wav");
                    }
                }


                // Write the JET file.
                File.WriteAllText($"{Path.GetDirectoryName(args[0])}\\CustomEarwaxPrompts\\{Path.GetFileNameWithoutExtension(args[0])}.jet", JsonConvert.SerializeObject(Data, Formatting.Indented));
            }

            // See if it is actually either and give up if not.
            if (text[0] != "[Audio]" && text[0] != "[Prompts]")
            {
                Console.WriteLine($"'{args[0]}' does not appear to be either an Audio List or a Prompt List.\nPress any key to continue.");
                Console.ReadKey();
                return;
            }

            // Tell the user we're done.
            Console.WriteLine("\nDone!\nPress any key to continue.");
            Console.ReadKey();
        }
    }
}
