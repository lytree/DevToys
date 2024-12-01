using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevToys.Api;

/// <summary>
/// A component that represents a grid that can display data with rows and columns.
/// </summary>
public interface IFluentDataGrid<T> : IUITitledElementWithChildren
{
    /// <summary>
    /// Gets the list of rows displayed in the data grid.
    /// </summary>
    ObservableCollection<T> Rows { get; }

    /// <summary>
    /// Gets the selected row in the data grid.
    /// </summary>
    T? SelectedRow { get; }
    /// <summary>
    /// Gets whether the element can be expanded to take the size of the whole tool boundaries. Default is false.
    /// </summary>
    /// <remarks>
    /// When <see cref="IUIElement.IsVisible"/> is false and that the element is in full screen mode, the element goes back to normal mode.
    /// </remarks>
    bool IsExtendableToFullScreen { get; }

    /// <summary>
    /// Gets an extra interactive content to display in the command bar of the text input.
    /// </summary>
    IUIElement? CommandBarExtraContent { get; }

    /// <summary>
    /// Raised when <see cref="CanSelectRow"/> is changed.
    /// </summary>
    event EventHandler? CanSelectRowChanged;

    /// <summary>
    /// Raised when <see cref="SelectedRow"/> is changed.
    /// </summary>
    event EventHandler? SelectedRowChanged;

    /// <summary>
    /// Raised when <see cref="IsExtendableToFullScreen"/> is changed.
    /// </summary>
    event EventHandler? IsExtendableToFullScreenChanged;

    /// <summary>
    /// Raised when <see cref="CommandBarExtraContent"/> is changed.
    /// </summary>
    event EventHandler? CommandBarExtraContentChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, SelectedRow = {{{nameof(SelectedRow)}}}")]
internal sealed class FluentDataGrid<T> : UITitledElementWithChildren, IFluentDataGrid<T>, IDisposable
{
    private readonly ObservableCollection<T> _rows = new();
    private T? _selectedRow;
    private bool _canSelectRow = true;
    private bool _isExtendableToFullScreen;
    private IUIElement? _commandBarExtraContent;

    internal FluentDataGrid(string? id)
        : base(id)
    {

    }

    public ObservableCollection<T> Rows => _rows;

    public bool CanSelectRow
    {
        get => _canSelectRow;
        internal set
        {
            SetPropertyValue(ref _canSelectRow, value, CanSelectRowChanged);
            if (!value)
            {
                SelectedRow = default(T);
            }
        }
    }

    public T? SelectedRow
    {
        get => _selectedRow;
        internal set
        {
            if (_selectedRow.Equals(value))
            {
                _selectedRow = value;
                OnRowSelectedAction?.Invoke(_selectedRow);
                SelectedRowChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged();
            }
        }
    }

    public bool IsExtendableToFullScreen
    {
        get => _isExtendableToFullScreen;
        internal set => SetPropertyValue(ref _isExtendableToFullScreen, value, IsExtendableToFullScreenChanged);
    }

    public IUIElement? CommandBarExtraContent
    {
        get => _commandBarExtraContent;
        internal set => SetPropertyValue(ref _commandBarExtraContent, value, CommandBarExtraContentChanged);
    }

    public Func<T?, ValueTask>? OnRowSelectedAction { get; internal set; }

    public event EventHandler? CanSelectRowChanged;
    public event EventHandler? SelectedRowChanged;
    public event EventHandler? ColumnsChanged;
    public event EventHandler? IsExtendableToFullScreenChanged;
    public event EventHandler? CommandBarExtraContentChanged;

    public void Dispose()
    {
        _rows.CollectionChanged -= Rows_CollectionChanged;
    }

    protected override IEnumerable<IUIElement> GetChildren()
    {
        //IFluentDataGridRow[] rows = _rows.ToArray();
        //for (int i = 0; i < rows.Length; i++)
        //{
        //    IFluentDataGridRow row = rows[i];
        //    IFluentDataGridCell[] cells = row.ToArray();
        //    for (int j = 0; j < cells.Length; j++)
        //    {
        //        IFluentDataGridCell cell = cells[j];
        //        if (cell.UIElement is not null)
        //        {
        //            yield return cell.UIElement;
        //        }
        //    }

        //    if (row.Details is not null)
        //    {
        //        yield return row.Details;
        //    }
        //}
        return Array.Empty<IUIElement>();
    }

    private void Rows_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        // Try to preserve the selection.
        //this.Select(SelectedRow);
    }
}

public static partial class GUI
{
    /// <summary>
    /// A component that represents a grid that can display data in with rows and columns.
    /// </summary>
    public static IFluentDataGrid<T> FluentDataGrid<T>()
    {
        return FluentDataGrid<T>(null);
    }

    /// <summary>
    /// A component that represents a grid that can display data in with rows and columns.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    public static IFluentDataGrid<T> FluentDataGrid<T>(string? id)
    {
        return new FluentDataGrid<T>(id);
    }


    /// <summary>
    /// Sets the <see cref="IFluentDataGrid.Rows"/> of the data grid.
    /// </summary>
    public static IFluentDataGrid<T> WithRows<T>(this IFluentDataGrid<T> element, params T[] rows)
    {
        var dataGrid = (FluentDataGrid<T>)element;
        dataGrid.Rows.Clear();
        dataGrid.Rows.AddRange(rows);

        return element;
    }

    /// <summary>
    /// Sets the action to run when selecting an item in the data grid.
    /// </summary>
    public static IFluentDataGrid<T> OnRowSelected<T>(this IFluentDataGrid<T> element, Func<T?, ValueTask>? onRowSelectedAction)
    {
        ((FluentDataGrid<T>)element).OnRowSelectedAction = onRowSelectedAction;
        return element;
    }

    /// <summary>
    /// Sets the action to run when selecting an item in the data grid.
    /// </summary>
    public static IFluentDataGrid<T> OnRowSelected<T>(this IFluentDataGrid<T> element, Action<T>? onRowSelectedAction)
    {
        ((FluentDataGrid<T>)element).OnRowSelectedAction
            = (value) =>
            {
                onRowSelectedAction?.Invoke(value);
                return ValueTask.CompletedTask;
            };
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IFluentDataGridRow"/> that should be selected in the data grid.
    /// If <paramref name="row"/> is null or does not exist in the data grid, no row will be selected.
    /// </summary>
    public static IFluentDataGrid<T> Select<T>(this IFluentDataGrid<T> element, T? row)
    {
        var dataGrid = (FluentDataGrid<T>)element;
        if ((row is not null
            && dataGrid.Rows is not null
            && !dataGrid.Rows.Contains(row))
            || !dataGrid.CanSelectRow)
        {
            dataGrid.SelectedRow = default(T);
        }
        else
        {
            dataGrid.SelectedRow = row;
        }

        return element;
    }

    /// <summary>
    /// Sets the <see cref="IFluentDataGridRow"/> that should be selected in the data grid, using its index in the data grid.
    /// If <paramref name="index"/> smaller or greater than the amount of rows in the data grid, no row will be selected.
    /// </summary>
    public static IFluentDataGrid<T> Select<T>(this IFluentDataGrid<T> element, int index)
    {
        var dataGrid = (FluentDataGrid<T>)element;

        if (dataGrid.Rows is null
            || index < 0
            || index > dataGrid.Rows.Count - 1
            || !dataGrid.CanSelectRow)
        {
            dataGrid.SelectedRow = default(T);
        }
        else
        {
            dataGrid.SelectedRow = dataGrid.Rows[index];
        }

        return element;
    }

    /// <summary>
    /// Allows the user to select a row in the data grid.
    /// </summary>
    public static IFluentDataGrid<T> AllowSelectItem<T>(this IFluentDataGrid<T> element)
    {
        ((FluentDataGrid<T>)element).CanSelectRow = true;
        return element;
    }

    /// <summary>
    /// Prevents the user from selecting a row in the data grid.
    /// </summary>
    public static IFluentDataGrid<T> ForbidSelectItem<T>(this IFluentDataGrid<T> element)
    {
        ((FluentDataGrid<T>)element).CanSelectRow = false;
        return element;
    }

    /// <summary>
    /// Indicates that the control can be extended to take the size of the whole tool boundaries.
    /// </summary>
    /// <remarks>
    /// When <see cref="IUIElement.IsVisible"/> is false and that the element is in full screen mode, the element goes back to normal mode.
    /// </remarks>
    public static IFluentDataGrid<T> Extendable<T>(this IFluentDataGrid<T> element)
    {
        ((FluentDataGrid<T>)element).IsExtendableToFullScreen = true;
        return element;
    }

    /// <summary>
    /// Indicates that the control can not be extended to take the size of the whole tool boundaries.
    /// </summary>
    public static IFluentDataGrid<T> NotExtendable<T>(this IFluentDataGrid<T> element)
    {
        ((FluentDataGrid<T>)element).IsExtendableToFullScreen = false;
        return element;
    }

    /// <summary>
    /// Defines an additional element to display in the command bar.
    /// </summary>
    public static IFluentDataGrid<T> CommandBarExtraContent<T>(this IFluentDataGrid<T> element, IUIElement? extraElement)
    {
        ((FluentDataGrid<T>)element).CommandBarExtraContent = extraElement;
        return element;
    }
}
