# UNTAR Go Home!
## Version 1.0.0

Yet they're still here in Tarkov...

UNTAR Go Home! adds UNTAR as a proper faction to SPT. Encounter roving patrols of peacekeepers. Their ROE states that they are only to shoot if threatened, so try not to piss them off!

# Features

## New Bot Types
UNTARGH adds completely custom bot types that don't replace existing bots, each with their own loadouts and roles.

### UNTAR Grunt
The bulk of the UNTAR mission's combat-ready forces. They're meant to "keep the peace" and protect civilian members of the mission while they perform their duties. Expect them to be armed with a variety of western weapons while wearing standard UNTAR armor.

### UNTAR Squad Lead
An NCO in charge of a squad of UNTAR grunts. They keep the group in check while donning equipment from their origin nation. They have fancier weapons and gear on average.

### UNTAR Officer
Officers are in charge of ensuring that the mission's objectives (extract capable civilians, distribute aid to those stranded, and ~~make a f#$% ton of money~~) are adhered to and completed. Currently, they're functionally similar to squad leaders besides the fact they always wear a blue beret, but I do intend on expanding their purpose.

## UNTAR Patrols
Patrols can spawn across several maps in randomly generated patrols at random times during the raid. Their size and composition varies, with some larger patrols being lead by officers and potentially having multiple squad leaders. As more UNTAR types are added patrols will become more varied.

### Maps
Settings related to patrols can be adjusted in the config/main.json server file. This includes adding new maps and zones!
Patrols will pick a random available spawn zone found in the config file and a time in a defined range. Maps can be configured to have multiple patrols rolled (which also means you can encounter multiple patrols in one raid).

Currently, patrols will spawn on:

- Woods, 2 possible patrols at a time, decent spawn chance, small-mid sized
- Customs, 1 possible patrol, good spawn chance, mid-big sized
- Interchange, 2 possible patrols at a time, meh spawn chance, small-mid sized
- Shoreline, 1 possible patrol, decent spawn chance, small-mid sized

## Config
The config folder in the server mod lets you configure things such as patrol spawning and supported maps. Adjust the conditions for different roles to spawn in patrols or add the chance for patrols to spawn on other maps. I'll add detailed explanations of the config options later but they're fairly self-explanatory at the moment.

# Compatability

## SAIN
UNTARGH has *full* (I might've missed something. If you notice any problems with SAIN or errors do let me know!) compatability with SAIN, with UNTAR bots using SAIN behaviors and having dedicated config options in SAIN's settings.

## Spawn Mods
UNTARGH has no specific compatability code at the moment with any spawn mods. I still have to do some testing and collaborate with some authors to make sure UNTAR spawning works 100% with spawn mods. If you encounter any issues with spawning while using these sorts of mods I won't offer support, but I will accept logs so I can work on ironing out compatability.

### MOAR
MOAR seems to work with UNTARGH, but there's no additional compatability. Basically, UNTAR works off my spawn system, everything else works on MOAR's system.

### ABPS
ABPS currently prevents UNTAR from spawning entirely but AcidPhantasm is adding compatability with UNTARGH next ABPS update (tysm <3) so please standby!

## Other Bot Behavior Mods (Questing, Looting, etc)
UNTARGH is relatively untested with these mods. Questing Bots and Looting Bots don't conflict at the very least, so you can play with those just fine from what I can tell. I'll be conducting some testing and patching in necessary compatability fixes. Eventually, I want to include functionality related to Questing Bots in particular but that's for a later update.

# Credits and Thanks
Thanks to GrooveypenguinX and nameless___ for letting me reference yalls code, and specifically Groovey for giving me the run down on what I needed to do to get custom bots working. This mod wouldn't exist without that starting point.

Thanks to Solarint for making a mod I felt was necessary to have compatability for before publishing this mod. No, seriously, SAIN is amazing and if you don't already have it installed give it a look.

Thanks DrakiXYZ for BigBrains and Waypoints, absolutely necessary mods for anything bot-related. I'll be using BigBrains heavily in future versions of this mod (and more coming mods) to implment some custom behaviors. The less I have to touch BSG's code the better lmao.

Goes without saying that thanks should be given to the SPT team for enabling any of this in the first place. Yall rock!