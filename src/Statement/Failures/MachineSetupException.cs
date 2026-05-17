using System;

namespace Statement.Failures;

public class MachineSetupException(string message) : Exception(message);