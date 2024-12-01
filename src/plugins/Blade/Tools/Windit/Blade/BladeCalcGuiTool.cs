using DevToys.Api;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Blade.Tools.Windit.Blade;
[Export(typeof(IGuiTool))]
[Name("BladeCalc")]
[ToolDisplayInformation(
    IconFontName = "DevToys-Tools-Icons",
    IconGlyph = '\u0137',
    GroupName = "Blade",
    ResourceManagerAssemblyIdentifier = nameof(BladeResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "Blade.Tools.Windit.Blade.BladeCalc",
    ShortDisplayTitleResourceName = nameof(BladeCalc.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(BladeCalc.LongDisplayTitle),
    DescriptionResourceName = nameof(BladeCalc.Description),
    AccessibleNameResourceName = nameof(BladeCalc.AccessibleName))]
internal sealed class BladeCalcGuiTool : IGuiTool
{
    /// <summary>
    /// Whether the generated UUID should be uppercase or lowercase.
    /// </summary>
    private static readonly SettingDefinition<string> host
        = new(
            name: $"{nameof(BladeCalcGuiTool)}.{nameof(host)}",
            defaultValue: "127.0.0.1");
    private readonly IUIDataGrid _outputDataGrid = DataGrid("output-data-grid");
    private readonly IUIFileSelector _fileSelector = FileSelector("blade-calc-file-selector");
    private readonly IUIPasswordInput _password = PasswordInput("blade-calc-password-input");
    private readonly ISettingsProvider _settingsProvider;
    [ImportingConstructor]
    public BladeCalcGuiTool(ISettingsProvider settingsProvider)
    {



        _settingsProvider = settingsProvider;

        OnGenerateButtonClick();
    }
    private enum GridRows
    {
        Settings,
        Results
    }

    private enum GridColumns
    {
        Stretch
    }
    public UIToolView View
        => new(
            isScrollable: true,
            Grid()
            .ColumnMediumSpacing()
            .RowLargeSpacing()
            .Rows(
                    (GridRows.Settings, Auto),
                    (GridRows.Results, new UIGridLength(1, UIGridUnitType.Fraction)))
            .Columns(
                (GridColumns.Stretch, new UIGridLength(1, UIGridUnitType.Fraction)))
            .Cells(
                Cell
                (
                    GridRows.Settings,
                    GridColumns.Stretch,
                    Stack()
                        .Vertical()
                        .LargeSpacing()
                        .WithChildren(
                            Stack().Vertical().WithChildren
                            (
                                Label().Text("设置"),
                                _fileSelector
                                .LimitFileTypesTo("json")
                                            .OnFilesSelected(OnFileSelectedAsync),
                                SettingGroup().Title("配置库设置").WithChildren(
                                    Setting().Title("IP"),
                                    Setting().Title("Port"),
                                    Setting().Title("User"),
                                    Setting().Title("Password")
                                ),
                                SettingGroup().Title("数据库设置").WithChildren(
                                    Setting().Title("IP"),
                                    Setting().Title("Port"),
                                    Setting().Title("User"),
                                    Setting().Title("Password")
                                )
                            )
                        )
                ),
                Cell
                (GridRows.Results,
                    GridColumns.Stretch,
                    _outputDataGrid.Title("数据展示").Extendable()
                    .AllowSelectItem()
                    .OnRowSelected(OnSelectionChanged)
                )
            ));




    private void OnGenerateButtonClick()
    {

    }
    private void OnExcludedCharactersChanged(string value)
    {
        OnGenerateButtonClick();
    }
    public void OnDataReceived(string dataTypeName, object? parsedData)
    {

    }
    private void OnSelectionChanged(IUIDataGridRow? selectedRow)
    {
        Console.WriteLine(selectedRow);
    }
    private async ValueTask OnFileSelectedAsync(SandboxedFileReader[] files)
    {
        Guard.HasSizeEqualTo(files, 1);

        using SandboxedFileReader selectedFile = files[0];
        using var memStream = new MemoryStream();

        await selectedFile.CopyFileContentToAsync(memStream, CancellationToken.None);

        byte[] bytes = memStream.ToArray();
        var test = JsonSerializer.Deserialize<List<LeafConfig>>(bytes);
        _outputDataGrid.WithColumns(["组织名", "机组编号", "机组名", "1KHz测点", "50Hz测点", "固有频率", "自定义频率"]);
        List<IUIDataGridRow> Rows = [];
        Rows.AddRange(from node in test
                      select Row(node.MachineId, node.TreeName, node.MachineName, node.MachineName, node.PositionIdL, node.PositionNameS, node.NaturalFreqs, node.CustomFreqs));
        _outputDataGrid.WithRows([.. Rows]);

    }
    public record class LeafConfig
    {
        [DisplayName("机组id")]
        public string MachineId { get; set; }
        [DisplayName("组织名称")]
        public string TreeName { get; set; }
        [DisplayName("机组名称")]
        public string MachineName { get; set; }

        [DisplayName("20采样测点")]
        public string PositionIdS { get; set; }

        [DisplayName("20采样测点名称")]
        public string PositionNameS { get; set; }

        [DisplayName("1k采样测点")]
        public string PositionIdL { get; set; }

        [DisplayName("1k采样测点名称")]
        public string PositionNameL { get; set; }

        [DisplayName("固有频率")]
        public string NaturalFreqs { get; set; }

        [DisplayName("自定义频率")]
        public string CustomFreqs { get; set; }
    }
    internal record DataGridContents(string[] Headings, List<string[]> Rows);
}
