using System.Collections.Generic;
using System.Threading.Tasks;

using Firefly.Models;

using NPOI.XWPF.UserModel;

namespace Firefly.Services.Abstractions;

public interface IFireTableWriter
{
    Task WriteAsync(string filePath, XWPFDocument document, IEnumerable<FireCheckContext> fireCheckContexts);
}
