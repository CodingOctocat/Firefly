using Firefly.Models;

namespace Firefly.Factories;

public interface IFireTableColumnMappingWindowFactory
{
    FireTableColumnMappingDialogResult ShowDialog(FireTableColumnMapping? columnMapping = null, string? filePath = null);
}
