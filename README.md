# 🛩️ OpenGL Flight Mini-Game

![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![OpenGL](https://img.shields.io/badge/OpenGL-FFFFFF?style=for-the-badge&logo=opengl)
![Silk.NET](https://img.shields.io/badge/Silk.NET-512BD4?style=for-the-badge)

> A 3D flight simulation mini-game built with **C#** and **Silk.NET**. This project serves as both a fun landing challenge and a practical demonstration of advanced OpenGL rendering techniques.

---

## 📖 Overview

Dive into the fascinating world of graphics programming! This project utilizes the **Silk.NET** library to create a high-performance OpenGL application.

The game places you in the cockpit of a plane with a simple but difficult objective: **Land the plane safely.** Along the way, the project demonstrates core computer graphics concepts, making it an excellent resource for developers looking to understand the "how" and "why" behind game engines.

### 🛠 Tech Stack
* **Language:** C# (.NET 6/7/8)
* **Graphics API:** OpenGL 3.3+ (Core Profile)
* **Windowing & Input:** Silk.NET

---

## ✨ Key Features

### 🎨 Visuals & Rendering
* **High-Fidelity Graphics:** Renders 3D models with custom vertex and fragment shaders.
* **Advanced Lighting:** Implements realistic lighting models (Ambient, Diffuse, Specular).
* **Texture Mapping:** High-quality texture support for terrain and vehicle assets.

### 🕹️ Gameplay
* **Flight Mechanics:** Physics-based movement for realistic plane handling.
* **Smooth Controls:** Responsive keyboard and mouse integration using Silk.NET's Input API.
* **The Challenge:** Navigate the environment and execute a precision landing.

### 📚 Educational Value
This codebase is structured to help you learn:
* **Shaders:** How to write GLSL vertex and fragment shaders.
* **Matrices:** Understanding Model, View, and Projection (MVP) transformations.
* **Camera:** How to implement a 3D Euler-angle camera system.

---

## 🎮 Controls

| Input | Action               |
| :--- |:---------------------|
| **A / D** | Roll Left / Right    |
| **Space** | Plane landing        |
---

## ⚙️ Installation & Setup

### Prerequisites
1.  **.NET SDK:** Version 6.0 or higher.
2.  **Graphics Card:** Must support OpenGL 3.3 or higher.
3.  **IDE (Optional):** JetBrains Rider (Recommended) or Visual Studio.

### 🚀 How to Run

You can run the simulation via the command line or your preferred IDE.

#### Option A: Using the Command Line (CLI)
1.  **Clone the Repository**
    ```bash
    git clone https://github.com/levilevente/hijacking.git
    ```
2.  **Navigate to the Folder**
    ```bash
    cd hijacking
    ```
3.  **Run the Game**
    ```bash
    dotnet run
    ```

#### Option B: Using JetBrains Rider
1.  **Open the Project**
    * Launch **Rider**.
    * Click **Open** and select the `.sln` (Solution) file inside the cloned folder.
2.  **Run the Game**
    * Wait for Rider to restore the NuGet packages (Silk.NET dependencies).
    * Select the correct **Run Configuration** (usually the project name) in the top toolbar.
    * Click the green **Play** button (or press `Shift+F10` / `^R`).

---

## 📸 Screenshots


---