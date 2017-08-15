MediatR
=======

[![Build Status](https://ci.appveyor.com/api/projects/status/github/jbogard/mediatr?branch=master&svg=true)](https://ci.appveyor.com/project/jbogard/mediatr) 
[![NuGet](https://img.shields.io/nuget/dt/mediatr.svg)](https://www.nuget.org/packages/mediatr) 
[![NuGet](https://img.shields.io/nuget/vpre/mediatr.svg)](https://www.nuget.org/packages/mediatr)

Simple mediator implementation in .NET

In-process messaging with no dependencies.

Supports request/response, commands, queries, notifications and events, synchronous and async with intelligent dispatching via C# generic variance.

Examples in the [wiki](https://github.com/jbogard/MediatR/wiki).

### Installing MediatR

You should install [MediatR with NuGet](https://www.nuget.org/packages/MediatR):

    Install-Package MediatR
    
Or via the .NET Core command line interface:

    dotnet add package MediatR

Either commands, from Package Manager Console or .NET Core CLI, will download and install MediatR and all required dependencies.
