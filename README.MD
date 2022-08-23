# Honours Project


My Honours project will be carried out on this game of my design: WarZone. This game and the platform it is built within is designed to be an educational platform in which lay-men to programming or AI can develop, implement and experiment with AI model training and testing using a variety of AI methodologies focussing primarily on deep reinforcement learning.

## WarZone

WarZone is a top down box-head style shooter in which two large teams of players fight in many different game modes, with a focus on the games fundamental gameplay mechanics. 
The game is designed for team games with an emphasis on strategy and teamwork. 

### Game Mechanics

The gameplay in this project is fairly unique that gives a new feel to shooters and multiplayer games in general. The primary aspect of this unique experience is the health system.
Players can only gain health by ranking up, and they can only rank up by killing players, or collecting tokens scattered about the map. As a player ranks up, there health is not only restored,
but their maximum health increases too. Once a player is at the highest rank, they will no longer have the means to gain more health, requiring extra strategy to stay alive. However, due to
the health and weapons bonus a fully ranked player will have, it will take a number of enemies to take them down.

The weapon system is likewise accessed by ranking up. There are five weapons in the game currently. The starting pistol, the shotgun, uzi, LMG and finally a bolt action sniper rifle. with 
each weapon more devastating than the last. This makes a fully ranked up player truly dangerous. The problem is, once a player dies, they return to rank 1 and loose all their health/weapons
priveledges. This is designed to introduce the concept of self-preservation and deeper strategy for potential AI agents.

- Multiplayer

The multiplayer capabilities of this game include both LAN and networked online multiplayer. This was implemented for ease of the study phase of this project in which participants of varying computational competency were to play the game with the researcher in another room, fighting enemies as the progressively spawn, and then later deciding which one(s) they thought were real humans and which were AI.

- Build mode

WarZone comes complete with a dedicated build system, which not only has the benefit of letting players build there own unique maps, but also will allow me to create multiple maps when
implementing my learning AI into the game for training. The build system is a drag and drop system that saves map information to text files for later reading.

- AI

The AI was implemented using a Tensorflow application written in python and encapsulated in a C# wrapper allowing it to be used as part of a Unity-based framework.
