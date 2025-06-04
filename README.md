# SimpleWebSocketServer.SIBS.Front

This project is a .NET-based WebSocket client designed to interface with SIBS payment terminals. It allows communication between a frontend system (such as a POS) and payment terminals through WebSocket protocols, supporting a range of operations including terminal configuration, transaction initiation, and data exchange.

## Features

- WebSocket client for communication with SIBS terminal servers
- Modular structure with models for common request/response formats
- Easy integration with POS and other frontend systems
- Support for:
  - Terminal configuration
  - Transaction management
  - Card and customer data exchange
- NuGet packaging support for easy distribution

## Project Structure

- `FrontClient.cs` – Core WebSocket communication logic
- `Models/` – Data models for various requests and responses (e.g. `AmountData`, `CardData`, `CustomerData`)
- `Enums/` – Common enums used throughout the project
- `App.config` – Configuration file
- `.nuspec` and `.cmd` – NuGet packaging support

## Getting Started

### Prerequisites

- .NET Framework (compatible with the target version in `.csproj`)
- Visual Studio (recommended)
- A running WebSocket server (e.g., SimpleWebSocketServer backend)

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/your-org/SimpleWebSocketServer.SIBS.Front.git
   ```

2. Open `SimpleWebSocketServer.SIBS.Front.sln` in Visual Studio.

3. Build the solution.

4. Reference the compiled library or package it via NuGet using:
   ```bash
   nugetPackagePush.cmd
   ```

## Usage

You can instantiate and use the `FrontClient` class to communicate with SIBS terminals. Here's an example:

```csharp
var client = new FrontClient("ws://yourserver:port");
client.Connect();

// Send a request
var configRequest = new ConfigTerminalReq { TerminalId = "12345" };
client.Send(configRequest);

// Handle responses...
```

## License

This project is licensed under the terms described in `LICENSE.txt`.

## Support

For support, questions, or suggestions, please [open an issue](https://github.com/yourusername/SimpleWebSocketServer.SIBS.Front/issues).
