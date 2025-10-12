using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OffRouteMap
{
    internal interface IFolderDialogService
    {
        string ShowSelectFolderDialog (string title, string initialDirectory = null);
    }
}
