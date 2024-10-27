
![RoboRacing](RoboRacing.png)

# RoboRacing
Side scroller game with procedurally generated levels and AI to compete with.

## Installation

Use the following command to create the required conda environment:

```bash
conda env create -f environment.yml
```

Activate the new conda environment from terminal:

```bash
conda activate torch
```

## Training the AI Model

From the root directory (cd to it or open it there), use the following command to train the model:

```bash
mlagents-learn config/ppo_config.yaml --run-id=PlatformerRun --resume
```

## File Descriptions

### `SpawnPoint.cs`
Handles the spawning logic of the player at designated spawn points in the game. Ensures the player respawns at the correct position after death or when restarting the game.

### `PlayerMovement.cs`
Manages the player's movement, including running, jumping, and wall jumping mechanics. It processes input for controlling horizontal movement, checks if the player is grounded, and allows jumping when on the ground or wall.

### `PlayerDeath.cs`
Handles player death and respawn logic. When the player collides with specific objects (such as the death zone), this script manages the player's death and triggers the respawn process at a predefined spawn point. Currently not in use.

### `PlatformerAgent.cs`
This script integrates with the ML-Agents toolkit to define the AI agent's behavior. It handles agent observations, action processing, reward mechanisms, and episode resets. The AI agent uses this to navigate the platformer world and learn optimal behaviors through reinforcement learning.

### `DeathZone.cs`
Defines the behavior for the death zones in the game. When the player or AI agent enters a death zone, this script triggers death and respawn logic (or ends the episode for the AI agent).

### `CameraController.cs`
Controls the movement of the game camera, ensuring it follows the player or AI agent as they traverse through the level. It provides a smooth camera experience by tracking player movement.

### `ppo_config.yaml`
Configuration file for training the AI model using Proximal Policy Optimization (PPO) in Unity ML-Agents. This file contains hyperparameters such as the learning rate, batch size, and other parameters required for the training process.

### `environment.yml`
Specifies the Conda environment configuration, including all required dependencies to run the game and train the AI. This file allows users to set up the environment by installing all necessary packages, ensuring compatibility across different systems.
