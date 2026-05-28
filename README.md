# Spin Rogue

## 1. Overview

**Spin Rogue** is a terminal-based roguelike slot machine combat game implemented in F# (.NET 10) for the CS-20200 Term Project.

The player's objective is to clear as many rounds as possible, up to 10 rounds. In each round, the player must reduce the opponent monster's HP to 0 while surviving without their own HP reaching 0. Every turn, the player spins their slot machine and deals a randomized amount of damage to the enemy based on the result. After defeating an enemy, the player chooses a reward to strengthen their slot machine before proceeding to the next round.

## 2. How to Run

This game is a console application written in F# using .NET 10. To run the game, follow these steps:

1. Ensure you have the **.NET 10 SDK** installed on your system.
2. Clone or download this repository.
3. Open your terminal and navigate to the project root directory.
4. Run the following command:
```bash
cd game
dotnet run

```
5. Follow the on-screen terminal instructions to play the game (Press `Enter` to spin, enter numbers to select rewards).

## 3. Requirements (Project Specification)

1. The player can check the current round, the enemy's element and HP, their own HP, and the current reward status in the terminal.
2. Each round consists of a sequence: Player's Turn -> Enemy's Turn -> Player's Turn -> Enemy's Turn, and so on. On the player's turn, the player presses the `Enter` key to spin the slot machine. The player's initial slot machine reels consist of 8 Water symbols, 8 Fire symbols, 8 Grass symbols, and 1 Prism symbol.
3. Once the slot machine spin ends, a total of 15 symbol results are printed to the terminal in a 3x5 grid format.
4. The damage for the resulting 15 symbols is calculated based on their elemental matchups against the enemy. The base damage for each element is the same, but the player can increase it through rewards. A fixed damage multiplier is applied: `2.0x` for an elemental advantage, `0.5x` for a disadvantage, and `1.5x` for the Prism symbol. After calculating the damage for each element by multiplying it by these weights, the enemy's HP is reduced by the sum of the damages from up to the top 2 highest-damaging elements.
5. If the damage from step 4 fails to reduce the enemy's HP to 0, the enemy counterattacks, reducing the player's HP by the enemy's attack power. The player cannot explicitly know the enemy's attack power before that. If the player's HP reaches 0, the game ends.
6. After repeating steps of turns, if the player defeats the enemy, they can choose a reward. The player selects a reward by entering the number corresponding to the given options. Choosing a reward cannot be skipped. Once a reward is chosen, the player proceeds to the next round.
7. After every 3 rounds, the enemies' element changes to a different one. Every 3 rounds, the enemies' HP and attack power increase further. The 10th round is a Boss Round, where an enemy with significantly higher HP and attack power appears.
8. If the player clears all 10 rounds, the game ends in a victory for the player.

## 4. Example Interaction
The game outputs the current round: `1`, enemy information (Element: `Grass`, HP: `30`), and the player's current and maximum HP (`30/30`) to the terminal. The system prompts the player to spin the slot machine. 
The player presses the `Enter` key. If the player does not press the `Enter` key, the game waits until the `Enter` key is pressed. The game prints a 3x5 symbol grid (slot result):

Grass Grass Water Fire  Fire
Fire  Grass Fire  Water Grass
Grass Water Grass Water Water

With 5 Fire symbols, 6 Grass symbols, and 4 Water symbols, the enemy's HP is reduced by a total of 5x2 + 6x1 = 16. Since the enemy's remaining HP is 14 (greater than 0), the enemy counterattacks and reduces the player's HP by 5.
The game outputs the updated enemy HP (14) and player HP (25), and prompts the player to spin the slot machine again. When this process repeats and the enemy's HP reaches 0, the game outputs reward options. If an invalid reward option number is entered, a message prompts the player to enter a valid one.
The player enters `1`, changing 3 Water symbols in their reels to random element symbols: Grass, Fire, and Fire. Since the player's HP is greater than 0 and the cleared round is not the 10th round, the game outputs the updated round information.

## 5. Requirement Changes & Justification

* **Changes:** None. The final implementation fully conforms to the original proposal submitted on May 4, 2026.

## 6. Use of Large Language Models (LLM)

