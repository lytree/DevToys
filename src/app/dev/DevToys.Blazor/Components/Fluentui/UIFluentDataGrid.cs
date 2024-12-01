using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevToys.Localization.Strings.UIDataGrid;

namespace DevToys.Blazor.Components.UIElements;
public partial class UIFluentDataGrid : StyledComponentBase, IDisposable
{
    public void Dispose()
    {
        //UIDataGrid.SelectedRowChanged -= UIDataGrid_SelectedRowChanged;
        //UIDataGrid.IsVisibleChanged -= UIDataGrid_IsVisibleChanged;
        GC.SuppressFinalize(this);
    }
}
