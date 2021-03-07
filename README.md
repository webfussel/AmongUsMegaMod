# MegaMod 1.4.1
A BepInEx Mod, that adds 9 new roles and 13 colors to Among Us.

# Notice
This Mod will work on official server if everyone has the Mod installed.  
It might be possible that someone without it can join your lobby, but that happiness won't last long, because the Custom RPC won't work which will result in a lot of unwanted behaviour.

# Installation
- Navigate to your `steamapps/common` directory
- Copy your `Among Us` directory and paste it again
- Rename the pasted directory in something like `Among Us Mods`
  - This skip prevents your copy to get updated, so you can continue using the mod until it's updated
- Open the `Among Us Mods` directory
- Extract the contents of `MegaMod-x.x.x.zip` into `Among Us Mods` and overwrite files if necessary

# New Roles
## Crewmates
### Engineer
#### Description
You've been to university all your life just to land on that god damn ship and crawl through stupid vents.  
Maybe there will be a time you can use your expertise to solve some kind of emergency without blinking an eye?
(The Engineer can solve one sabotage per game from anywhere on the map immediately through his "Repair"-Map.)

***Notice**: We think this is a very weak abilty, so we're going to improve that someday*
#### Options
The Engineer currently has no extra options.

### Detective
#### Description
You're a depressed fighter against evil. All you want is to kill that damn impostor that ate your little sister. But nearly killing a Crewmate would just drive you into suicide.  
Whenever you inspect a dead body you try to find traces of THAT impostor, even though those reports might not always be viable.

***Notice**: We think the Detective is a bit over powered, so we're going to split the role into two seperate roles someday*
#### Options
- Kill cooldown: Cooldown for kill button in seconds (Default: **30**)
- Body Reports: Turns Body Reports on and off (Default: **true**)
- Can Kill Maniac: Detective can kill Maniac without commiting suicide (Default: **true**)

### Doctor
#### Description
After years of experience you finally learned how to protect anyone you love! Okay, maybe not anyone - but at least one person!  
Create a shield and put it on someone you trust so they can never be killed... except when they get thrown into space or lava.  
You shield is very transparent glass which makes a **TING** sound whenever it gets hit.

***Notice**: This role will be renamed "Bodyguard" in the near future.*

#### Options
- Show Shielded Player: Shows the shielded player to the people set in the option
  - Self
  - Doctor
  - Self + Doctor
  - Everyone
- Shielded Player Murder Attempt Indicator: Shielded Player can hear the **TING** noise (Default: **true**)

### Seer
#### Description
You know things... But to be exact: You get a chat message whenever someone enters a vent, exits a vent and if someone dies.  
The problem is: If you know someone died you can not even call an emergency but have to find the corpse!
(*This is because no one of your mates believes in psychic abilities, think you're crazy and therefore revoked your right to use the emergency button.)

#### Options
- Can call emergency: Maybe your crew isn't that stubborn after all? (Default: **false**)

### Tracker
#### Description
Everyone is using a Tracking Blocker nowadays, but this Tracker is someone you should trust!  
You can mark an sabotage and whenever someone applies that sabotage you get a Chat message about where the saboteur was seen lately! 

#### Options
The Tracker currently has no extra options.

### Nocturnal
#### Description
The night is your home. You can see much better when the lights are out! Use that ability to see killers when the lights are out.

#### Options
The Nocturnal currently has no extra options.

### Pathfinder
#### Description
All the summers at camp are finally worth it! You can see the footsteps of all other players in their respective color.

#### Options
- Footprint Lifespan (seconds): When will the footsteps vanish? (Default: **4**)
- Footprint interval: How often do player leave footprints behind? (Default: **0.3**)
- Anonymous Footprints: Finding out which animals have which footprints was never your strength... Turns footprints grey. (Default: **false**)

## Solo
### Maniac
#### Description
Holy cow, you want to die! But you do not want to be killed off the cuff.  
Get yourself voted from the ship to win!  
Because you're fricking maniac you can see everyone's special role!

#### Options
- Can see all Roles: It's not like you stalked everyone before... like a maniac... (Default: **true**)

## Impostors
### Ninja
#### Description
DOUBLE KILL! You're a master of killing. Ones per match you can execute a kill while still on cooldown.  
That stuff is pretty exhausting, so you will need even more resting time!  
(That means: New Cooldown = Remaining Cooldown + Impostor Cooldown x 2)

#### Options
The Ninja currently has no extra options.

# Resources
- https://github.com/BepInEx
  - Hooking game functions
- https://github.com/NuclearPowered/Reactor
  - Modding Framework
- https://github.com/DorCoMaNdO/Reactor-Essentials
  - Creating Custom Game Options
- https://github.com/NotHunter101/ExtraRolesAmongUs
  - As this Mod began as a Fork of Hunters101's ExtraRoles Mod, we still have some base code of this mod.
- https://github.com/Hardel-DW/TooManyRolesMods
  - The Footprints for our "Pathfinder"
