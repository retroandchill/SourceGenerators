using System;
#if AUTO_COMMAND_LINE_GENERATOR
using RhoMicro.CodeAnalysis;
#endif

namespace Retro.AutoCommandLine.Attributes;

[AttributeUsage(AttributeTargets.Method)]
#if AUTO_COMMAND_LINE_GENERATOR
[IncludeFile]
#endif
internal class CommandHandlerAttribute : Attribute;