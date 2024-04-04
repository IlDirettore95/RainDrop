# RainDrop

Project for the first assignment of the DBGA course Level 2

Mechanics:
  - Operations (Sum, Sub, Mul, Div, And, Or) will randomly appear (as drops) on screen. The user must solve the operation and submit the solution through an input field
  - If the user solves the golden operation every drop on screen will get "solved" 
  - Each operation scored grants 100 points
  - The user will lose 1 life for each operation not solved in time
  - When the user's life reach 0 it is game over and the current points are saved as "best score"
  - Difficulty (Spawn rates, drops speed, operations' difficulty, etc) increases over time and were handled thourgh a data-driven approach using scriptable objects
  - The game can be paused using the P key
