
![RoboRacing](RoboRacing.png)

# RoboRacing
Side scroller platforming game with procedurally generated levels and AI to compete with. Created by Chris Harvey, Henry Chaffin, Ian Collins, Kenny Meade, & Ryan Strong.

## Installation

Use the following command to create the required conda environment:

```bash
conda env create -f environment.yml
```

Activate the new conda environment from terminal:

```bash
conda activate unity
```

## Training the AI Model

From the root directory (cd to it or open it there), use the following command to train the model:

```bash
mlagents-learn config/ppo_config.yaml --run-id=PlatformerRun --resume
```

## File Descriptions

### `Scripts`

#### `environment.yml`
Specifies the Conda environment configuration, including all required dependencies to run the game and train the AI. This file allows users to set up the environment by installing all necessary packages, ensuring compatibility across different systems.

#### `MenuManager.cs`
Script to handle the menu configurations and game intialization given the configurations. Each change is handled with an event listener which sets up the variables change game modes.

#### `MusicManager.cs`
Intiates the music to play and ensures only one music manager exists at any given time

#### `Parallax.cs`
Script used to illustrate and determine how and where the player has moved as time goes on. This is a constantly updating structure which informs camera movement.

#### `Player2Movement.cs`
For the co-op mode, this script handles the player2 physics and inputs. The physics is identical to the AI and player1 however the player is controlled using the arrow key instead of 'wasd' keys.

#### `CoOpInitialize.cs`
Creates and manages the 2nd player when the Co-Op mode is selected. Initialization occurs before the game loads from the menu and is updated every frame as the 2nd player interacts with the game.

#### `CameraController.cs`
Controls the movement of the game camera, ensuring it follows the player or AI agent as they traverse through the level. It provides a smooth camera experience by tracking player movement.

### `Scripts/Level`

#### `SpawnPoint.cs`
Handles the spawning logic of the player at designated spawn points in the game. Ensures the player respawns at the correct position after death or when restarting the game.

#### `DeathZone.cs`
Defines the behavior for the death zones in the game. When the player or AI agent enters a death zone, this script triggers death and respawn logic (or ends the episode for the AI agent).

#### `flagHandler.cs`
Attaches the correct audio to collision events between the player, platforms, and enemies. The script also loads the correct sound effects into the game resources.

#### `FreeRun.cs`
Handles the procesdural generation for the free run mode, so the prerendered chunks of platforms and dangers are loaded initially and put together until the player dies or stops. The procedural generation cleans the chucks that the player has passed once out of camera view and increases difficulty after certain amounts of chucks have been passed.

#### `LevelManager.cs`
Manages how the levels are built at the varying difficulty designed into the levels. Utilized for training the AI model on procedurally generated levels as after a certain number of levels are completed, the difficulty increases. This script also initializes and tracks the state of the levels whether tracking the progress or checking if the level was completed. 

#### `ProcGen.cs`
Holds all of the functions and methods to validate and create procedurally generated levels of track. The difficulty level is built into these functions as a range of probability to increase difficulty in jumps and enemy spawns.

#### `SpawnPoint.cs`
This function allows for the respawn functionality which is handled mostly by the Unity Engine itself given the already established parameters.

### `Scripts/Player`

#### `PlayerMovement.cs`
Manages the player's movement, including running, jumping, and wall jumping mechanics. It processes input for controlling horizontal movement, checks if the player is grounded, and allows jumping when on the ground or wall.

#### `PlayerDeath.cs`
Handles player death and respawn logic. When the player collides with specific objects (such as the death zone), this script manages the player's death and triggers the respawn process at a predefined spawn point. Currently not in use.

#### `PlatformerAgent.cs`
This script integrates with the ML-Agents toolkit to define the AI agent's behavior. It handles agent observations, action processing, reward mechanisms, and episode resets. The AI agent uses this to navigate the platformer world and learn optimal behaviors through reinforcement learning.

#### `LivesUI.cs`
Handles the transition from the game after the player dies to return back to the main menu. The death animation will play, and transition back to the main menu given the state of the game.

### `Scripts/Enemies`

#### `EnemyBulletPhysics.cs`
Creates the physics of the bullets as they move across the screen handling the projectile time limit or collition with player or ground.

#### `EnemyMovement.cs`
Describes the different movement behaviors an enemy may have like jump in place, walk back and forth, or stand still. Sets many of the same physics parameters while updating the animations reflecting that movement.

#### `EnemyShooting.cs`
Handles the animation and sound effect of the shooting enemy with the shots happening after a certain time interval.

### `Scripts/Powerups`

#### `AIStop_Powerup.cs`
Defines the behavior of the stop AI player powerup while handling the collision and change in state of the AI player

#### `Blinder_Powerup.cs`
Defines the behavior of the blinder powerup (or more accurately hazard) while handling the collision. This powerup blocks the screen from the player for a certain time interval like the Ink in Mario Kart.

#### `Invicible_Powerup.cs`
Defines the behavior of the invicible powerup while handling the collision. This powerup makes the player who collides with it invicible for a given time interval.

### `config`

#### `ppo_config.yaml`
Configuration file for training the AI model using Proximal Policy Optimization (PPO) in Unity ML-Agents. This file contains hyperparameters such as the learning rate, batch size, and other parameters required for the training process.

### `ML-Agents`
Handles and stores the different images of the machine learning models we created during the process of making this game. Each model has a different amount of time spent training as to create the sense of difficulty increasing. The hardest model trained for ~6 weeks.

### `Animation|Images|Resources|Levels|Sound|Sprites|TextMesh Pro|Tilemaps|Settings`
Directories for all of the games resources such as textures, sounds/music, animations, level blocks, and other game configurations

### `Packages`
Contains any information that VSCode, Unity, or C# would need during the compiling and running process of the game.