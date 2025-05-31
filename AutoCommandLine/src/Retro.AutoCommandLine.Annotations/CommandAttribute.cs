using System;
namespace Retro.AutoCommandLine.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public class CommandAttribute : Attribute;