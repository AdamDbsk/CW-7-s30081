using System.Runtime.InteropServices;

public class TooMuchDataException(string message) : Exception(message);