# AR Escape Room - Unity Project Setup

An immersive AR/3D Escape Room game template set inside a Mall environment.

## Initial Game Flow

1. **Mall Environment**: The player spawns in the Mall background, facing the entrance of the **"AR Escape Room"**.
2. **Entrance Trap**: As the player approaches and enters the AR Escape Room, an automatic trigger detects entry, slamming the entrance door shut behind them and locking it. The player is now trapped inside!
3. **Exit Door & Keypad**:
   - On the opposite side of the room is the locked Exit Door.
   - Approaching the door displays an interactive **Keypad UI**.
   - Entering an **incorrect password** displays: `INCORRECT PASSWORD! TRY AGAIN`.
   - Entering the **correct password** (default: `1234`) displays: `ACCESS GRANTED!` and automatically unlocks and opens the exit door.

---

## How to Set Up & Run in Unity

1. **Open in Unity**:
   - Launch Unity Hub and click **Add** -> Select this project folder (`c:\SHYAM\ARVRPROJECT\arvr`).
   - Open the project in Unity 2021 LTS, 2022 LTS, 6, or any modern 3D/AR Unity version.

2. **Generate the Scene (One Click)**:
   - In the top menu bar in Unity, click:  
     `Tools` ➔ `AR Escape Room` ➔ `Build Initial Scene`
   - This automatically creates all 3D meshes (Mall, Escape Room walls, hinged Doors, Keypad Canvas UI, Player controller, and Managers) and saves the scene to `Assets/Scenes/AREscapeRoomScene.unity`.

3. **Test in Play Mode**:
   - Press **Play** in Unity.
   - Use **WASD / Arrows** to move and **Mouse** to look around.
   - Walk toward the room entrance ➔ Observe the door closing and trapping you inside.
   - Walk toward the back Exit Door ➔ The Keypad UI appears.
   - Try entering `9999` (Incorrect feedback) and then `1234` (Door unlocks & opens!).

---

## Codebase Architecture

- **`Assets/Scripts/Core/EscapeRoomManager.cs`**: Central game state manager (`MallExploration`, `TrappedInRoom`, `Escaped`).
- **`Assets/Scripts/Doors/DoorController.cs`**: Handles smooth door open/close/lock animations.
- **`Assets/Scripts/Doors/EntranceTrapTrigger.cs`**: Auto-closes & locks the entrance door upon player entry.
- **`Assets/Scripts/Doors/ExitDoorLock.cs`**: Connects password validation to the exit door lock mechanism.
- **`Assets/Scripts/UI/KeypadUI.cs`**: Keypad UI controller for digit input, clear, submit, and error/success feedback animations.
- **`Assets/Scripts/Player/PlayerController.cs`**: First-person player controller.
- **`Assets/Scripts/Editor/SceneSetupBuilder.cs`**: Editor script for automated scene generation.
