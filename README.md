# MauiExamApp

This is a .NET MAUI application.

## How to Run

This project is built with .NET MAUI and targets multiple platforms.

### Prerequisites

- .NET 9 SDK or later
- .NET MAUI workload (`dotnet workload install maui`)

### Windows

1. Open `MauiExamApp.sln` in Visual Studio.
2. Set `MauiExamApp` as the startup project.
3. Select the `net9.0-windows10.0.19041.0` target framework from the debug target dropdown.
4. Press F5 or click the Run button to build and run the application on your local machine.

Alternatively, use the .NET CLI:

```powershell
dotnet build -t:Run -f net9.0-windows10.0.19041.0
```

### iOS

1. **On a Mac:** Open `MauiExamApp.sln` in Visual Studio for Mac.
   **On Windows:** Open `MauiExamApp.sln` in Visual Studio and ensure you are paired to a Mac.
2. Set `MauiExamApp` as the startup project.
3. Select an iOS Simulator or a connected device as the debug target.
4. Press F5 or click the Run button.

Alternatively, use the .NET CLI on a Mac:

```bash
dotnet build -t:Run -f net9.0-ios
```

### MacCatalyst

1. Open `MauiExamApp.sln` in Visual Studio for Mac.
2. Set `MauiExamApp` as the startup project.
3. Select `net9.0-maccatalyst` as the debug target.
4. Press F5 or click the Run button.

Alternatively, use the .NET CLI on a Mac:

```bash
dotnet build -t:Run -f net9.0-maccatalyst
```

## Project Structure

- `MauiExamApp.sln`: The main solution file.
- `MauiExamApp/`: The .NET MAUI project.
  - `ViewModels/`: Contains the view models.
  - `Views/`: Contains the XAML views.
  - `Models/`: Contains the application'''s data models.
  - `Services/`: Contains services, such as `DatabaseService`.
  - `Platforms/`: Contains platform-specific code for Windows, Android, iOS, and MacCatalyst.
  - `Resources/`: Contains shared resources like images, fonts, and styles.
