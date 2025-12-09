# Lost in the Dungeon

![G2-01-final Image 7](https://github.com/user-attachments/assets/1f165f83-a2fe-4e1a-b9dc-0f52f4a97315)


## Title, Hook, Goals
Lost in the Dungeon is a 3D puzzle adventure game where you have to cooperate with your
partner in order to solve puzzles and find your way out of the dungeon you got trapped in
and retrieve Thor’s Hammer.

## Story Synopsis, Characters, Backstories
Eldric (a mage) and Rynn (a rogue) find themselves in a dungeon in search of adventure.
Suddenly, the lights turn off and Eldric fumbles with his flashlight. It turns on for a second,
but the battery goes out, and it turns off again. Out of nowhere, they hear a snap, the
flashlight turns on and illuminates the room. A mysterious voice starts speaking. It is Thor
(the narrator), who is looking for his hammer, Mjölnir, that his brother Loki hid. Since his
brother cursed him, to not be able to find it himself, he puts his trust into Eldric and Rynn to
retrieve the hammer. They need to use their flashlights and mirrors to traverse the dungeon
and bring the hammer to Thor. The dialogue of the whole story is in the annex.

## Gameplay, Main Mechanics
It’s a 3D puzzle-solving adventure game where the main mechanic is the element of light.
The players can run, cooperate and interact with mirrors. Eldric’s color is purple, and he can
only use the purple flashlight, Rynn’s color is green, and she can only use the green
flashlight. They need to position the flashlights in a way that the light reflects off enough
mirrors. If both beams hit the target at the same time, the target turns yellow, otherwise it
remains white or turns to the player’s color

## Genre
Puzzle Adventure Game

## Hardware Requirements
Processor: Intel Core i5-6400, Memory: 8 GB RAM, Graphics: Intel Iris XE, Storage: 3 GB
available space

## Co-Op Multiplayer Support
It is implemented using NetCode. It is the backbone of our game, since one of the main
elements is cooperation. The biggest problem was that almost every mechanic needed to be
synchronized and propagated to the client. Otherwise, e.g. if Eldric’s beam was hitting the
statue, Rynn would not be able to see it and the game wouldn’t register when both are hitting
the same target. Also, objects would be positioned differently. The host is doing part of the
computation that needs to be sent to the client. In some cases you need to prevent the client
from sending data to the host and in other cases you need to allow it.
