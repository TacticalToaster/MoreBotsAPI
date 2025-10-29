# MoreBotsAPI for SPT 4.0.X
A client and server API that makes making custom bots a little less infuriating. Create a prepatch and server mod that implements this API and you'll have the basis for introducing new bosses, factions, and any other custom bot you can think of (maybe, this is still Tarkov).

### Table of Contents

 - [Features](#features)
 - [What's not included](#notincluded)
 - [Installation](#installation)
 - [Using the API](#using)
	 - [Example Repo](#example)
	 - [Client](#client)
	 - [Server](#server)
	 - [Enum Practice](#enumpractice)
	 - [Used Enums](#usedenum)

<a id="features"></a>
## Features
Adds to WildSpawnType enum on client
Server properly sends custom bot data to client
Defining custom bot data in a simple format with JSONs on the server
Defining client-specific data in a client prepatcher
SAIN compatability with a little setup
<a id="notincluded"></a>
## What's not included
**Locales**, reference SPT server mod examples for that
**Spawning of custom bots**, either implement that using the boss spawn system (UNTAR Go Home uses this method) or create your own system
**Custom behaviors**, use BigBrain if you intend on creating completely custom behavior not already found in base EFT bots.
<a id="installation"></a>
## Installation
 1. Download a release version from GitHub or SPT Forge.
 2. Open the .7z file using 7-Zip
 3. Drag the SPT and BepInEx folders into your main SPT directory, those folders alongside EscapeFromTarkov.exe should already exist.
<a id="using"></a>
# Using the API as a modder
To use the API, you'll need to make a client prepatcher and a server mod.
<a id="example"></a>
## Example Repo
https://github.com/TacticalToaster/MoreBotsAPI-Example
<a id="client"></a>
## Client Prepatcher
You'll need two classes, the prepatcher plugin class and the patch that implements your custom bots. The plugin must have this API's prepatcher as a hard dependency using the following attribute tag

    [BepInDependency("com.morebotsapiprepatch.tacticaltoaster", BepInDependency.DependencyFlags.HardDependency)]

Your patch should target ``Assembly-CSharp.dll``. In there you will define the custom information regarding your new bot type(s). Here is an example of what your patch class should look like (a similar example can be found in the example repo linked above):

```c#
public static class WildSpawnTypePatch
{
    public static IEnumerable<string> TargetDLLs { get; } = new[] { "Assembly-CSharp.dll" };

    public static void Patch(ref AssemblyDefinition assembly)
    {
        // 1069 is the enum value for your custom type, followed by the enum name. The other variables are explained in the CustomWildSpawnType class comments
        var exampleBot = new CustomWildSpawnType(1069, "bossExampleBot", "Boss", 32, true, false, false);

        exampleBot.SetCountAsBossForStatistics(true);
        // Should having max fence loyalty stop this bot from warning your pscav. Doesn't affect hostility (that is defined in the type json), only interaction with warn behavior.
        exampleBot.SetShouldUseFenceNoBossAttack(true, false);
        // Exclude all difficulties except Normal. This is done by default if you do not set excluded difficulties.
        exampleBot.SetExcludedDifficulties(new List<int> { 0, 2, 3 });

        SAINSettings settings = new SAINSettings(exampleBot.WildSpawnTypeValue)
        {
            Name = "Example Bot",
            Description = "An example bot created using MoreBotsAPI.",
            // This is the bot section your bot will appear under, like Bosses, Followers, PMCs, etc.
            Section = "Custom",
            // Not used unless you didn't define BrainsToApply
            BaseBrain = "Assault",
            // Look for the ShortName in the class of the brains you want to apply to.
            BrainsToApply = new List<string> { "Assault" }, 
        };

        exampleBot.SetSAINSettings(settings);

        // This is what registers your new spawn type, adding it to the WildSpawnType enum and manager for custom bot types.
        CustomWildSpawnTypeManager.RegisterWildSpawnType(exampleBot, assembly);

        // This allows the example bot to be the boss in a group with itself or normal scavs.
        // If you want your bot to be solo you don't have to worry about this.
        CustomWildSpawnTypeManager.AddSuitableGroup(new List<int> { 1069, 1 });
    }

}
```

This is everything you need to set your bot up on the client.
<a id="server"></a>
## Server Mod
Your server mod should have this API's server mod set as a dependency. Wherever you define your mod's metadata, make sure you add to the ModDependencies list:

``"com.morebotsapi.tacticaltoaster", new SemanticVersioning.Range(">=1.0.0") ``

Set the version range to what's applicable to your mod. If you require features from a later version of this API, make sure the dependency reflects that so we both have less time figuring out a user has the wrong version of the API installed.

To load your bots on the server, you'll want to have an injectable class with a TypePriority of ``OnLoadOrder.PostDBModLoader + 2``. Anything greater than 2 also works. Make sure your class implements ``IOnLoad``. After that, you'll want to inject the API ``MoreBotsServer.MoreBotsAPI`` into your class using the constructor. Finally, you'll want to call ``LoadBots`` on the API, passing your mod's assembly with ``Assembly.GetExecutingAssembly()``. From here, the API will load your custom bot data in your mod's folder using the file path standard defined in the API.

```
db/bots/types (BotType data goes here)
db/bots/config (BotConfig data goes here)
```

You can find an example of all this in the example repo above.

If you need to reference custom bot WildSpawnTypes in your server data files, use the int value of the enum instead of the name. The server currently doesn't support the names of custom enums but is okay with the int value.
<a id="enumpractice"></a>
## Enum Value Practice
To avoid conflicting enum values, I recommend following this format for deciding the value of your enums:

 1. Pick two uppercase characters, preferably related to your username or mod. For example, I'd pick TT if going by my username.
 2. Get the ASCII decimal value of those characters (make sure they're for uppercase). TT = 8484
 3. Add two zeros, that's your starting point of enum values. For me, that would be 848400.
 4. You now can safely (unless someone picks your same characters) use up to 100 values. For me, my range would end at 848499

Don't use 0-200. This avoids clashing with base Tarkov values and gives room for any new bot types they might add in the future.

I will try to document the values people use in their mods here in the next section. If you use this API, include what values and names you use on your mod page and readme and try to notify me so I can keep them cataloged.
<a id="usedenum"></a>
## Used Enum Values

### UNTAR Go Home (TacticalToaster)
1170-1179
