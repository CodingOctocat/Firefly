namespace Firefly.Models;

public class FireTableColumnMappingDialogResult
{
    public FireTableColumnMapping ColumnMapping { get; set; }

    /// <summary>
    /// <see langword="true"/>: 保存；<see langword="false"/>: 关闭；<see langword="null"/>: 删除。
    /// </summary>
    public bool? DialogResult { get; set; }

    public string? FilePath { get; set; }

    public FireTableColumnMappingDialogResult(bool? dialogResult, FireTableColumnMapping columnMapping, string? filePath)
    {
        DialogResult = dialogResult;
        ColumnMapping = columnMapping;
        FilePath = filePath;
    }
}
