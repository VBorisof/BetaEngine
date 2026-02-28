using System;

namespace aced.Exceptions;

public class ActorLoadException(string message) : Exception(message);