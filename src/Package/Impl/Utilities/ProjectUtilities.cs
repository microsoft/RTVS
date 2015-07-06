using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Languages.Editor.EditorHelpers;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Workspace;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.VisualStudio.R.Package.Utilities
{
    public static class ProjectUtilities
    {
        public static bool IsSolutionLoading(ServiceProvider serviceProvider)
        {
            var solution = serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            if (solution != null)
            {
                object variant;
                solution.GetProperty((int)VsSolutionPropID.SolutionFileNameBeingLoaded, out variant);

                return ((variant is String) && ((String)variant).Length > 0);
            }

            return false;
        }

        public static string GetProjectDir(this IVsHierarchy hierarchy)
        {
            string projectDir = null;
            object projectDirObj = null;

            if (ErrorHandler.Succeeded(
                hierarchy.GetProperty(
                    (uint)VSConstants.VSITEMID.Root,
                    (int)__VSHPROPID.VSHPROPID_ProjectDir,
                    out projectDirObj)))
            {
                projectDir = projectDirObj as string;
            }

            return projectDir;
        }

        public static string GetProjectFilePath(this IVsHierarchy hierarchy)
        {
            string projectDir = String.Empty;

            IVsProject project = hierarchy as IVsProject;
            if (project != null)
            {
                int hr = project.GetMkDocument(VSConstants.VSITEMID_ROOT, out projectDir);
                Debug.Assert(ErrorHandler.Succeeded(hr), "Unable to get project file path");
            }

            return projectDir;
        }

        public static bool IsProjectMemberItem(this IVsHierarchy hierarchy, string filename, out uint itemId)
        {
            itemId = (uint)VSConstants.VSITEMID.Nil;
            bool result = false;

            if (ErrorHandler.Succeeded(hierarchy.ParseCanonicalName(filename, out itemId)))
            {
                result = IsProjectMemberItem(hierarchy, itemId);
            }

            return result;
        }

        public static bool IsProjectMemberItem(this IVsHierarchy hierarchy, uint itemId)
        {
            bool result = false;

            object isNonMemberItemValue;
            if (ErrorHandler.Succeeded(hierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_IsNonMemberItem, out isNonMemberItemValue)))
            {
                result = !(bool)isNonMemberItemValue;
            }

            return result;
        }

        public static bool IsFile(this IVsHierarchy hierarchy, uint itemId)
        {
            bool result = false;

            Guid typeGuid;
            try
            {
                if (!ErrorHandler.Succeeded(hierarchy.GetGuidProperty(itemId, (int)__VSHPROPID.VSHPROPID_TypeGuid, out typeGuid)))
                {
                    typeGuid = Guid.Empty;
                }
            }
            catch (Exception)
            {
                typeGuid = Guid.Empty;
            }

            result = typeGuid.CompareTo(VSConstants.GUID_ItemType_PhysicalFile) == 0;

            return result;
        }

        public static bool IsFolder(this IVsHierarchy hierarchy, uint itemId)
        {
            bool result = false;

            Guid typeGuid;
            try
            {
                if (!ErrorHandler.Succeeded(hierarchy.GetGuidProperty(itemId, (int)__VSHPROPID.VSHPROPID_TypeGuid, out typeGuid)))
                {
                    typeGuid = Guid.Empty;
                }
            }
            catch (Exception)
            {
                typeGuid = Guid.Empty;
            }

            result = typeGuid.CompareTo(VSConstants.GUID_ItemType_PhysicalFolder) == 0;

            return result;
        }

        public static bool IsExpandableItem(this IVsHierarchy hierarchy, uint itemId)
        {
            bool result = false;
            object isExpandableItemValue;
            if (ErrorHandler.Succeeded(hierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_Expandable, out isExpandableItemValue)))
            {
                if (isExpandableItemValue is bool)
                {
                    result = (bool)isExpandableItemValue;
                }
                else
                {
                    result = (isExpandableItemValue is int) ? (int)isExpandableItemValue != 0 : false;
                }
            }

            return result;
        }

        public static bool IsDescendantOf(this IVsHierarchy hierarchy, uint childItemId, uint parentItemId)
        {
            bool result = false;
            object currentParentItemIdValue;
            uint currentParentItemId = (uint)Microsoft.VisualStudio.VSConstants.VSITEMID_NIL;
            while (ErrorHandler.Succeeded(hierarchy.GetProperty(childItemId, (int)__VSHPROPID.VSHPROPID_Parent, out currentParentItemIdValue)))
            {
                if (currentParentItemIdValue is int)
                {
                    currentParentItemId = unchecked((uint)((int)currentParentItemIdValue));
                }
                else if (currentParentItemIdValue is uint)
                {
                    currentParentItemId = (uint)currentParentItemIdValue;
                }
                else
                {
                    Debug.Fail("Unexpected parent item ID type: " + currentParentItemIdValue.GetType());
                    break;
                }

                result = currentParentItemId == parentItemId;
                if (result || currentParentItemId == (uint)VSConstants.VSITEMID.Nil)
                {
                    break;
                }

                childItemId = currentParentItemId;
            }

            return result;
        }

        public static IVsHierarchy GetHierarchy(this ITextBuffer textBuffer)
        {
            string filePath = textBuffer.GetFileName();
            IVsHierarchy vsHierarchy = null;
            uint vsItemID = (uint)VSConstants.VSITEMID.Nil;

            return TryGetHierarchy(filePath, out vsHierarchy, out vsItemID) ? vsHierarchy : null;
        }

        public static bool TryGetHierarchy(string filePath, out IVsHierarchy vsHierarchy, out uint vsItemId)
        {
            bool result = true;

            vsHierarchy = null;
            vsItemId = (uint)VSConstants.VSITEMID.Nil;

            IVsUIShellOpenDocument vsUIShellOpenDocument = ServiceProvider.GlobalProvider.GetService(typeof(SVsUIShellOpenDocument)) as IVsUIShellOpenDocument;

            IOleServiceProvider serviceProviderUnused = null;
            int docInProject = 0;
            IVsUIHierarchy uiHier = null;


            int hr = vsUIShellOpenDocument.IsDocumentInAProject(filePath, out uiHier, out vsItemId, out serviceProviderUnused, out docInProject);
            if (ErrorHandler.Succeeded(hr) && uiHier != null)
            {
                vsHierarchy = uiHier as IVsHierarchy;
            }
            else
            {
                vsHierarchy = null;
                vsItemId = (uint)VSConstants.VSITEMID.Nil;
                result = false;
            }

            return result;
        }

        public static bool SaveFile(this ITextBuffer textBuffer)
        {
            IVsHierarchy vsHierarchy = textBuffer.GetHierarchy();
            if (vsHierarchy != null)
            {
                VsFileInfo fileInfo = VsFileInfo.FromTextBuffer(textBuffer);

                if (fileInfo != null)
                {
                    IVsSolution solution = AppShell.Current.GetGlobalService<IVsSolution>(typeof(SVsSolution));
                    int hr = solution.SaveSolutionElement((uint)__VSSLNSAVEOPTIONS.SLNSAVEOPT_SaveIfDirty, vsHierarchy, fileInfo.RunningDocumentItemCookie);
                    return ErrorHandler.Succeeded(hr);
                }
            }

            return false;
        }

        public static IEnumerable<uint> GetChildren(this IVsHierarchy hierarchy)
        {
            object pvarCurChild;

            int hr = hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_FirstChild, out pvarCurChild);
            while (ErrorHandler.Succeeded(hr) && (pvarCurChild != null))
            {
                uint curChildItemID = (uint)(int)pvarCurChild;
                yield return curChildItemID;

                hr = hierarchy.GetProperty(curChildItemID, (int)__VSHPROPID.VSHPROPID_NextSibling, out pvarCurChild);
            }
        }

        public static string GetCurrentProjectTypeGuids(string filePath)
        {
            int hr = VSConstants.S_OK;

            string projectTypeGuids = null;
            IVsHierarchy hierarchy = null;
            uint vsItemID = (uint)VSConstants.VSITEMID.Nil;
            uint docCookieUnused = 0;
            IVsProject project = null;

            RunningDocumentTable rdt = new RunningDocumentTable(ServiceProvider.GlobalProvider);
            rdt.FindDocument(filePath, out hierarchy, out vsItemID, out docCookieUnused);

            project = (IVsProject)hierarchy;
            if (project != null)
            {
                IVsAggregatableProject aggProject = project as IVsAggregatableProject;
                if (aggProject != null)
                {
                    hr = aggProject.GetAggregateProjectTypeGuids(out projectTypeGuids);
                    ErrorHandler.ThrowOnFailure(hr);
                }
            }

            return projectTypeGuids;
        }
    }
}
