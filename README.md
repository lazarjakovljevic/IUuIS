# Network Service

A desktop application for monitoring electricity consumption and managing a network of meters. The project includes the **Network Service** WPF application and a separate **Metering Simulator** that sends simulated readings.

## Features

- Add, view, and filter smart meters and interval meters
- Visualize the meter network and connect meters by dragging them into the grid
- View measurement history charts and alerts for values outside the allowed range
- Exchange readings with the simulator over TCP on port `25675`
- Undo the most recent action in the main application

## Technologies

- C# and WPF
- .NET Framework 4.7.2
- TCP communication over `localhost:25675`

## Getting started

Open `NetworkService/NetworkService/NetworkService.sln` in Visual Studio and run the **NetworkService** project. The simulator is in `MeteringSimulator/MeteringSimulator.sln`; run it alongside the main application to send simulated readings.

## Screenshots

| Home screen | Meter management |
|:--:|:--:|
| ![Home screen](docs/screenshots/screen-01.png) | ![Meter management](docs/screenshots/screen-02.png) |

| Network with connections | Network layout |
|:--:|:--:|
| ![Network with connections](docs/screenshots/screen-03.png) | ![Network layout](docs/screenshots/screen-04.png) |

| Measurement history | Invalid reading alert |
|:--:|:--:|
| ![Measurement history](docs/screenshots/screen-05.png) | ![Invalid reading alert](docs/screenshots/screen-06.png) |

| Additional measurement history view |
|:--:|
| ![Additional measurement history view](docs/screenshots/screen-07.png) |
