<h1 align="center">LogFlake Client .NET Core <img alt="Version" src="https://img.shields.io/badge/version-1.0.0-blue.svg?cacheSeconds=2592000" /></h1>

> This repository contains the sources for the client-side components of the LogFlake product suite for applications logs and performance collection for .NET applications.

<h3 align="center">🏠 [LogFlake Website](https://logflake.io) |  🔥 [CloudPhoenix Website](https://cloudphoenix.it)</h3>

## Downloads

|                                  NuGet Package Name                                   |                                 Version                                  |                                  Downloads                                  |
|:-------------------------------------------------------------------------------------:|:------------------------------------------------------------------------:|:---------------------------------------------------------------------------:|
| [LogFlake.Client.NetCore](https://www.nuget.org/packages/LogFlake.Client.NetCore) | ![NuGet Version](https://img.shields.io/nuget/v/logflake.client.netcore) | ![NuGet Downloads](https://img.shields.io/nuget/dt/logflake.client.netcore) |

## Usage
Retrieve your _application-key_ from Application Settings in LogFlake UI.
```csharp
const logger = new LogFlake("application-key");
logger.SendLog(LogLevels.INFO, "Hello World");
```
