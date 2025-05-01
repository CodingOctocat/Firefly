using System.Collections.Generic;

namespace Firefly.Models;

public interface IPageFindable<out TInfo> where TInfo : IPageFindableInfo
{
    IEnumerable<TInfo> GetFindableInfos();
}
