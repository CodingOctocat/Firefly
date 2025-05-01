using System.IO;
using System.Threading.Tasks;

using NPOI.XWPF.UserModel;

namespace Firefly.Services.Abstractions;

public interface IDocxProvider
{
    Task<XWPFDocument> LoadAsync(string path, FileMode mode, FileAccess access, FileShare share);
}
