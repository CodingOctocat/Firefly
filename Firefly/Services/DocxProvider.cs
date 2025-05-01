using System.IO;
using System.Threading.Tasks;

using Firefly.Helpers;
using Firefly.Services.Abstractions;

using NPOI.XWPF.UserModel;

namespace Firefly.Services;

public class DocxProvider : IDocxProvider
{
    public Task<XWPFDocument> LoadAsync(string path, FileMode mode, FileAccess access, FileShare share)
    {
        return DocxHelper.LoadDocumentAsync(path, mode, access, share);
    }
}
