using System.Collections.Generic;

using Firefly.Models;

namespace Firefly.Services.Abstractions;

public interface ISupportScrollFindableToCenterBehavior
{
    IScrollFindableToCenter CenterScroller { get; set; }

    IEnumerable<IPageFindable<IPageFindableInfo>>? DataSource { get; set; }

    IEnumerable<IPageFindable<IPageFindableInfo>>? FindableScopes { get; set; }
}
