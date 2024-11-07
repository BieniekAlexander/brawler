# Introduction
I'm messing around with Unity, trying to build a game for the fun of it.

## Design
In brief, I really enjoyed [Battlerite](https://store.steampowered.com/app/504370/Battlerite/) and would like to build a similar game with stronger movement and technical mechanics. At the time of this writing, you can see my scratch notes of a Game Design Document [here](https://hickory-bamboo-ea4.notion.site/Brawler-48147dfaabd54280aad6ffeda98289f1?pvs=4).


# Development
## Initializing Project in Unity
- [Setting Up an Existing Repo in Unity](https://stackoverflow.com/a/71301724/3600382)

## Development Workflow
Check out [this tutorial](https://www.youtube.com/watch?v=XtQMytORBmM) to get started with the development loop.

## Gameplay Testing Primer
At the time of this writing, I'm still tinkering around with the system mechanics of the game. See the GDD mentioned above for details on implementation progress. As it relates to getting started with testing, here's a quick overview:
- The game is a top-down fighting game, where you play as a character with HP, armor, stamina, and abilities
- Matches are played on a stage, and I'm planning to support 1v1 and 3v3 matches
    - The stage in which I'm doing most tinkering is [Temple Summit](Assets\Scenes\TempleSummit.unity) - load this scene and run it as a sandbox
- Each character has the following abilities, with the specified controls and details:

| Ability          | Control                                            | Details                                                                                          |
|------------------|----------------------------------------------------|--------------------------------------------------------------------------------------------------|
| Base Movement    | WASD                                               |                                                                                                  |
| Special Movement | Alt                                                | Each character will have at least one movement ability, costing stamina                          |
| Attacks          | LMB,RMB,MMB (Shift+)LMB,RMB,MMB (Ctrl+)LMB,RMB,MMB | 3 light attacks, 3 medium attacks, 3 heavy attacks The MMB attacks are special, and cost stamina |
| Block            | Q                                                  | Blocking activates some particular movement physics, WIP                                         |
| Throws           | E, R, T                                            | Throw with T is special, and costs stamina                                                       |
| Abilities        | 1,2,3,4,Shift+1,Shift+2,F                          | Shift+1, Shift+2, F are special, special, and ultimate, and they cost energy                     |

- For testing, I've also provided the following additional controls:
    - Numpad Asterisk (*) - switch to other character
    - Some other stuff that I can't remember now - see [SceneController.cs](Assets\Scenes\SceneController.cs)

_This project is not yet stable, so things will definitely be broken and wonky!_