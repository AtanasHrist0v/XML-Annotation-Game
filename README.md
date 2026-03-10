# XML Annotation Game

An educational 3D game developed with **Unity**, where players annotate geometric objects with their correct names and formulas. The project uses an **XML-based configuration**, which defines the models, annotations, levels, scoring, and player feedback.

## Project Overview

The goal of the game is to support learning in solid geometry through an interactive 3D environment. Players match geometric shapes with their correct:

- names
- surface area formulas
- volume formulas

The gameplay is organized into multiple levels with increasing difficulty.

## Features

- 3D environment with interactive geometric objects
- XML-based game configuration
- annotations loaded from an external XML file
- multiple difficulty levels
- scoring system
- HUD displaying:
  - score
  - time
  - attempts
  - current level
- visual and audio feedback for correct and incorrect choices
- configuration validation before gameplay starts

## Technologies Used

- **Unity 6**
- **C#**
- **XML**
- **TextMesh Pro**
- **Unity UI**
- **Universal Render Pipeline (URP)**

## Project Structure

```text
Assets/
├── Resources/
│   ├── Models/
│   ├── Prefabs/
│   └── Images/
├── Scenes/
│   ├── SplashScene.unity
│   ├── MenuScene.unity
│   └── MainScene.unity
├── Scripts/
│   ├── GameManager/
│   │   ├── GameManager.cs
│   │   ├── GameConfigValidator.cs
│   │   └── XMLParser.cs
│   ├── Player/
│   │   ├── PlayerMovement.cs
│   │   └── Annotate.cs
│   ├── UI/
│   │   └── StopwatchUI.cs
│   ├── MainMenu.cs
│   └── SplashScreen.cs
└── XML.xml
