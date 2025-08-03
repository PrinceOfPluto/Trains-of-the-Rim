# Trains of the Rim

## Overview

Trains of the Rim revolutionizes your colony's bulk transportation options by introducing a railway vehicle system. Trains can efficiently move large quantities of cargo and passengers across vast distances. However, your colony must pay the upfront cost of labor-intensive rail infrastructure to gain the long-term benefits of massive cargo capacity and significant fuel efficiency. This mod aims to offer trains as an early industrial vehicle option while still remaining relevant in mid-to-late game colonies as a fuel efficient way to meet bulk trading and transportation needs.

## Railway Vehicles
- 8 different rail cars including boxcar, caboose, flatbed, flatbed with stakes, gondola, hopper, passenger car, and tanker
- 3 different locomotives including a small diesel electric, a small steam tank engine, and a heavy steam locomotive
- Steam locomotives can be researched early after Smithing, but these burn wood logs as fuel and are less efficient than chemfuel locomotives that can be researched after Biofuel Refining and Machining
- Steam locomotives have variants for either using wood or chemfuel
- You can cycle between alternate textures for wood, coal, or chemfuel burning locomotives depending on which texture you prefer

## Caravans
- Trains use the Vehicles tab in the Form Caravan menu
- Trains may only travel along railroads on the world map
- Rail car "vehicles" can move without fuel and do not require a driver, but the rail cars cannot form a train caravan without a fuel burning locomotive
- Currently each additional rail car in a caravan needs a pawn passenger due to a Vehicle Framework bug with autonomous vehicles joining caravans

## Railroad Building
- Build both railroads and normal roads using mechanics based on the road building system in Vanilla Factions Expanded: Classical (very thankful for the generous permission from Oskar Potocki to reuse some VFE: Classical C# code!)
- Send a caravan to a tile and choose a road type to build to a neighboring tile. This doesn't use any resources, but takes a lot of time, particularly for railroads. The more pawns in the caravan, the faster it will be built.
- Railroad building requires researching Steam Locomotives and stone roads require researching Stonecutting.
- Built-in compatibility if you also use road building from VFE: Classical
- Compatible with Rails and Roads of the Rim
- New rail assets buildable on the colony-map, inspired by Decorative Railway Props

## Train Pathing
- Train vehicles can be coupled together to follow a locomotive vehicle. When you order the locomotive to move, all coupled train vehicles will attempt to path to the appropriate relative location to reform the train. I strongly recommend using the "Right click and drag to orient" feature when ordering the locomotive to move to a new location so that the train is rotated correctly.
- Coupled train vehicles can be recalled to a saved position similar to the Defensive Positions mod
- Train vehicles returning to the colony map from a caravan will automatically return to their saved position
- Optional experimental feature for requiring special railroad terrain to be built in order for train vehicles to move. This is highly experimental mainly due to some pathfinding quirks within Vehicle Framework when the Full Vehicle Pathing mod option from VF is enabled which it is by default. You can disable Full Vehicle Pathing in the mod settings for Vehicle Framework, but this causes some different movement quirks.
- When the VF mod setting Full Vehicle Pathing is enabled, a longer locomotive like the Frisco may have issues with being ordered to move to a location if the vehicle would not fit when rotated.

## Future Plans
- Adjust gameplay balance and textures based on feedback.
- Additional locomotives including a classic 19th century 4-6-0 steam locomotive, a streetcar trolley, and Derail Valley's DM3 and DE6.
- Additional rolling stock and cargo texture options like steel in hopper cars or wood logs on staked flatbeds.
- Add guidance for creating custom locomotives and railcars
- Possibly increasing quantity of bulk goods available at nearby settlements if connected in a rail network.

## Compatibility
- Should be compatible with all vehicle mods including Vanilla Vehicles Expanded
- Built-in compatibility with VFE: Classical
- Compatible with Rails and Roads of the Rim

## FAQ
Q: Do trains require rails?
A: On the world map, trains require railroads. On the colony map, trains are not restricted to rails by default. There is an experimental setting to require railroad terrain on the colony map, but it still has many issues and quirks.

Q: How do I create a train?
A: Select a locomotive and click "Create train". You can then couple rail cars to it with the "Couple to train" gizmo.

Q: How do I form a train caravan?
A: Use the normal Form Caravan menu and select the train vehicles in the Vehicles tab.

Q: Why can't I select the train vehicles when forming a caravan?
A: Make sure your colony world tile itself has a railroad, otherwise you will be unable to select the train vehicles for a caravan.

Q: How do I build a railroad on a world tile?
A: Send a caravan to a tile and choose a road type to build to a neighboring tile. This doesn't use any resources, but takes a lot of time, particularly for railroads. The more pawns in the caravan, the faster it will be built. Make sure you've researched "Steam Locomotives" first.

Q: Are rail cars supposed to move on their own?
A: Yes, currently for ease of adjusting placement of rail cars. In the future, I may restrict rail car movement if the car is not attached to a train with a locomotive.

Q: Do tankers interact with pipe networks?
A: No, but I might look into it or perhaps even make non-vehicle versions of the rail cars for storage use.

## Credits
- SmashPhil for creating Vehicle Framework
- Oskar Potocki and the Vanilla Expanded team for road building mechanics from Vanilla Factions Expanded: Classical
- PentalimbedP for Decorative Railway Props, particularly the art for the Frisco steam locomotive
- Symbolic for Defensive Positions, used as inspiration for train recall mechanics