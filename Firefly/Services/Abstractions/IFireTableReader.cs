using System.Collections.Generic;

using Firefly.Models;

using NPOI.XWPF.UserModel;

namespace Firefly.Services.Abstractions;

public interface IFireTableReader
{
    IEnumerable<FireProduct> Read(XWPFDocument doc, FireTableColumnMapping columnMapping);
}
