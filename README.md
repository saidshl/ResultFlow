# SimpleResult

## Overview

SimpleResult is a project aimed at providing a straightforward mechanism for handling operation results in a clean and efficient manner. This project helps in managing success and error states in a unified way.

## Features

- Handles success and error results uniformly.
- Provides clear and concise result handling.
- Easy to integrate into existing projects.

## Installation

To install SimpleResult, you can download the source code or clone the repository using:

```bash
dotnet add package Shl.SimpleResult --version 1.0.1-beta

## Usage Example

Here is an example of how to use the `Result<TValue>` class:

```csharp
int result = Result<int>.Success(100);
