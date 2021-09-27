# Earwax Prompt Importer
Very sloppy cmd app that converts plain text into a prompt file or a list of audio for the game Earwax in The Jackbox Party Pack 2. To use, simply build the project then drag a text file onto the resulting executable.


### Prompts
For Prompts, the text file file needs to begin with `[Prompt]` then have each prompt seperated by a line break. Prompt Lists generate a `.jet` file (really just a `.json` file) insde a new `CustomEarwaxPrompts` directory that can replace the original `EarwaxPrompts.jet` file in your Earwax content directory (`{INSTALLDIR}\The Jackbox Party Pack 2\games\Earwax\content`). This will also create OGG files in the `CustomEarwaxPrompts` directory which are simple Text to Speech generations of the prompt text. Place these into the EarwaxPrompts directory (`{INSTALLDIR}\The Jackbox Party Pack 2\games\Earwax\content\EarwaxPrompts`).

A prompt can italisice part by using the standard `<i></i>` html tags, or a player name by using an `<ANY>` tag where you want the name to show.

### Sounds
For Sounds, the text file needs to begin with `[Audio]` then have each sound's file path on a line, followed by a break character `|` then the text that should be shown to subtitle the sound. This will generate a `.jet` file (really just a `.json` file) insde a new `CustomEarwaxSounds` directory that can replace the original `EarwaxAudio.jet` file in your Earwax content directory (`{INSTALLDIR}\The Jackbox Party Pack 2\games\Earwax\content`). An OGG version of each sound will also be created in `CustomEarwaxSounds\Oggs`, these need to be copied to the Audio folder (`{INSTALLDIR}\The Jackbox Party Pack 2\games\Earwax\content\EarwaxAudio\Audio`). Dummy spectrum visualisations are also placed in `CustomEarwaxSounds\SpectrumDummies`, these need to be copied to the Spectrum folder (`{INSTALLDIR}\The Jackbox Party Pack 2\games\Earwax\content\EarwaxAudio\Spectrum`).

### File Examples
```
[Prompt]
Demonstration Prompt 1
Demonstration Prompt 2, using <i>Italic</i> text.
Demonstration Prompt 3, using <ANY>'s name.
```

```
[Audio]
Z:\DemoSound1.wav|Demonstration Sound 1
Z:\DemoSound2.ogg|Demonstration Sound 2
```