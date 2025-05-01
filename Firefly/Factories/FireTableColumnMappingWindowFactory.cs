using System;

using Firefly.Helpers;
using Firefly.Models;
using Firefly.ViewModels;
using Firefly.Views;

using HandyControl.Controls;

using Microsoft.Extensions.DependencyInjection;

namespace Firefly.Factories;

public class FireTableColumnMappingWindowFactory : IFireTableColumnMappingWindowFactory
{
    public FireTableColumnMappingDialogResult ShowDialog(FireTableColumnMapping? columnMapping = null, string? filePath = null)
    {
        var vm = new FireTableColumnMappingViewModel(columnMapping, filePath);
        var window = new FireTableColumnMappingWindow(vm);

        var mainWindow = App.Current.Services.GetRequiredService<MainWindow>();
        mainWindow.Hide();
        window.ShowDialog();
        mainWindow.Show();

        if (columnMapping is not null && vm.DialogResult is true)
        {
            Growl.Success($"表列映射保存成功。\n> {PathHelper.GetRelativePathOrOriginal(vm.MappingFilePath)}");
        }
        else if (vm.DialogResult is null)
        {
            Growl.Info($"已删除表列映射。\n> {PathHelper.GetRelativePathOrOriginal(vm.MappingFilePath)}");
        }

        return new FireTableColumnMappingDialogResult(vm.DialogResult, vm.ColumnMapping, vm.MappingFilePath);
    }
}
