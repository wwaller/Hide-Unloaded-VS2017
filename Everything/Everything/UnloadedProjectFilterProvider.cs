using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using MrHideyUnloady;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace MrHideyUnloady
{
    [SolutionTreeFilterProvider(UnloadedProjectFilterCommand.CommandSetString, (uint)UnloadedProjectFilterCommand.CommandId)]
    [Export]
    public class UnloadedProjectFilterProvider : HierarchyTreeFilterProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IVsHierarchyItemCollectionProvider _hierarchyCollectionProvider;

        [ImportingConstructor]
        public UnloadedProjectFilterProvider(SVsServiceProvider serviceProvider, IVsHierarchyItemCollectionProvider hierarchyCollectionProvider)
        {
            this._serviceProvider = (IServiceProvider)serviceProvider;
            this._hierarchyCollectionProvider = hierarchyCollectionProvider;
        }

        protected override HierarchyTreeFilter CreateFilter()
        {
            return new UnloadedProjectFilter(this._serviceProvider, this._hierarchyCollectionProvider);
        }

        public override bool IsFilteringSupported(IEnumerable rootItems) => true;

        private sealed class UnloadedProjectFilter : HierarchyTreeFilter
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly IVsHierarchyItemCollectionProvider _hierarchyCollectionProvider;

            public UnloadedProjectFilter(IServiceProvider serviceProvider, IVsHierarchyItemCollectionProvider hierarchyCollectionProvider)
            {
                this._serviceProvider = serviceProvider;
                this._hierarchyCollectionProvider = hierarchyCollectionProvider;
            }

            protected override async Task<IReadOnlyObservableSet> GetIncludedItemsAsync(IEnumerable<IVsHierarchyItem> rootItems)
            {
                IVsHierarchy solutionHierarchy = (IVsHierarchy) _serviceProvider.GetService(typeof(IVsSolution));

                IReadOnlyObservableSet<IVsHierarchyItem> sourceItems = await _hierarchyCollectionProvider.GetDescendantsAsync(solutionHierarchy, CancellationToken);

                return await _hierarchyCollectionProvider.GetFilteredHierarchyItemsAsync(sourceItems, ShouldIncludeInFilter, CancellationToken);
            }

            private bool ShouldIncludeInFilter(IVsHierarchyItem hierarchyItem)
            {
                try
                {
                    if (hierarchyItem == null)
                        return false;
                    IVsHierarchy hierarchy = hierarchyItem.HierarchyIdentity.Hierarchy;
                    Guid pguid;
                    hierarchy.GetGuidProperty(4294967294U, (int)__VSHPROPID.VSHPROPID_TypeGuid, out pguid);
                    if (hierarchyItem.HierarchyIdentity.IsNestedItem && (pguid == VSConstants.GUID_ItemType_VirtualFolder || pguid == Guid.Empty && hierarchyItem.Children.Any()) || !hierarchyItem.HierarchyIdentity.IsRoot && pguid == Guid.Empty)
                        return false;
                    object pvar1;
                    hierarchy.GetProperty(hierarchyItem.HierarchyIdentity.ItemID, (int)__VSHPROPID.VSHPROPID_ProjectType, out pvar1);
                    object pvar2;
                    hierarchy.GetProperty(hierarchyItem.HierarchyIdentity.ItemID, (int)__VSHPROPID.VSHPROPID_ProjectDir, out pvar2);
                    object pvar3;
                    bool didSucceed = !ErrorHandler.Succeeded(hierarchy.GetProperty(
                        hierarchyItem.HierarchyIdentity.ItemID, (int) __VSHPROPID5.VSHPROPID_ProjectUnloadStatus,
                        out pvar3));
                    return didSucceed || pvar3 != null && (uint)pvar3 != 0U;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}
