using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using HandlebarsDotNet;
using HandlebarsDotNet.IO;
using IFormatter = HandlebarsDotNet.IO.IFormatter;
namespace Retro.SourceGeneratorUtilities.Formatters;

public class EnumStringValueFormatter : IFormatter, IFormatterProvider {

  public void Format<T>(T value, in EncodedTextWriter writer) {
    if (value?.GetType().IsEnum != true) throw new NotSupportedException(typeof(T).ToString());

    writer.Write(value.GetType()
                     .GetTypeInfo()
                     .DeclaredMembers
                     .SingleOrDefault(x => x.Name == value?.ToString())
                     ?.GetCustomAttribute<EnumMemberAttribute>(false)
                     ?.Value);
  }
  
  public bool TryCreateFormatter(Type type, out IFormatter? formatter) {
    if (!type.IsEnum) {
      formatter = null;
      return false;
    }

    formatter = this;
    return true;
  }
}