# Wolverine Study Case

## Table of contents

1. Introduction
2. Mixes well with current setup
2. Integration with NServiceBus
3. What can Wolverine do more?
4. We can take it even further!
5. Help with setup
6. CritterWatch
7. Conclusion

## Introduction

Wolverine is a .NET library for building distributed applications. It provides a simple and powerful way to build microservices, message-based applications, and more. In this study case I'll show a lot of the features that I think make Wolverine a better tool than NServiceBus for building distributed applications. I'm not going to cover every single feature of Wolverine, but I'll focus on the ones that I think are most relevant.

Do know that for general purpose use, Wolverine and NServiceBus are both feature complete for distrubuted computing and that the preference for one or the other is mostly a matter of taste. So unless it is something very exotic that you are trying to do, NServiceBus can be replaced by Wolverine without any issues.

## 1. Mixes well with current setup

Wolverine can be used alongside NServiceBus without any issues. You can use Wolverine for new projects while still using NServiceBus for existing projects. This allows you to gradually migrate to Wolverine without having to rewrite everything at once.

It also has an HTTP endpoint package that supports vertical slicing of endpoints, which is a great way to organize your code. Wolverine endpoints can run side by side with other technologies such as ASP.NET Core, which means you can use it for new features while still maintaining your existing codebase.

## 2. Integration with NServiceBus

It's possible to send messages from Wolverine to NServiceBus and vice versa. This means that you can use Wolverine for new features while still using NServiceBus for existing features. This allows you to gradually migrate to Wolverine without having to rewrite everything at once.

I've set up a simple example where I can send a message from a Wolverine endpoint to an NServiceBus handler and there is a Wolverine handler that listens for messages sent by the NServiceBus instance. I've set this up with existing clean architecture and constructor injection to demonstrate that Wolverine supports your current way of writing code.

## 3. What can Wolverine do more?

Now that we know that we can keep writing our code the way we are used to, let's find out what Wolverine offers on top. I've created a saga with a simple flow: a request is sent, then there is a timeout, and if the request is not approved within the timeout, it will be automatically rejected. I'm using Entity Framework to store the saga state.

There are a few differences here, first of all, I'm using method injection instead of constructor injection. Every handler method gets injected what it needs, other methods aren't impacted. When testing this, I can call the method directly and only supply it what it needs. No endless setup of all the other dependencies that the class needs, but aren't used in the method under test. This is a huge improvement in testability and makes it much easier to write unit tests for your handlers.

Next is what I return. This isn't a simple Task, but a Tuple of several different things. I can return outgoing messages, code that handles side effects such as updating the database and writing to files and in endpoints even an HTTP response. This doesn't just make testing easier because I don't need to catch my side effect code (no more mocking of the database or file system), but it also makes it easier to read and understand what the handler is doing. Over time I've seen this promote reuse of side effect code because it's just a simple method that can be called from anywhere, instead of being buried in the middle of a handler method.

There are a few things to take into consideration. Side effects that can fail, such as network call, are better handled in a dedicated handler that listens for a command than a Wolverine side effect. This is because if the side effect fails, the whole handler will fail and the message will be retried. This can lead to a lot of retries and can cause issues if the side effect is something that can fail frequently. By using a dedicated handler for the side effect, you can handle failures more gracefully and avoid unnecessary retries.

The other thing to consider is that we are using a timed message to trigger the timeout. This can lead to a lot of errors in the logs because the timed message is picked up, but the saga might have been removed after completion. All you need to do is add an empty `NotFound` handler for the timed message. This is the recommended approach by the documentation and it works well. The timed message will be picked up, but since there is a handler for it, it won't cause any errors in the logs.

## 4. We can take it even further!

Using the saga, I've showed you how Wolverine can help you write easier handler code, but we can take this even further. A first example is the `ApproveHandler` where the saga will automatically be stopped when it is approved. Wolverine supports supports several methods that can be added to a handler class that will execute in a certain order:

1. Load
2. Validate
3. Before
4. Handle
5. After
6. Finally

The load method can be used to load any data that is needed for the handler. Database calls, file system reads, network calls. All data can be returned as a tuple or a deconstructable record and be injected piece by piece into other methods. In this case, the validate method takes the loaded saga and a the current date and time to validate whether this saga can be correctly stopped.

When a `Finally` method is added, it will be executed regardless of whether the handler succeeded or failed. This is a great place to put any cleanup code that needs to run after the handler has finished, just like a `finally` block in a try-catch statement.

In the `DenyHandler`, I stop the saga when I deny the request. What I want to focus on here is that there is no `Load` method, but I still get the saga injected into the `Handle` method. The `[Entity]` attribute tells Wolverine to load the saga from the database and inject it into the `Handle` and `Validate` methods.

All these features help cut down on boilerplate and make testing easier. It lets you focus on the logic of your handlers. This comes at a cost of needing to learn a few new concepts and ways of doing things, but I think the benefits outweigh the costs in the long run.

## 5. Help with setup

When I add JasperFX command line support by replacing the `app.Run();` with `await app.RunJasperFxCommands(args);` I get a lot of new commands that can help with Wolverine setup, source code generation and more.

### Commands to run:
```powershell
dotnet run -- help
dotnet run -- describe # describes the current Wolverine setup, including all handlers, sagas, etc.
dotnet run -- diagnose # runs a diagnostic check on the current Wolverine setup and reports known issues and suggestions for improvement.
dotnet run -- resources list # lists all resources that are registered in the current Wolverine setup, including handlers, sagas, etc.
```

## 6. CritterWatch

Now I want to be a little careful with this one, as it's still in early stages of development and I haven't had the chance to test it out yet, but it looks very promising. It's a dashboard that can be used to monitor your Wolverine application in real time. It can show you all the messages that are being processed, the handlers that are being executed, the sagas that are being updated, and more. It also has a lot of features for debugging and troubleshooting your application.

Because this is in beta, there are some limitations and not everything is working perfectly. For example, this dashboards needs a postgres database to work. I believe that this will be changed by the time v1 is released. And to note, the more advanced features will be only for companies with a paid license, but the basic monitoring features will be available for everyone.

## 7. Conclusion

I could go on about how many transports Wolverine supports, that there are LLM friendly docs, how it has made my life easier in the past and how it's only gotten better over time, but I think the best way to find out if it's a good fit for you is to try it out yourself. It's easy to set up and you can have a simple endpoint up and running in no time. The documentation is very good and there are a lot of examples available to help you get started.