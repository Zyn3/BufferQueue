# BufferQueue

BufferQueue is a .NET library designed to facilitate efficient and concurrent handling of buffer queues for various types of data processing tasks. It provides a robust infrastructure for managing background tasks, enabling seamless enqueueing and dequeueing of work items in a thread-safe manner.

## Features

- **Concurrent Buffer Management**: Utilizes `ConcurrentBuffer<T>` to manage data in a thread-safe way, ensuring efficient processing of items.
- **Dual Buffer Support**: Offers the `DualBufferQueueMonitor<TSendType, TReturnType>` class for managing two types of buffer queues, suitable for scenarios involving both sending and receiving data.
- **Asynchronous Task Processing**: Leverages asynchronous programming models to handle background tasks effectively, improving the scalability of applications.
- **Extensible Design**: Abstract classes and interfaces like `IBufferQueueWorkProvider<TBufferType>` allow for easy extension and customization to fit various use cases.
- **Integration with Hosted Services**: Implements `IHostedService` in relevant components, ensuring smooth integration with .NET hosted services architecture.

## Installation

To use BufferQueue in your project, you can clone this repository and include the relevant projects in your solution. Follow these steps:

1. Clone the repository:

```
git clone https://github.com/Zyn3/BufferQueue.git
```

2. Add the project(s) to your solution using Visual Studio or another IDE of your choice.

3. Reference the project in your application where you intend to use buffer queue functionalities.

## Usage

### Setting Up a Single Buffer Queue

```csharp
// Example of setting up a single buffer queue
var services = new ServiceCollection();
services.SetupBufferQueue<YourDataType>(new YourBufferQueueWorkProvider(), maxQueueSize: 100);
// ... other service configurations
```

### Setting Up a Dual Buffer Queue
```csharp
// Example of setting up a dual buffer queue
var services = new ServiceCollection();
services.SetupDualBufferQueue<SendDataType, ReturnDataType>(
    new YourSendBufferWorkProvider(), 
    new YourReturnBufferWorkProvider(), 
    maxQueueSize: 100);
// ... other service configurations
```

# Contributing
Contributions to the BufferQueue project are welcome. If you have a feature request, bug report, or a pull request, please open an issue or a pull request on this repository.

Please ensure your contributions adhere to the following guidelines:

* Write clean, maintainable, and testable code.
* Follow existing coding styles and practices.
* Include unit tests for new features or bug fixes.
* Update the README.md if necessary.

License
BufferQueue is licensed under the MIT License.