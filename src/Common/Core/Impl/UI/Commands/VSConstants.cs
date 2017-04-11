// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Common.Core.UI.Commands {
    public sealed class VSConstants {
        private VSConstants() { }

        // VS Command ID's
        public const int cmdidToolsOptions = 264;

        /// <include file='doc\VSConstants.uex' path='docs/doc[@for="Constants.GUID_VSStandardCommandSet97"]/*' />
        /// <summary>This GUID identifies the standard set of commands known by VisualStudio 97 (version 6).</summary>
        public static readonly Guid GUID_VSStandardCommandSet97 = new Guid("{5EFC7975-14BC-11CF-9B2B-00AA00573819}");
        public static readonly Guid VsDebugCommandGroup = new Guid("C9DD4A59-47FB-11D2-83E7-00C04F9902C1");

        [Guid("5EFC7975-14BC-11CF-9B2B-00AA00573819")]
        public enum VSStd97CmdID {
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AlignBottom"]/*' />
            AlignBottom = 1,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AlignHorizontalCenters"]/*' />
            AlignHorizontalCenters = 2,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AlignLeft"]/*' />
            AlignLeft = 3,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AlignRight"]/*' />
            AlignRight = 4,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AlignToGrid"]/*' />
            AlignToGrid = 5,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AlignTop"]/*' />
            AlignTop = 6,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AlignVerticalCenters"]/*' />
            AlignVerticalCenters = 7,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ArrangeBottom"]/*' />
            ArrangeBottom = 8,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ArrangeRight"]/*' />
            ArrangeRight = 9,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BringForward"]/*' />
            BringForward = 10,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BringToFront"]/*' />
            BringToFront = 11,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CenterHorizontally"]/*' />
            CenterHorizontally = 12,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CenterVertically"]/*' />
            CenterVertically = 13,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Code"]/*' />
            Code = 14,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Copy"]/*' />
            Copy = 15,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Cut"]/*' />
            Cut = 16,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Delete"]/*' />
            Delete = 17,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.FontName"]/*' />
            FontName = 18,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.FontNameGetList"]/*' />
            FontNameGetList = 500,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.FontSize"]/*' />
            FontSize = 19,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.FontSizeGetList"]/*' />
            FontSizeGetList = 501,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Group"]/*' />
            Group = 20,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.HorizSpaceConcatenate"]/*' />
            HorizSpaceConcatenate = 21,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.HorizSpaceDecrease"]/*' />
            HorizSpaceDecrease = 22,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.HorizSpaceIncrease"]/*' />
            HorizSpaceIncrease = 23,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.HorizSpaceMakeEqual"]/*' />
            HorizSpaceMakeEqual = 24,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.LockControls"]/*' />
            LockControls = 369,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.InsertObject"]/*' />
            InsertObject = 25,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Paste"]/*' />
            Paste = 26,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Print"]/*' />
            Print = 27,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Properties"]/*' />
            Properties = 28,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Redo"]/*' />
            Redo = 29,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MultiLevelRedo"]/*' />
            MultiLevelRedo = 30,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SelectAll"]/*' />
            SelectAll = 31,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SendBackward"]/*' />
            SendBackward = 32,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SendToBack"]/*' />
            SendToBack = 33,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShowTable"]/*' />
            ShowTable = 34,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SizeToControl"]/*' />
            SizeToControl = 35,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SizeToControlHeight"]/*' />
            SizeToControlHeight = 36,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SizeToControlWidth"]/*' />
            SizeToControlWidth = 37,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SizeToFit"]/*' />
            SizeToFit = 38,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SizeToGrid"]/*' />
            SizeToGrid = 39,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SnapToGrid"]/*' />
            SnapToGrid = 40,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TabOrder"]/*' />
            TabOrder = 41,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Toolbox"]/*' />
            Toolbox = 42,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Undo"]/*' />
            Undo = 43,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MultiLevelUndo"]/*' />
            MultiLevelUndo = 44,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Ungroup"]/*' />
            Ungroup = 45,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.VertSpaceConcatenate"]/*' />
            VertSpaceConcatenate = 46,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.VertSpaceDecrease"]/*' />
            VertSpaceDecrease = 47,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.VertSpaceIncrease"]/*' />
            VertSpaceIncrease = 48,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.VertSpaceMakeEqual"]/*' />
            VertSpaceMakeEqual = 49,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ZoomPercent"]/*' />
            ZoomPercent = 50,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BackColor"]/*' />
            BackColor = 51,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Bold"]/*' />
            Bold = 52,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BorderColor"]/*' />
            BorderColor = 53,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BorderDashDot"]/*' />
            BorderDashDot = 54,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BorderDashDotDot"]/*' />
            BorderDashDotDot = 55,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BorderDashes"]/*' />
            BorderDashes = 56,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BorderDots"]/*' />
            BorderDots = 57,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BorderShortDashes"]/*' />
            BorderShortDashes = 58,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BorderSolid"]/*' />
            BorderSolid = 59,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BorderSparseDots"]/*' />
            BorderSparseDots = 60,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BorderWidth1"]/*' />
            BorderWidth1 = 61,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BorderWidth2"]/*' />
            BorderWidth2 = 62,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BorderWidth3"]/*' />
            BorderWidth3 = 63,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BorderWidth4"]/*' />
            BorderWidth4 = 64,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BorderWidth5"]/*' />
            BorderWidth5 = 65,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BorderWidth6"]/*' />
            BorderWidth6 = 66,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BorderWidthHairline"]/*' />
            BorderWidthHairline = 67,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Flat"]/*' />
            Flat = 68,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ForeColor"]/*' />
            ForeColor = 69,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Italic"]/*' />
            Italic = 70,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.JustifyCenter"]/*' />
            JustifyCenter = 71,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.JustifyGeneral"]/*' />
            JustifyGeneral = 72,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.JustifyLeft"]/*' />
            JustifyLeft = 73,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.JustifyRight"]/*' />
            JustifyRight = 74,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Raised"]/*' />
            Raised = 75,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Sunken"]/*' />
            Sunken = 76,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Underline"]/*' />
            Underline = 77,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Chiseled"]/*' />
            Chiseled = 78,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Etched"]/*' />
            Etched = 79,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Shadowed"]/*' />
            Shadowed = 80,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CompDebug1"]/*' />
            CompDebug1 = 81,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CompDebug2"]/*' />
            CompDebug2 = 82,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CompDebug3"]/*' />
            CompDebug3 = 83,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CompDebug4"]/*' />
            CompDebug4 = 84,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CompDebug5"]/*' />
            CompDebug5 = 85,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CompDebug6"]/*' />
            CompDebug6 = 86,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CompDebug7"]/*' />
            CompDebug7 = 87,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CompDebug8"]/*' />
            CompDebug8 = 88,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CompDebug9"]/*' />
            CompDebug9 = 89,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CompDebug10"]/*' />
            CompDebug10 = 90,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CompDebug11"]/*' />
            CompDebug11 = 91,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CompDebug12"]/*' />
            CompDebug12 = 92,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CompDebug13"]/*' />
            CompDebug13 = 93,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CompDebug14"]/*' />
            CompDebug14 = 94,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CompDebug15"]/*' />
            CompDebug15 = 95,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ExistingSchemaEdit"]/*' />
            ExistingSchemaEdit = 96,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Find"]/*' />
            Find = 97,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.GetZoom"]/*' />
            GetZoom = 98,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.QueryOpenDesign"]/*' />
            QueryOpenDesign = 99,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.QueryOpenNew"]/*' />
            QueryOpenNew = 100,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SingleTableDesign"]/*' />
            SingleTableDesign = 101,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SingleTableNew"]/*' />
            SingleTableNew = 102,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShowGrid"]/*' />
            ShowGrid = 103,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.NewTable"]/*' />
            NewTable = 104,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CollapsedView"]/*' />
            CollapsedView = 105,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.FieldView"]/*' />
            FieldView = 106,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.VerifySQL"]/*' />
            VerifySQL = 107,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.HideTable"]/*' />
            HideTable = 108,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.PrimaryKey"]/*' />
            PrimaryKey = 109,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Save"]/*' />
            Save = 110,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SaveAs"]/*' />
            SaveAs = 111,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SortAscending"]/*' />
            SortAscending = 112,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SortDescending"]/*' />
            SortDescending = 113,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AppendQuery"]/*' />
            AppendQuery = 114,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CrosstabQuery"]/*' />
            CrosstabQuery = 115,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.DeleteQuery"]/*' />
            DeleteQuery = 116,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MakeTableQuery"]/*' />
            MakeTableQuery = 117,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SelectQuery"]/*' />
            SelectQuery = 118,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.UpdateQuery"]/*' />
            UpdateQuery = 119,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Parameters"]/*' />
            Parameters = 120,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Totals"]/*' />
            Totals = 121,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewCollapsed"]/*' />
            ViewCollapsed = 122,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewFieldList"]/*' />
            ViewFieldList = 123,


            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewKeys"]/*' />
            ViewKeys = 124,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewGrid"]/*' />
            ViewGrid = 125,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.InnerJoin"]/*' />
            InnerJoin = 126,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.RightOuterJoin"]/*' />
            RightOuterJoin = 127,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.LeftOuterJoin"]/*' />
            LeftOuterJoin = 128,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.FullOuterJoin"]/*' />
            FullOuterJoin = 129,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.UnionJoin"]/*' />
            UnionJoin = 130,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShowSQLPane"]/*' />
            ShowSQLPane = 131,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShowGraphicalPane"]/*' />
            ShowGraphicalPane = 132,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShowDataPane"]/*' />
            ShowDataPane = 133,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShowQBEPane"]/*' />
            ShowQBEPane = 134,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SelectAllFields"]/*' />
            SelectAllFields = 135,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OLEObjectMenuButton"]/*' />
            OLEObjectMenuButton = 136,

            // ids on the ole verbs menu - these must be sequential ie verblist0-verblist9
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ObjectVerbList0"]/*' />
            ObjectVerbList0 = 137,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ObjectVerbList1"]/*' />
            ObjectVerbList1 = 138,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ObjectVerbList2"]/*' />
            ObjectVerbList2 = 139,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ObjectVerbList3"]/*' />
            ObjectVerbList3 = 140,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ObjectVerbList4"]/*' />
            ObjectVerbList4 = 141,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ObjectVerbList5"]/*' />
            ObjectVerbList5 = 142,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ObjectVerbList6"]/*' />
            ObjectVerbList6 = 143,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ObjectVerbList7"]/*' />
            ObjectVerbList7 = 144,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ObjectVerbList8"]/*' />
            ObjectVerbList8 = 145,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ObjectVerbList9"]/*' />
            ObjectVerbList9 = 146,  // Unused on purpose!

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ConvertObject"]/*' />
            ConvertObject = 147,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CustomControl"]/*' />
            CustomControl = 148,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CustomizeItem"]/*' />
            CustomizeItem = 149,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Rename"]/*' />
            Rename = 150,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Import"]/*' />
            Import = 151,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.NewPage"]/*' />
            NewPage = 152,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Move"]/*' />
            Move = 153,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Cancel"]/*' />
            Cancel = 154,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Font"]/*' />
            Font = 155,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ExpandLinks"]/*' />
            ExpandLinks = 156,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ExpandImages"]/*' />
            ExpandImages = 157,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ExpandPages"]/*' />
            ExpandPages = 158,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.RefocusDiagram"]/*' />
            RefocusDiagram = 159,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TransitiveClosure"]/*' />
            TransitiveClosure = 160,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CenterDiagram"]/*' />
            CenterDiagram = 161,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ZoomIn"]/*' />
            ZoomIn = 162,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ZoomOut"]/*' />
            ZoomOut = 163,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.RemoveFilter"]/*' />
            RemoveFilter = 164,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.HidePane"]/*' />
            HidePane = 165,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.DeleteTable"]/*' />
            DeleteTable = 166,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.DeleteRelationship"]/*' />
            DeleteRelationship = 167,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Remove"]/*' />
            Remove = 168,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.JoinLeftAll"]/*' />
            JoinLeftAll = 169,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.JoinRightAll"]/*' />
            JoinRightAll = 170,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AddToOutput"]/*' />
            AddToOutput = 171,      // Add selected fields to query output
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OtherQuery"]/*' />
            OtherQuery = 172,      // change query type to 'other'
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.GenerateChangeScript"]/*' />
            GenerateChangeScript = 173,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SaveSelection"]/*' />
            SaveSelection = 174,     // Save current selection
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutojoinCurrent"]/*' />
            AutojoinCurrent = 175,     // Autojoin current tables
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutojoinAlways"]/*' />
            AutojoinAlways = 176,     // Toggle Autojoin state
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.EditPage"]/*' />
            EditPage = 177,     // Launch editor for url
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewLinks"]/*' />
            ViewLinks = 178,     // Launch new webscope for url
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Stop"]/*' />
            Stop = 179,     // Stope webscope rendering
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Pause"]/*' />
            Pause = 180,     // Pause webscope rendering
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Resume"]/*' />
            Resume = 181,     // Resume webscope rendering
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.FilterDiagram"]/*' />
            FilterDiagram = 182,     // Filter webscope diagram
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShowAllObjects"]/*' />
            ShowAllObjects = 183,     // Show All objects in webscope diagram
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShowApplications"]/*' />
            ShowApplications = 184,     // Show Application objects in webscope diagram
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShowOtherObjects"]/*' />
            ShowOtherObjects = 185,     // Show other objects in webscope diagram
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShowPrimRelationships"]/*' />
            ShowPrimRelationships = 186,     // Show primary relationships
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Expand"]/*' />
            Expand = 187,     // Expand links
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Collapse"]/*' />
            Collapse = 188,     // Collapse links
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Refresh"]/*' />
            Refresh = 189,     // Refresh Webscope diagram
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Layout"]/*' />
            Layout = 190,     // Layout websope diagram
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShowResources"]/*' />
            ShowResources = 191,     // Show resouce objects in webscope diagram
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.InsertHTMLWizard"]/*' />
            InsertHTMLWizard = 192,     // Insert HTML using a Wizard
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShowDownloads"]/*' />
            ShowDownloads = 193,     // Show download objects in webscope diagram
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShowExternals"]/*' />
            ShowExternals = 194,     // Show external objects in webscope diagram
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShowInBoundLinks"]/*' />
            ShowInBoundLinks = 195,     // Show inbound links in webscope diagram
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShowOutBoundLinks"]/*' />
            ShowOutBoundLinks = 196,     // Show out bound links in webscope diagram
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShowInAndOutBoundLinks"]/*' />
            ShowInAndOutBoundLinks = 197,     // Show in and out bound links in webscope diagram
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Preview"]/*' />
            Preview = 198,     // Preview page
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Open"]/*' />
            Open = 261,     // Open
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OpenWith"]/*' />
            OpenWith = 199,     // Open with
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShowPages"]/*' />
            ShowPages = 200,     // Show HTML pages
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.RunQuery"]/*' />
            RunQuery = 201,      // Runs a query
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ClearQuery"]/*' />
            ClearQuery = 202,      // Clears the query's associated cursor
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.RecordFirst"]/*' />
            RecordFirst = 203,      // Go to first record in set
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.RecordLast"]/*' />
            RecordLast = 204,      // Go to last record in set
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.RecordNext"]/*' />
            RecordNext = 205,      // Go to next record in set
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.RecordPrevious"]/*' />
            RecordPrevious = 206,      // Go to previous record in set
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.RecordGoto"]/*' />
            RecordGoto = 207,      // Go to record via dialog
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.RecordNew"]/*' />
            RecordNew = 208,      // Add a record to set

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.InsertNewMenu"]/*' />
            InsertNewMenu = 209,     // menu designer
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.InsertSeparator"]/*' />
            InsertSeparator = 210,     // menu designer
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.EditMenuNames"]/*' />
            EditMenuNames = 211,     // menu designer

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.DebugExplorer"]/*' />
            DebugExplorer = 212,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.DebugProcesses"]/*' />
            DebugProcesses = 213,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewThreadsWindow"]/*' />
            ViewThreadsWindow = 214,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.WindowUIList"]/*' />
            WindowUIList = 215,

            // ids on the file menu
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.NewProject"]/*' />
            NewProject = 216,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OpenProject"]/*' />
            OpenProject = 217,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OpenProjectFromWeb"]/*' />
            OpenProjectFromWeb = 450,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OpenSolution"]/*' />
            OpenSolution = 218,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CloseSolution"]/*' />
            CloseSolution = 219,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.FileNew"]/*' />
            FileNew = 221,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.NewProjectFromExisting"]/*' />
            NewProjectFromExisting = 385,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.FileOpen"]/*' />
            FileOpen = 222,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.FileOpenFromWeb"]/*' />
            FileOpenFromWeb = 451,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.FileClose"]/*' />
            FileClose = 223,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SaveSolution"]/*' />
            SaveSolution = 224,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SaveSolutionAs"]/*' />
            SaveSolutionAs = 225,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SaveProjectItemAs"]/*' />
            SaveProjectItemAs = 226,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.PageSetup"]/*' />
            PageSetup = 227,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.PrintPreview"]/*' />
            PrintPreview = 228,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Exit"]/*' />
            Exit = 229,

            // ids on the edit menu
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Replace"]/*' />
            Replace = 230,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Goto"]/*' />
            Goto = 231,

            // ids on the view menu
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.PropertyPages"]/*' />
            PropertyPages = 232,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.FullScreen"]/*' />
            FullScreen = 233,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ProjectExplorer"]/*' />
            ProjectExplorer = 234,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.PropertiesWindow"]/*' />
            PropertiesWindow = 235,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListWindow"]/*' />
            TaskListWindow = 236,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OutputWindow"]/*' />
            OutputWindow = 237,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ObjectBrowser"]/*' />
            ObjectBrowser = 238,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.DocOutlineWindow"]/*' />
            DocOutlineWindow = 239,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ImmediateWindow"]/*' />
            ImmediateWindow = 240,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.WatchWindow"]/*' />
            WatchWindow = 241,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.LocalsWindow"]/*' />
            LocalsWindow = 242,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CallStack"]/*' />
            CallStack = 243,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutosWindow"]/*' />
            AutosWindow = DebugReserved1,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ThisWindow"]/*' />
            ThisWindow = DebugReserved2,

            // ids on project menu
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AddNewItem"]/*' />
            AddNewItem = 220,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AddExistingItem"]/*' />
            AddExistingItem = 244,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.NewFolder"]/*' />
            NewFolder = 245,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SetStartupProject"]/*' />
            SetStartupProject = 246,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ProjectSettings"]/*' />
            ProjectSettings = 247,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ProjectReferences"]/*' />
            ProjectReferences = 367,

            // ids on the debug menu
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.StepInto"]/*' />
            StepInto = 248,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.StepOver"]/*' />
            StepOver = 249,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.StepOut"]/*' />
            StepOut = 250,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.RunToCursor"]/*' />
            RunToCursor = 251,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AddWatch"]/*' />
            AddWatch = 252,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.EditWatch"]/*' />
            EditWatch = 253,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.QuickWatch"]/*' />
            QuickWatch = 254,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ToggleBreakpoint"]/*' />
            ToggleBreakpoint = 255,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ClearBreakpoints"]/*' />
            ClearBreakpoints = 256,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShowBreakpoints"]/*' />
            ShowBreakpoints = 257,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SetNextStatement"]/*' />
            SetNextStatement = 258,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShowNextStatement"]/*' />
            ShowNextStatement = 259,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.EditBreakpoint"]/*' />
            EditBreakpoint = 260,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.DetachDebugger"]/*' />
            DetachDebugger = 262,

            // ids on the tools menu
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CustomizeKeyboard"]/*' />
            CustomizeKeyboard = 263,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ToolsOptions"]/*' />
            ToolsOptions = 264,

            // ids on the windows menu
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.NewWindow"]/*' />
            NewWindow = 265,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Split"]/*' />
            Split = 266,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Cascade"]/*' />
            Cascade = 267,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TileHorz"]/*' />
            TileHorz = 268,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TileVert"]/*' />
            TileVert = 269,

            // ids on the help menu
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TechSupport"]/*' />
            TechSupport = 270,

            // NOTE cmdidAbout and cmdidDebugOptions must be consecutive
            //      cmd after cmdidDebugOptions (ie 273) must not be used
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.About"]/*' />
            About = 271,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.DebugOptions"]/*' />
            DebugOptions = 272,

            // ids on the watch context menu
            // CollapseWatch appears as 'Collapse Parent', on any
            // non-top-level item
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.DeleteWatch"]/*' />
            DeleteWatch = 274,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CollapseWatch"]/*' />
            CollapseWatch = 275,
            // ids 276, 277, 278, 279, 280 are in use
            // below 
            // ids on the property browser context menu
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.PbrsToggleStatus"]/*' />
            PbrsToggleStatus = 282,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.PropbrsHide"]/*' />
            PropbrsHide = 283,

            // ids on the docking context menu
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.DockingView"]/*' />
            DockingView = 284,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.HideActivePane"]/*' />
            HideActivePane = 285,
            // ids for window selection via keyboard
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.PaneNextPane"]/*' />
            PaneNextPane = 316,  //(listed below in order)
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.PanePrevPane"]/*' />
            PanePrevPane = 317,  //(listed below in order)
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.PaneNextTab"]/*' />
            PaneNextTab = 286,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.PanePrevTab"]/*' />
            PanePrevTab = 287,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.PaneCloseToolWindow"]/*' />
            PaneCloseToolWindow = 288,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.PaneActivateDocWindow"]/*' />
            PaneActivateDocWindow = 289,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.DockingViewMDI"]/*' />
            DockingViewMDI = 290,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.DockingViewFloater"]/*' />
            DockingViewFloater = 291,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideWindow"]/*' />
            AutoHideWindow = 292,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MoveToDropdownBar"]/*' />
            MoveToDropdownBar = 293,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.FindCmd"]/*' />
            FindCmd = 294,  // internal Find commands
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Start"]/*' />
            Start = 295,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Restart"]/*' />
            Restart = 296,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AddinManager"]/*' />
            AddinManager = 297,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MultiLevelUndoList"]/*' />
            MultiLevelUndoList = 298,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MultiLevelRedoList"]/*' />
            MultiLevelRedoList = 299,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ToolboxAddTab"]/*' />
            ToolboxAddTab = 300,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ToolboxDeleteTab"]/*' />
            ToolboxDeleteTab = 301,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ToolboxRenameTab"]/*' />
            ToolboxRenameTab = 302,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ToolboxTabMoveUp"]/*' />
            ToolboxTabMoveUp = 303,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ToolboxTabMoveDown"]/*' />
            ToolboxTabMoveDown = 304,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ToolboxRenameItem"]/*' />
            ToolboxRenameItem = 305,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ToolboxListView"]/*' />
            ToolboxListView = 306,
            //(below) cmdidSearchSetCombo        307

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.WindowUIGetList"]/*' />
            WindowUIGetList = 308,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.InsertValuesQuery"]/*' />
            InsertValuesQuery = 309,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShowProperties"]/*' />
            ShowProperties = 310,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ThreadSuspend"]/*' />
            ThreadSuspend = 311,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ThreadResume"]/*' />
            ThreadResume = 312,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ThreadSetFocus"]/*' />
            ThreadSetFocus = 313,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.DisplayRadix"]/*' />
            DisplayRadix = 314,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OpenProjectItem"]/*' />
            OpenProjectItem = 315,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ClearPane"]/*' />
            ClearPane = 318,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.GotoErrorTag"]/*' />
            GotoErrorTag = 319,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListSortByCategory"]/*' />
            TaskListSortByCategory = 320,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListSortByFileLine"]/*' />
            TaskListSortByFileLine = 321,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListSortByPriority"]/*' />
            TaskListSortByPriority = 322,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListSortByDefaultSort"]/*' />
            TaskListSortByDefaultSort = 323,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListShowTooltip"]/*' />
            TaskListShowTooltip = 324,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListFilterByNothing"]/*' />
            TaskListFilterByNothing = 325,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CancelEZDrag"]/*' />
            CancelEZDrag = 326,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListFilterByCategoryCompiler"]/*' />
            TaskListFilterByCategoryCompiler = 327,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListFilterByCategoryComment"]/*' />
            TaskListFilterByCategoryComment = 328,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ToolboxAddItem"]/*' />
            ToolboxAddItem = 329,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ToolboxReset"]/*' />
            ToolboxReset = 330,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SaveProjectItem"]/*' />
            SaveProjectItem = 331,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SaveOptions"]/*' />
            SaveOptions = 959,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewForm"]/*' />
            ViewForm = 332,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewCode"]/*' />
            ViewCode = 333,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.PreviewInBrowser"]/*' />
            PreviewInBrowser = 334,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BrowseWith"]/*' />
            BrowseWith = 336,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SearchSetCombo"]/*' />
            SearchSetCombo = 307,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SearchCombo"]/*' />
            SearchCombo = 337,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.EditLabel"]/*' />
            EditLabel = 338,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Exceptions"]/*' />
            Exceptions = 339,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.DefineViews"]/*' />
            DefineViews = 340,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ToggleSelMode"]/*' />
            ToggleSelMode = 341,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ToggleInsMode"]/*' />
            ToggleInsMode = 342,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.LoadUnloadedProject"]/*' />
            LoadUnloadedProject = 343,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.UnloadLoadedProject"]/*' />
            UnloadLoadedProject = 344,

            // ids on the treegrids (watch/local/threads/stack)
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ElasticColumn"]/*' />
            ElasticColumn = 345,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.HideColumn"]/*' />
            HideColumn = 346,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListPreviousView"]/*' />
            TaskListPreviousView = 347,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ZoomDialog"]/*' />
            ZoomDialog = 348,

            // find/replace options
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.FindHiddenText"]/*' />
            FindHiddenText = 349,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.FindMatchCase"]/*' />
            FindMatchCase = 350,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.FindWholeWord"]/*' />
            FindWholeWord = 351,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.FindSimplePattern"]/*' />
            FindSimplePattern = 276,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.FindRegularExpression"]/*' />
            FindRegularExpression = 352,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.FindBackwards"]/*' />
            FindBackwards = 353,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.FindInSelection"]/*' />
            FindInSelection = 354,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.FindStop"]/*' />
            FindStop = 355,
            // UNUSED                               356
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.FindInFiles"]/*' />
            FindInFiles = 277,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ReplaceInFiles"]/*' />
            ReplaceInFiles = 278,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.NextLocation"]/*' />
            NextLocation = 279,  // next item in task list, find in files results, etc.
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.PreviousLocation"]/*' />
            PreviousLocation = 280,  // prev item "
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.GotoQuick"]/*' />
            GotoQuick = 281,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListNextError"]/*' />
            TaskListNextError = 357,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListPrevError"]/*' />
            TaskListPrevError = 358,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListFilterByCategoryUser"]/*' />
            TaskListFilterByCategoryUser = 359,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListFilterByCategoryShortcut"]/*' />
            TaskListFilterByCategoryShortcut = 360,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListFilterByCategoryHTML"]/*' />
            TaskListFilterByCategoryHTML = 361,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListFilterByCurrentFile"]/*' />
            TaskListFilterByCurrentFile = 362,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListFilterByChecked"]/*' />
            TaskListFilterByChecked = 363,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListFilterByUnchecked"]/*' />
            TaskListFilterByUnchecked = 364,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListSortByDescription"]/*' />
            TaskListSortByDescription = 365,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListSortByChecked"]/*' />
            TaskListSortByChecked = 366,

            // 367 is used above in cmdidProjectReferences
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.StartNoDebug"]/*' />
            StartNoDebug = 368,
            // 369 is used above in cmdidLockControls

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.FindNext"]/*' />
            FindNext = 370,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.FindPrev"]/*' />
            FindPrev = 371,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.FindSelectedNext"]/*' />
            FindSelectedNext = 372,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.FindSelectedPrev"]/*' />
            FindSelectedPrev = 373,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SearchGetList"]/*' />
            SearchGetList = 374,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.InsertBreakpoint"]/*' />
            InsertBreakpoint = 375,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.EnableBreakpoint"]/*' />
            EnableBreakpoint = 376,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.F1Help"]/*' />
            F1Help = 377,

            //UNUSED 378-396

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MoveToNextEZCntr"]/*' />
            MoveToNextEZCntr = 384,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.UpdateMarkerSpans"]/*' />
            UpdateMarkerSpans = 386,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MoveToPreviousEZCntr"]/*' />
            MoveToPreviousEZCntr = 393,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ProjectProperties"]/*' />
            ProjectProperties = 396,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.PropSheetOrProperties"]/*' />
            PropSheetOrProperties = 397,

            // NOTE - the next items are debug only !!
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TshellStep"]/*' />
            TshellStep = 398,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TshellRun"]/*' />
            TshellRun = 399,

            // marker commands on the codewin menu
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MarkerCmd0"]/*' />
            MarkerCmd0 = 400,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MarkerCmd1"]/*' />
            MarkerCmd1 = 401,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MarkerCmd2"]/*' />
            MarkerCmd2 = 402,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MarkerCmd3"]/*' />
            MarkerCmd3 = 403,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MarkerCmd4"]/*' />
            MarkerCmd4 = 404,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MarkerCmd5"]/*' />
            MarkerCmd5 = 405,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MarkerCmd6"]/*' />
            MarkerCmd6 = 406,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MarkerCmd7"]/*' />
            MarkerCmd7 = 407,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MarkerCmd8"]/*' />
            MarkerCmd8 = 408,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MarkerCmd9"]/*' />
            MarkerCmd9 = 409,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MarkerLast"]/*' />
            MarkerLast = 409,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MarkerEnd"]/*' />
            MarkerEnd = 410,  // list terminator reserved

            // user-invoked project reload and unload
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ReloadProject"]/*' />
            ReloadProject = 412,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.UnloadProject"]/*' />
            UnloadProject = 413,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.NewBlankSolution"]/*' />
            NewBlankSolution = 414,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SelectProjectTemplate"]/*' />
            SelectProjectTemplate = 415,

            // document outline commands
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.DetachAttachOutline"]/*' />
            DetachAttachOutline = 420,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShowHideOutline"]/*' />
            ShowHideOutline = 421,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SyncOutline"]/*' />
            SyncOutline = 422,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.RunToCallstCursor"]/*' />
            RunToCallstCursor = 423,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.NoCmdsAvailable"]/*' />
            NoCmdsAvailable = 424,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ContextWindow"]/*' />
            ContextWindow = 427,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Alias"]/*' />
            Alias = 428,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.GotoCommandLine"]/*' />
            GotoCommandLine = 429,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.EvaluateExpression"]/*' />
            EvaluateExpression = 430,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ImmediateMode"]/*' />
            ImmediateMode = 431,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.EvaluateStatement"]/*' />
            EvaluateStatement = 432,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.FindResultWindow1"]/*' />
            FindResultWindow1 = 433,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.FindResultWindow2"]/*' />
            FindResultWindow2 = 434,

            // 500 is used above in cmdidFontNameGetList
            // 501 is used above in cmdidFontSizeGetList

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.RenameBookmark"]/*' />
            RenameBookmark = 559,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ToggleBookmark"]/*' />
            ToggleBookmark = 560,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.DeleteBookmark"]/*' />
            DeleteBookmark = 561,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BookmarkWindowGoToBookmark"]/*' />
            BookmarkWindowGoToBookmark = 562,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.EnableBookmark"]/*' />
            EnableBookmark = 564,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.NewBookmarkFolder"]/*' />
            NewBookmarkFolder = 565,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.NextBookmarkFolder"]/*' />
            NextBookmarkFolder = 568,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.PrevBookmarkFolder"]/*' />
            PrevBookmarkFolder = 569,

            // ids on the window menu - these must be sequential ie window1-morewind
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Window1"]/*' />
            Window1 = 570,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Window2"]/*' />
            Window2 = 571,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Window3"]/*' />
            Window3 = 572,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Window4"]/*' />
            Window4 = 573,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Window5"]/*' />
            Window5 = 574,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Window6"]/*' />
            Window6 = 575,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Window7"]/*' />
            Window7 = 576,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Window8"]/*' />
            Window8 = 577,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Window9"]/*' />
            Window9 = 578,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Window10"]/*' />
            Window10 = 579,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Window11"]/*' />
            Window11 = 580,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Window12"]/*' />
            Window12 = 581,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Window13"]/*' />
            Window13 = 582,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Window14"]/*' />
            Window14 = 583,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Window15"]/*' />
            Window15 = 584,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Window16"]/*' />
            Window16 = 585,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Window17"]/*' />
            Window17 = 586,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Window18"]/*' />
            Window18 = 587,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Window19"]/*' />
            Window19 = 588,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Window20"]/*' />
            Window20 = 589,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Window21"]/*' />
            Window21 = 590,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Window22"]/*' />
            Window22 = 591,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Window23"]/*' />
            Window23 = 592,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Window24"]/*' />
            Window24 = 593,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Window25"]/*' />
            Window25 = 594,    // note cmdidWindow25 is unused on purpose!
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MoreWindows"]/*' />
            MoreWindows = 595,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideAllWindows"]/*' />
            AutoHideAllWindows = 597,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListTaskHelp"]/*' />
            TaskListTaskHelp = 598,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ClassView"]/*' />
            ClassView = 599,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUProj1"]/*' />
            MRUProj1 = 600,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUProj2"]/*' />
            MRUProj2 = 601,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUProj3"]/*' />
            MRUProj3 = 602,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUProj4"]/*' />
            MRUProj4 = 603,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUProj5"]/*' />
            MRUProj5 = 604,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUProj6"]/*' />
            MRUProj6 = 605,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUProj7"]/*' />
            MRUProj7 = 606,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUProj8"]/*' />
            MRUProj8 = 607,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUProj9"]/*' />
            MRUProj9 = 608,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUProj10"]/*' />
            MRUProj10 = 609,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUProj11"]/*' />
            MRUProj11 = 610,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUProj12"]/*' />
            MRUProj12 = 611,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUProj13"]/*' />
            MRUProj13 = 612,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUProj14"]/*' />
            MRUProj14 = 613,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUProj15"]/*' />
            MRUProj15 = 614,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUProj16"]/*' />
            MRUProj16 = 615,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUProj17"]/*' />
            MRUProj17 = 616,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUProj18"]/*' />
            MRUProj18 = 617,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUProj19"]/*' />
            MRUProj19 = 618,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUProj20"]/*' />
            MRUProj20 = 619,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUProj21"]/*' />
            MRUProj21 = 620,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUProj22"]/*' />
            MRUProj22 = 621,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUProj23"]/*' />
            MRUProj23 = 622,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUProj24"]/*' />
            MRUProj24 = 623,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUProj25"]/*' />
            MRUProj25 = 624,   // note cmdidMRUProj25 is unused on purpose!

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SplitNext"]/*' />
            SplitNext = 625,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SplitPrev"]/*' />
            SplitPrev = 626,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CloseAllDocuments"]/*' />
            CloseAllDocuments = 627,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.NextDocument"]/*' />
            NextDocument = 628,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.PrevDocument"]/*' />
            PrevDocument = 629,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Tool1"]/*' />
            Tool1 = 630,   // note cmdidTool1 - cmdidTool24 must be
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Tool2"]/*' />
            Tool2 = 631,   // consecutive
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Tool3"]/*' />
            Tool3 = 632,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Tool4"]/*' />
            Tool4 = 633,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Tool5"]/*' />
            Tool5 = 634,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Tool6"]/*' />
            Tool6 = 635,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Tool7"]/*' />
            Tool7 = 636,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Tool8"]/*' />
            Tool8 = 637,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Tool9"]/*' />
            Tool9 = 638,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Tool10"]/*' />
            Tool10 = 639,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Tool11"]/*' />
            Tool11 = 640,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Tool12"]/*' />
            Tool12 = 641,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Tool13"]/*' />
            Tool13 = 642,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Tool14"]/*' />
            Tool14 = 643,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Tool15"]/*' />
            Tool15 = 644,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Tool16"]/*' />
            Tool16 = 645,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Tool17"]/*' />
            Tool17 = 646,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Tool18"]/*' />
            Tool18 = 647,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Tool19"]/*' />
            Tool19 = 648,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Tool20"]/*' />
            Tool20 = 649,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Tool21"]/*' />
            Tool21 = 650,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Tool22"]/*' />
            Tool22 = 651,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Tool23"]/*' />
            Tool23 = 652,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Tool24"]/*' />
            Tool24 = 653,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ExternalCommands"]/*' />
            ExternalCommands = 654,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.PasteNextTBXCBItem"]/*' />
            PasteNextTBXCBItem = 655,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ToolboxShowAllTabs"]/*' />
            ToolboxShowAllTabs = 656,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ProjectDependencies"]/*' />
            ProjectDependencies = 657,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CloseDocument"]/*' />
            CloseDocument = 658,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ToolboxSortItems"]/*' />
            ToolboxSortItems = 659,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewBarView1"]/*' />
            ViewBarView1 = 660,    //UNUSED
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewBarView2"]/*' />
            ViewBarView2 = 661,    //UNUSED
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewBarView3"]/*' />
            ViewBarView3 = 662,    //UNUSED
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewBarView4"]/*' />
            ViewBarView4 = 663,    //UNUSED
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewBarView5"]/*' />
            ViewBarView5 = 664,    //UNUSED
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewBarView6"]/*' />
            ViewBarView6 = 665,    //UNUSED
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewBarView7"]/*' />
            ViewBarView7 = 666,    //UNUSED
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewBarView8"]/*' />
            ViewBarView8 = 667,    //UNUSED
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewBarView9"]/*' />
            ViewBarView9 = 668,    //UNUSED
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewBarView10"]/*' />
            ViewBarView10 = 669,    //UNUSED
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewBarView11"]/*' />
            ViewBarView11 = 670,    //UNUSED
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewBarView12"]/*' />
            ViewBarView12 = 671,    //UNUSED
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewBarView13"]/*' />
            ViewBarView13 = 672,    //UNUSED
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewBarView14"]/*' />
            ViewBarView14 = 673,    //UNUSED
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewBarView15"]/*' />
            ViewBarView15 = 674,    //UNUSED
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewBarView16"]/*' />
            ViewBarView16 = 675,    //UNUSED
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewBarView17"]/*' />
            ViewBarView17 = 676,    //UNUSED
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewBarView18"]/*' />
            ViewBarView18 = 677,    //UNUSED
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewBarView19"]/*' />
            ViewBarView19 = 678,    //UNUSED
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewBarView20"]/*' />
            ViewBarView20 = 679,    //UNUSED
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewBarView21"]/*' />
            ViewBarView21 = 680,    //UNUSED
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewBarView22"]/*' />
            ViewBarView22 = 681,    //UNUSED
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewBarView23"]/*' />
            ViewBarView23 = 682,    //UNUSED
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewBarView24"]/*' />
            ViewBarView24 = 683,    //UNUSED

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SolutionCfg"]/*' />
            SolutionCfg = 684,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SolutionCfgGetList"]/*' />
            SolutionCfgGetList = 685,

            //
            // Schema table commands:
            // All invoke table property dialog and select appropriate page.
            //
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ManageIndexes"]/*' />
            ManageIndexes = 675,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ManageRelationships"]/*' />
            ManageRelationships = 676,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ManageConstraints"]/*' />
            ManageConstraints = 677,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView1"]/*' />
            TaskListCustomView1 = 678,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView2"]/*' />
            TaskListCustomView2 = 679,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView3"]/*' />
            TaskListCustomView3 = 680,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView4"]/*' />
            TaskListCustomView4 = 681,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView5"]/*' />
            TaskListCustomView5 = 682,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView6"]/*' />
            TaskListCustomView6 = 683,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView7"]/*' />
            TaskListCustomView7 = 684,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView8"]/*' />
            TaskListCustomView8 = 685,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView9"]/*' />
            TaskListCustomView9 = 686,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView10"]/*' />
            TaskListCustomView10 = 687,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView11"]/*' />
            TaskListCustomView11 = 688,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView12"]/*' />
            TaskListCustomView12 = 689,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView13"]/*' />
            TaskListCustomView13 = 690,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView14"]/*' />
            TaskListCustomView14 = 691,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView15"]/*' />
            TaskListCustomView15 = 692,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView16"]/*' />
            TaskListCustomView16 = 693,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView17"]/*' />
            TaskListCustomView17 = 694,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView18"]/*' />
            TaskListCustomView18 = 695,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView19"]/*' />
            TaskListCustomView19 = 696,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView20"]/*' />
            TaskListCustomView20 = 697,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView21"]/*' />
            TaskListCustomView21 = 698,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView22"]/*' />
            TaskListCustomView22 = 699,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView23"]/*' />
            TaskListCustomView23 = 700,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView24"]/*' />
            TaskListCustomView24 = 701,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView25"]/*' />
            TaskListCustomView25 = 702,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView26"]/*' />
            TaskListCustomView26 = 703,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView27"]/*' />
            TaskListCustomView27 = 704,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView28"]/*' />
            TaskListCustomView28 = 705,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView29"]/*' />
            TaskListCustomView29 = 706,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView30"]/*' />
            TaskListCustomView30 = 707,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView31"]/*' />
            TaskListCustomView31 = 708,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView32"]/*' />
            TaskListCustomView32 = 709,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView33"]/*' />
            TaskListCustomView33 = 710,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView34"]/*' />
            TaskListCustomView34 = 711,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView35"]/*' />
            TaskListCustomView35 = 712,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView36"]/*' />
            TaskListCustomView36 = 713,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView37"]/*' />
            TaskListCustomView37 = 714,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView38"]/*' />
            TaskListCustomView38 = 715,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView39"]/*' />
            TaskListCustomView39 = 716,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView40"]/*' />
            TaskListCustomView40 = 717,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView41"]/*' />
            TaskListCustomView41 = 718,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView42"]/*' />
            TaskListCustomView42 = 719,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView43"]/*' />
            TaskListCustomView43 = 720,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView44"]/*' />
            TaskListCustomView44 = 721,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView45"]/*' />
            TaskListCustomView45 = 722,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView46"]/*' />
            TaskListCustomView46 = 723,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView47"]/*' />
            TaskListCustomView47 = 724,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView48"]/*' />
            TaskListCustomView48 = 725,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView49"]/*' />
            TaskListCustomView49 = 726,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaskListCustomView50"]/*' />
            TaskListCustomView50 = 727,  //not used on purpose, ends the list

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.WhiteSpace"]/*' />
            WhiteSpace = 728,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CommandWindow"]/*' />
            CommandWindow = 729,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CommandWindowMarkMode"]/*' />
            CommandWindowMarkMode = 730,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.LogCommandWindow"]/*' />
            LogCommandWindow = 731,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Shell"]/*' />
            Shell = 732,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SingleChar"]/*' />
            SingleChar = 733,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ZeroOrMore"]/*' />
            ZeroOrMore = 734,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OneOrMore"]/*' />
            OneOrMore = 735,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BeginLine"]/*' />
            BeginLine = 736,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.EndLine"]/*' />
            EndLine = 737,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BeginWord"]/*' />
            BeginWord = 738,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.EndWord"]/*' />
            EndWord = 739,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CharInSet"]/*' />
            CharInSet = 740,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CharNotInSet"]/*' />
            CharNotInSet = 741,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Or"]/*' />
            Or = 742,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Escape"]/*' />
            Escape = 743,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TagExp"]/*' />
            TagExp = 744,

            // Regex builder context help menu commands
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.PatternMatchHelp"]/*' />
            PatternMatchHelp = 745,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.RegExList"]/*' />
            RegExList = 746,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.DebugReserved1"]/*' />
            DebugReserved1 = 747,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.DebugReserved2"]/*' />
            DebugReserved2 = 748,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.DebugReserved3"]/*' />
            DebugReserved3 = 749,
            //USED ABOVE                        750
            //USED ABOVE                        751
            //USED ABOVE                        752
            //USED ABOVE                        753

            //Regex builder wildcard menu commands
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.WildZeroOrMore"]/*' />
            WildZeroOrMore = 754,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.WildSingleChar"]/*' />
            WildSingleChar = 755,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.WildSingleDigit"]/*' />
            WildSingleDigit = 756,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.WildCharInSet"]/*' />
            WildCharInSet = 757,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.WildCharNotInSet"]/*' />
            WildCharNotInSet = 758,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.FindWhatText"]/*' />
            FindWhatText = 759,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaggedExp1"]/*' />
            TaggedExp1 = 760,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaggedExp2"]/*' />
            TaggedExp2 = 761,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaggedExp3"]/*' />
            TaggedExp3 = 762,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaggedExp4"]/*' />
            TaggedExp4 = 763,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaggedExp5"]/*' />
            TaggedExp5 = 764,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaggedExp6"]/*' />
            TaggedExp6 = 765,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaggedExp7"]/*' />
            TaggedExp7 = 766,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaggedExp8"]/*' />
            TaggedExp8 = 767,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.TaggedExp9"]/*' />
            TaggedExp9 = 768,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.EditorWidgetClick"]/*' />
            EditorWidgetClick = 769,  // param 0 is the moniker as VT_BSTR, param 1 is the buffer line as VT_I4, and param 2 is the buffer index as VT_I4
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CmdWinUpdateAC"]/*' />
            CmdWinUpdateAC = 770,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SlnCfgMgr"]/*' />
            SlnCfgMgr = 771,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AddNewProject"]/*' />
            AddNewProject = 772,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AddExistingProject"]/*' />
            AddExistingProject = 773,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AddExistingProjFromWeb"]/*' />
            AddExistingProjFromWeb = 774,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideContext1"]/*' />
            AutoHideContext1 = 776,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideContext2"]/*' />
            AutoHideContext2 = 777,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideContext3"]/*' />
            AutoHideContext3 = 778,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideContext4"]/*' />
            AutoHideContext4 = 779,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideContext5"]/*' />
            AutoHideContext5 = 780,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideContext6"]/*' />
            AutoHideContext6 = 781,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideContext7"]/*' />
            AutoHideContext7 = 782,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideContext8"]/*' />
            AutoHideContext8 = 783,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideContext9"]/*' />
            AutoHideContext9 = 784,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideContext10"]/*' />
            AutoHideContext10 = 785,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideContext11"]/*' />
            AutoHideContext11 = 786,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideContext12"]/*' />
            AutoHideContext12 = 787,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideContext13"]/*' />
            AutoHideContext13 = 788,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideContext14"]/*' />
            AutoHideContext14 = 789,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideContext15"]/*' />
            AutoHideContext15 = 790,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideContext16"]/*' />
            AutoHideContext16 = 791,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideContext17"]/*' />
            AutoHideContext17 = 792,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideContext18"]/*' />
            AutoHideContext18 = 793,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideContext19"]/*' />
            AutoHideContext19 = 794,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideContext20"]/*' />
            AutoHideContext20 = 795,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideContext21"]/*' />
            AutoHideContext21 = 796,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideContext22"]/*' />
            AutoHideContext22 = 797,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideContext23"]/*' />
            AutoHideContext23 = 798,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideContext24"]/*' />
            AutoHideContext24 = 799,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideContext25"]/*' />
            AutoHideContext25 = 800,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideContext26"]/*' />
            AutoHideContext26 = 801,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideContext27"]/*' />
            AutoHideContext27 = 802,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideContext28"]/*' />
            AutoHideContext28 = 803,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideContext29"]/*' />
            AutoHideContext29 = 804,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideContext30"]/*' />
            AutoHideContext30 = 805,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideContext31"]/*' />
            AutoHideContext31 = 806,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideContext32"]/*' />
            AutoHideContext32 = 807,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AutoHideContext33"]/*' />
            AutoHideContext33 = 808,   // must remain unused

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavBackward"]/*' />
            ShellNavBackward = 809,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavForward"]/*' />
            ShellNavForward = 810,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavigate1"]/*' />
            ShellNavigate1 = 811,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavigate2"]/*' />
            ShellNavigate2 = 812,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavigate3"]/*' />
            ShellNavigate3 = 813,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavigate4"]/*' />
            ShellNavigate4 = 814,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavigate5"]/*' />
            ShellNavigate5 = 815,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavigate6"]/*' />
            ShellNavigate6 = 816,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavigate7"]/*' />
            ShellNavigate7 = 817,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavigate8"]/*' />
            ShellNavigate8 = 818,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavigate9"]/*' />
            ShellNavigate9 = 819,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavigate10"]/*' />
            ShellNavigate10 = 820,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavigate11"]/*' />
            ShellNavigate11 = 821,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavigate12"]/*' />
            ShellNavigate12 = 822,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavigate13"]/*' />
            ShellNavigate13 = 823,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavigate14"]/*' />
            ShellNavigate14 = 824,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavigate15"]/*' />
            ShellNavigate15 = 825,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavigate16"]/*' />
            ShellNavigate16 = 826,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavigate17"]/*' />
            ShellNavigate17 = 827,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavigate18"]/*' />
            ShellNavigate18 = 828,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavigate19"]/*' />
            ShellNavigate19 = 829,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavigate20"]/*' />
            ShellNavigate20 = 830,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavigate21"]/*' />
            ShellNavigate21 = 831,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavigate22"]/*' />
            ShellNavigate22 = 832,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavigate23"]/*' />
            ShellNavigate23 = 833,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavigate24"]/*' />
            ShellNavigate24 = 834,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavigate25"]/*' />
            ShellNavigate25 = 835,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavigate26"]/*' />
            ShellNavigate26 = 836,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavigate27"]/*' />
            ShellNavigate27 = 837,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavigate28"]/*' />
            ShellNavigate28 = 838,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavigate29"]/*' />
            ShellNavigate29 = 839,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavigate30"]/*' />
            ShellNavigate30 = 840,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavigate31"]/*' />
            ShellNavigate31 = 841,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavigate32"]/*' />
            ShellNavigate32 = 842,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellNavigate33"]/*' />
            ShellNavigate33 = 843,   // must remain unused

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellWindowNavigate1"]/*' />
            ShellWindowNavigate1 = 844,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellWindowNavigate2"]/*' />
            ShellWindowNavigate2 = 845,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellWindowNavigate3"]/*' />
            ShellWindowNavigate3 = 846,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellWindowNavigate4"]/*' />
            ShellWindowNavigate4 = 847,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellWindowNavigate5"]/*' />
            ShellWindowNavigate5 = 848,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellWindowNavigate6"]/*' />
            ShellWindowNavigate6 = 849,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellWindowNavigate7"]/*' />
            ShellWindowNavigate7 = 850,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellWindowNavigate8"]/*' />
            ShellWindowNavigate8 = 851,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellWindowNavigate9"]/*' />
            ShellWindowNavigate9 = 852,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellWindowNavigate10"]/*' />
            ShellWindowNavigate10 = 853,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellWindowNavigate11"]/*' />
            ShellWindowNavigate11 = 854,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellWindowNavigate12"]/*' />
            ShellWindowNavigate12 = 855,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellWindowNavigate13"]/*' />
            ShellWindowNavigate13 = 856,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellWindowNavigate14"]/*' />
            ShellWindowNavigate14 = 857,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellWindowNavigate15"]/*' />
            ShellWindowNavigate15 = 858,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellWindowNavigate16"]/*' />
            ShellWindowNavigate16 = 859,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellWindowNavigate17"]/*' />
            ShellWindowNavigate17 = 860,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellWindowNavigate18"]/*' />
            ShellWindowNavigate18 = 861,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellWindowNavigate19"]/*' />
            ShellWindowNavigate19 = 862,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellWindowNavigate20"]/*' />
            ShellWindowNavigate20 = 863,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellWindowNavigate21"]/*' />
            ShellWindowNavigate21 = 864,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellWindowNavigate22"]/*' />
            ShellWindowNavigate22 = 865,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellWindowNavigate23"]/*' />
            ShellWindowNavigate23 = 866,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellWindowNavigate24"]/*' />
            ShellWindowNavigate24 = 867,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellWindowNavigate25"]/*' />
            ShellWindowNavigate25 = 868,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellWindowNavigate26"]/*' />
            ShellWindowNavigate26 = 869,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellWindowNavigate27"]/*' />
            ShellWindowNavigate27 = 870,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellWindowNavigate28"]/*' />
            ShellWindowNavigate28 = 871,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellWindowNavigate29"]/*' />
            ShellWindowNavigate29 = 872,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellWindowNavigate30"]/*' />
            ShellWindowNavigate30 = 873,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellWindowNavigate31"]/*' />
            ShellWindowNavigate31 = 874,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellWindowNavigate32"]/*' />
            ShellWindowNavigate32 = 875,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShellWindowNavigate33"]/*' />
            ShellWindowNavigate33 = 876,   // must remain unused

            // ObjectSearch cmds
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OBSDoFind"]/*' />
            OBSDoFind = 877,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OBSMatchCase"]/*' />
            OBSMatchCase = 878,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OBSMatchSubString"]/*' />
            OBSMatchSubString = 879,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OBSMatchWholeWord"]/*' />
            OBSMatchWholeWord = 880,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OBSMatchPrefix"]/*' />
            OBSMatchPrefix = 881,

            // build cmds
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BuildSln"]/*' />
            BuildSln = 882,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.RebuildSln"]/*' />
            RebuildSln = 883,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.DeploySln"]/*' />
            DeploySln = 884,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CleanSln"]/*' />
            CleanSln = 885,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BuildSel"]/*' />
            BuildSel = 886,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.RebuildSel"]/*' />
            RebuildSel = 887,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.DeploySel"]/*' />
            DeploySel = 888,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CleanSel"]/*' />
            CleanSel = 889,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CancelBuild"]/*' />
            CancelBuild = 890,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BatchBuildDlg"]/*' />
            BatchBuildDlg = 891,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BuildCtx"]/*' />
            BuildCtx = 892,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.RebuildCtx"]/*' />
            RebuildCtx = 893,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.DeployCtx"]/*' />
            DeployCtx = 894,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CleanCtx"]/*' />
            CleanCtx = 895,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.QryManageIndexes"]/*' />
            QryManageIndexes = 896,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.PrintDefault"]/*' />
            PrintDefault = 897,         // quick print
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BrowseDoc"]/*' />
            BrowseDoc = 898,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShowStartPage"]/*' />
            ShowStartPage = 899,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUFile1"]/*' />
            MRUFile1 = 900,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUFile2"]/*' />
            MRUFile2 = 901,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUFile3"]/*' />
            MRUFile3 = 902,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUFile4"]/*' />
            MRUFile4 = 903,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUFile5"]/*' />
            MRUFile5 = 904,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUFile6"]/*' />
            MRUFile6 = 905,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUFile7"]/*' />
            MRUFile7 = 906,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUFile8"]/*' />
            MRUFile8 = 907,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUFile9"]/*' />
            MRUFile9 = 908,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUFile10"]/*' />
            MRUFile10 = 909,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUFile11"]/*' />
            MRUFile11 = 910,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUFile12"]/*' />
            MRUFile12 = 911,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUFile13"]/*' />
            MRUFile13 = 912,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUFile14"]/*' />
            MRUFile14 = 913,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUFile15"]/*' />
            MRUFile15 = 914,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUFile16"]/*' />
            MRUFile16 = 915,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUFile17"]/*' />
            MRUFile17 = 916,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUFile18"]/*' />
            MRUFile18 = 917,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUFile19"]/*' />
            MRUFile19 = 918,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUFile20"]/*' />
            MRUFile20 = 919,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUFile21"]/*' />
            MRUFile21 = 920,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUFile22"]/*' />
            MRUFile22 = 921,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUFile23"]/*' />
            MRUFile23 = 922,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUFile24"]/*' />
            MRUFile24 = 923,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MRUFile25"]/*' />
            MRUFile25 = 924,   // note cmdidMRUFile25 is unused on purpose!

            //External Tools Context Menu Commands
            // continued at 1109
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ExtToolsCurPath"]/*' />
            ExtToolsCurPath = 925,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ExtToolsCurDir"]/*' />
            ExtToolsCurDir = 926,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ExtToolsCurFileName"]/*' />
            ExtToolsCurFileName = 927,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ExtToolsCurExtension"]/*' />
            ExtToolsCurExtension = 928,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ExtToolsProjDir"]/*' />
            ExtToolsProjDir = 929,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ExtToolsProjFileName"]/*' />
            ExtToolsProjFileName = 930,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ExtToolsSlnDir"]/*' />
            ExtToolsSlnDir = 931,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ExtToolsSlnFileName"]/*' />
            ExtToolsSlnFileName = 932,


            // Object Browsing & ClassView cmds
            // Shared shell cmds (for accessing Object Browsing functionality)
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.GotoDefn"]/*' />
            GotoDefn = 935,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.GotoDecl"]/*' />
            GotoDecl = 936,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BrowseDefn"]/*' />
            BrowseDefn = 937,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SyncClassView"]/*' />
            SyncClassView = 938,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShowMembers"]/*' />
            ShowMembers = 939,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShowBases"]/*' />
            ShowBases = 940,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShowDerived"]/*' />
            ShowDerived = 941,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShowDefns"]/*' />
            ShowDefns = 942,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShowRefs"]/*' />
            ShowRefs = 943,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShowCallers"]/*' />
            ShowCallers = 944,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShowCallees"]/*' />
            ShowCallees = 945,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AddClass"]/*' />
            AddClass = 946,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AddNestedClass"]/*' />
            AddNestedClass = 947,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AddInterface"]/*' />
            AddInterface = 948,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AddMethod"]/*' />
            AddMethod = 949,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AddProperty"]/*' />
            AddProperty = 950,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AddEvent"]/*' />
            AddEvent = 951,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AddVariable"]/*' />
            AddVariable = 952,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ImplementInterface"]/*' />
            ImplementInterface = 953,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Override"]/*' />
            Override = 954,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AddFunction"]/*' />
            AddFunction = 955,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AddConnectionPoint"]/*' />
            AddConnectionPoint = 956,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.AddIndexer"]/*' />
            AddIndexer = 957,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BuildOrder"]/*' />
            BuildOrder = 958,
            //959 used above for cmdidSaveOptions

            // Object Browser Tool Specific cmds
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OBShowHidden"]/*' />
            OBShowHidden = 960,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OBEnableGrouping"]/*' />
            OBEnableGrouping = 961,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OBSetGroupingCriteria"]/*' />
            OBSetGroupingCriteria = 962,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OBBack"]/*' />
            OBBack = 963,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OBForward"]/*' />
            OBForward = 964,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OBShowPackages"]/*' />
            OBShowPackages = 965,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OBSearchCombo"]/*' />
            OBSearchCombo = 966,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OBSearchOptWholeWord"]/*' />
            OBSearchOptWholeWord = 967,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OBSearchOptSubstring"]/*' />
            OBSearchOptSubstring = 968,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OBSearchOptPrefix"]/*' />
            OBSearchOptPrefix = 969,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OBSearchOptCaseSensitive"]/*' />
            OBSearchOptCaseSensitive = 970,

            // ClassView Tool Specific cmds
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CVGroupingNone"]/*' />
            CVGroupingNone = 971,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CVGroupingSortOnly"]/*' />
            CVGroupingSortOnly = 972,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CVGroupingGrouped"]/*' />
            CVGroupingGrouped = 973,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CVShowPackages"]/*' />
            CVShowPackages = 974,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CVNewFolder"]/*' />
            CVNewFolder = 975,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CVGroupingSortAccess"]/*' />
            CVGroupingSortAccess = 976,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ObjectSearch"]/*' />
            ObjectSearch = 977,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ObjectSearchResults"]/*' />
            ObjectSearchResults = 978,

            // Further Obj Browsing cmds at 1095

            // build cascade menus
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Build1"]/*' />
            Build1 = 979,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Build2"]/*' />
            Build2 = 980,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Build3"]/*' />
            Build3 = 981,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Build4"]/*' />
            Build4 = 982,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Build5"]/*' />
            Build5 = 983,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Build6"]/*' />
            Build6 = 984,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Build7"]/*' />
            Build7 = 985,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Build8"]/*' />
            Build8 = 986,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Build9"]/*' />
            Build9 = 987,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BuildLast"]/*' />
            BuildLast = 988,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Rebuild1"]/*' />
            Rebuild1 = 989,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Rebuild2"]/*' />
            Rebuild2 = 990,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Rebuild3"]/*' />
            Rebuild3 = 991,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Rebuild4"]/*' />
            Rebuild4 = 992,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Rebuild5"]/*' />
            Rebuild5 = 993,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Rebuild6"]/*' />
            Rebuild6 = 994,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Rebuild7"]/*' />
            Rebuild7 = 995,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Rebuild8"]/*' />
            Rebuild8 = 996,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Rebuild9"]/*' />
            Rebuild9 = 997,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.RebuildLast"]/*' />
            RebuildLast = 998,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Clean1"]/*' />
            Clean1 = 999,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Clean2"]/*' />
            Clean2 = 1000,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Clean3"]/*' />
            Clean3 = 1001,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Clean4"]/*' />
            Clean4 = 1002,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Clean5"]/*' />
            Clean5 = 1003,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Clean6"]/*' />
            Clean6 = 1004,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Clean7"]/*' />
            Clean7 = 1005,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Clean8"]/*' />
            Clean8 = 1006,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Clean9"]/*' />
            Clean9 = 1007,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CleanLast"]/*' />
            CleanLast = 1008,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Deploy1"]/*' />
            Deploy1 = 1009,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Deploy2"]/*' />
            Deploy2 = 1010,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Deploy3"]/*' />
            Deploy3 = 1011,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Deploy4"]/*' />
            Deploy4 = 1012,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Deploy5"]/*' />
            Deploy5 = 1013,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Deploy6"]/*' />
            Deploy6 = 1014,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Deploy7"]/*' />
            Deploy7 = 1015,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Deploy8"]/*' />
            Deploy8 = 1016,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Deploy9"]/*' />
            Deploy9 = 1017,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.DeployLast"]/*' />
            DeployLast = 1018,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BuildProjPicker"]/*' />
            BuildProjPicker = 1019,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.RebuildProjPicker"]/*' />
            RebuildProjPicker = 1020,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CleanProjPicker"]/*' />
            CleanProjPicker = 1021,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.DeployProjPicker"]/*' />
            DeployProjPicker = 1022,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ResourceView"]/*' />
            ResourceView = 1023,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ShowHomePage"]/*' />
            ShowHomePage = 1024,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.EditMenuIDs"]/*' />
            EditMenuIDs = 1025,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.LineBreak"]/*' />
            LineBreak = 1026,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CPPIdentifier"]/*' />
            CPPIdentifier = 1027,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.QuotedString"]/*' />
            QuotedString = 1028,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SpaceOrTab"]/*' />
            SpaceOrTab = 1029,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Integer"]/*' />
            Integer = 1030,
            //unused 1031-1035

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CustomizeToolbars"]/*' />
            CustomizeToolbars = 1036,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MoveToTop"]/*' />
            MoveToTop = 1037,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.WindowHelp"]/*' />
            WindowHelp = 1038,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewPopup"]/*' />
            ViewPopup = 1039,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CheckMnemonics"]/*' />
            CheckMnemonics = 1040,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.PRSortAlphabeticaly"]/*' />
            PRSortAlphabeticaly = 1041,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.PRSortByCategory"]/*' />
            PRSortByCategory = 1042,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ViewNextTab"]/*' />
            ViewNextTab = 1043,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CheckForUpdates"]/*' />
            CheckForUpdates = 1044,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Browser1"]/*' />
            Browser1 = 1045,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Browser2"]/*' />
            Browser2 = 1046,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Browser3"]/*' />
            Browser3 = 1047,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Browser4"]/*' />
            Browser4 = 1048,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Browser5"]/*' />
            Browser5 = 1049,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Browser6"]/*' />
            Browser6 = 1050,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Browser7"]/*' />
            Browser7 = 1051,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Browser8"]/*' />
            Browser8 = 1052,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Browser9"]/*' />
            Browser9 = 1053,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Browser10"]/*' />
            Browser10 = 1054,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Browser11"]/*' />
            Browser11 = 1055,  //note unused on purpose to end list

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OpenDropDownOpen"]/*' />
            OpenDropDownOpen = 1058,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OpenDropDownOpenWith"]/*' />
            OpenDropDownOpenWith = 1059,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ToolsDebugProcesses"]/*' />
            ToolsDebugProcesses = 1060,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.PaneNextSubPane"]/*' />
            PaneNextSubPane = 1062,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.PanePrevSubPane"]/*' />
            PanePrevSubPane = 1063,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MoveFileToProject1"]/*' />
            MoveFileToProject1 = 1070,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MoveFileToProject2"]/*' />
            MoveFileToProject2 = 1071,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MoveFileToProject3"]/*' />
            MoveFileToProject3 = 1072,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MoveFileToProject4"]/*' />
            MoveFileToProject4 = 1073,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MoveFileToProject5"]/*' />
            MoveFileToProject5 = 1074,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MoveFileToProject6"]/*' />
            MoveFileToProject6 = 1075,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MoveFileToProject7"]/*' />
            MoveFileToProject7 = 1076,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MoveFileToProject8"]/*' />
            MoveFileToProject8 = 1077,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MoveFileToProject9"]/*' />
            MoveFileToProject9 = 1078,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MoveFileToProjectLast"]/*' />
            MoveFileToProjectLast = 1079,  // unused in order to end list
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.MoveFileToProjectPick"]/*' />
            MoveFileToProjectPick = 1081,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.DefineSubset"]/*' />
            DefineSubset = 1095,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SubsetCombo"]/*' />
            SubsetCombo = 1096,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SubsetGetList"]/*' />
            SubsetGetList = 1097,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OBSortObjectsAlpha"]/*' />
            OBSortObjectsAlpha = 1098,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OBSortObjectsType"]/*' />
            OBSortObjectsType = 1099,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OBSortObjectsAccess"]/*' />
            OBSortObjectsAccess = 1100,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OBGroupObjectsType"]/*' />
            OBGroupObjectsType = 1101,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OBGroupObjectsAccess"]/*' />
            OBGroupObjectsAccess = 1102,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OBSortMembersAlpha"]/*' />
            OBSortMembersAlpha = 1103,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OBSortMembersType"]/*' />
            OBSortMembersType = 1104,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OBSortMembersAccess"]/*' />
            OBSortMembersAccess = 1105,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.PopBrowseContext"]/*' />
            PopBrowseContext = 1106,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.GotoRef"]/*' />
            GotoRef = 1107,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.OBSLookInReferences"]/*' />
            OBSLookInReferences = 1108,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ExtToolsTargetPath"]/*' />
            ExtToolsTargetPath = 1109,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ExtToolsTargetDir"]/*' />
            ExtToolsTargetDir = 1110,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ExtToolsTargetFileName"]/*' />
            ExtToolsTargetFileName = 1111,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ExtToolsTargetExtension"]/*' />
            ExtToolsTargetExtension = 1112,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ExtToolsCurLine"]/*' />
            ExtToolsCurLine = 1113,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ExtToolsCurCol"]/*' />
            ExtToolsCurCol = 1114,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ExtToolsCurText"]/*' />
            ExtToolsCurText = 1115,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BrowseNext"]/*' />
            BrowseNext = 1116,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BrowsePrev"]/*' />
            BrowsePrev = 1117,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BrowseUnload"]/*' />
            BrowseUnload = 1118,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.QuickObjectSearch"]/*' />
            QuickObjectSearch = 1119,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.ExpandAll"]/*' />
            ExpandAll = 1120,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.BookmarkWindow"]/*' />
            BookmarkWindow = 1122,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.CodeExpansionWindow"]/*' />
            CodeExpansionWindow = 1123,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.NextDocumentNav"]/*' />
            NextDocumentNav = 1124,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.PrevDocumentNav"]/*' />
            PrevDocumentNav = 1125,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.StandardMax"]/*' />
            StandardMax = 1500,

            ///////////////////////////////////////////
            //
            // cmdidStandardMax is now thought to be
            // obsolete. Any new shell commands should
            // be added to the end of StandardCommandSet2K
            // which appears below.
            //
            // If you are not adding shell commands,
            // you shouldn't be doing it in this file! 
            //
            ///////////////////////////////////////////


            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.FormsFirst"]/*' />
            FormsFirst = 0x00006000,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.FormsLast"]/*' />
            FormsLast = 0x00006FFF,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.VBEFirst"]/*' />
            VBEFirst = 0x00008000,


            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Zoom200"]/*' />
            Zoom200 = 0x00008002,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Zoom150"]/*' />
            Zoom150 = 0x00008003,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Zoom100"]/*' />
            Zoom100 = 0x00008004,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Zoom75"]/*' />
            Zoom75 = 0x00008005,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Zoom50"]/*' />
            Zoom50 = 0x00008006,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Zoom25"]/*' />
            Zoom25 = 0x00008007,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.Zoom10"]/*' />
            Zoom10 = 0x00008010,


            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.VBELast"]/*' />
            VBELast = 0x00009FFF,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SterlingFirst"]/*' />
            SterlingFirst = 0x0000A000,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.SterlingLast"]/*' />
            SterlingLast = 0x0000BFFF,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.uieventidFirst"]/*' />
            uieventidFirst = 0xC000,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.uieventidSelectRegion"]/*' />
            uieventidSelectRegion = 0xC001,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.uieventidDrop"]/*' />
            uieventidDrop = 0xC002,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd97CmdID.uieventidLast"]/*' />
            uieventidLast = 0xDFFF,
        }

        /// <include file='doc\VSConstants.uex' path='docs/doc[@for="Constants.VSStd2K"]/*' />
        /// <summary>
        /// GUID for the 2K command set. This is a set of standard editor commands.
        /// </summary>
        public static readonly Guid VSStd2K = new Guid("{1496A755-94DE-11D0-8C3F-00C04FC2AAE2}");

        /// <include file='doc\VSConstants.uex' path='docs/doc[@for="Constants.VSStd2KCmdID"]/*' />
        /// <summary>
        /// Set of the standard, shared editor commands in StandardCommandSet2k.
        /// </summary>
        [Guid("1496A755-94DE-11D0-8C3F-00C04FC2AAE2")]
        public enum VSStd2KCmdID {
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.TYPECHAR"]/*' />
            TYPECHAR = 1,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.BACKSPACE"]/*' />
            BACKSPACE = 2,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.RETURN"]/*' />
            RETURN = 3,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.TAB"]/*' />
            TAB = 4,  // test
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.BACKTAB"]/*' />
            BACKTAB = 5,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DELETE"]/*' />
            DELETE = 6,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.LEFT"]/*' />
            LEFT = 7,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.LEFT_EXT"]/*' />
            LEFT_EXT = 8,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.RIGHT"]/*' />
            RIGHT = 9,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.RIGHT_EXT"]/*' />
            RIGHT_EXT = 10,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.UP"]/*' />
            UP = 11,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.UP_EXT"]/*' />
            UP_EXT = 12,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DOWN"]/*' />
            DOWN = 13,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DOWN_EXT"]/*' />
            DOWN_EXT = 14,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.HOME"]/*' />
            HOME = 15,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.HOME_EXT"]/*' />
            HOME_EXT = 16,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.END"]/*' />
            END = 17,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.END_EXT"]/*' />
            END_EXT = 18,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.BOL"]/*' />
            BOL = 19,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.BOL_EXT"]/*' />
            BOL_EXT = 20,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.FIRSTCHAR"]/*' />
            FIRSTCHAR = 21,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.FIRSTCHAR_EXT"]/*' />
            FIRSTCHAR_EXT = 22,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.EOL"]/*' />
            EOL = 23,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.EOL_EXT"]/*' />
            EOL_EXT = 24,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.LASTCHAR"]/*' />
            LASTCHAR = 25,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.LASTCHAR_EXT"]/*' />
            LASTCHAR_EXT = 26,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.PAGEUP"]/*' />
            PAGEUP = 27,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.PAGEUP_EXT"]/*' />
            PAGEUP_EXT = 28,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.PAGEDN"]/*' />
            PAGEDN = 29,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.PAGEDN_EXT"]/*' />
            PAGEDN_EXT = 30,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.TOPLINE"]/*' />
            TOPLINE = 31,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.TOPLINE_EXT"]/*' />
            TOPLINE_EXT = 32,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.BOTTOMLINE"]/*' />
            BOTTOMLINE = 33,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.BOTTOMLINE_EXT"]/*' />
            BOTTOMLINE_EXT = 34,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SCROLLUP"]/*' />
            SCROLLUP = 35,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SCROLLDN"]/*' />
            SCROLLDN = 36,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SCROLLPAGEUP"]/*' />
            SCROLLPAGEUP = 37,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SCROLLPAGEDN"]/*' />
            SCROLLPAGEDN = 38,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SCROLLLEFT"]/*' />
            SCROLLLEFT = 39,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SCROLLRIGHT"]/*' />
            SCROLLRIGHT = 40,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SCROLLBOTTOM"]/*' />
            SCROLLBOTTOM = 41,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SCROLLCENTER"]/*' />
            SCROLLCENTER = 42,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SCROLLTOP"]/*' />
            SCROLLTOP = 43,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SELECTALL"]/*' />
            SELECTALL = 44,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SELTABIFY"]/*' />
            SELTABIFY = 45,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SELUNTABIFY"]/*' />
            SELUNTABIFY = 46,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SELLOWCASE"]/*' />
            SELLOWCASE = 47,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SELUPCASE"]/*' />
            SELUPCASE = 48,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SELTOGGLECASE"]/*' />
            SELTOGGLECASE = 49,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SELTITLECASE"]/*' />
            SELTITLECASE = 50,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SELSWAPANCHOR"]/*' />
            SELSWAPANCHOR = 51,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.GOTOLINE"]/*' />
            GOTOLINE = 52,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.GOTOBRACE"]/*' />
            GOTOBRACE = 53,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.GOTOBRACE_EXT"]/*' />
            GOTOBRACE_EXT = 54,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.GOBACK"]/*' />
            GOBACK = 55,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SELECTMODE"]/*' />
            SELECTMODE = 56,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.TOGGLE_OVERTYPE_MODE"]/*' />
            TOGGLE_OVERTYPE_MODE = 57,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CUT"]/*' />
            CUT = 58,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.COPY"]/*' />
            COPY = 59,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.PASTE"]/*' />
            PASTE = 60,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CUTLINE"]/*' />
            CUTLINE = 61,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DELETELINE"]/*' />
            DELETELINE = 62,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DELETEBLANKLINES"]/*' />
            DELETEBLANKLINES = 63,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DELETEWHITESPACE"]/*' />
            DELETEWHITESPACE = 64,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DELETETOEOL"]/*' />
            DELETETOEOL = 65,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DELETETOBOL"]/*' />
            DELETETOBOL = 66,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.OPENLINEABOVE"]/*' />
            OPENLINEABOVE = 67,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.OPENLINEBELOW"]/*' />
            OPENLINEBELOW = 68,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.INDENT"]/*' />
            INDENT = 69,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.UNINDENT"]/*' />
            UNINDENT = 70,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.UNDO"]/*' />
            UNDO = 71,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.UNDONOMOVE"]/*' />
            UNDONOMOVE = 72,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.REDO"]/*' />
            REDO = 73,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.REDONOMOVE"]/*' />
            REDONOMOVE = 74,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DELETEALLTEMPBOOKMARKS"]/*' />
            DELETEALLTEMPBOOKMARKS = 75,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.TOGGLETEMPBOOKMARK"]/*' />
            TOGGLETEMPBOOKMARK = 76,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.GOTONEXTBOOKMARK"]/*' />
            GOTONEXTBOOKMARK = 77,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.GOTOPREVBOOKMARK"]/*' />
            GOTOPREVBOOKMARK = 78,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.FIND"]/*' />
            FIND = 79,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.REPLACE"]/*' />
            REPLACE = 80,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.REPLACE_ALL"]/*' />
            REPLACE_ALL = 81,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.FINDNEXT"]/*' />
            FINDNEXT = 82,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.FINDNEXTWORD"]/*' />
            FINDNEXTWORD = 83,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.FINDPREV"]/*' />
            FINDPREV = 84,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.FINDPREVWORD"]/*' />
            FINDPREVWORD = 85,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.FINDAGAIN"]/*' />
            FINDAGAIN = 86,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.TRANSPOSECHAR"]/*' />
            TRANSPOSECHAR = 87,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.TRANSPOSEWORD"]/*' />
            TRANSPOSEWORD = 88,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.TRANSPOSELINE"]/*' />
            TRANSPOSELINE = 89,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SELECTCURRENTWORD"]/*' />
            SELECTCURRENTWORD = 90,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DELETEWORDRIGHT"]/*' />
            DELETEWORDRIGHT = 91,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DELETEWORDLEFT"]/*' />
            DELETEWORDLEFT = 92,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.WORDPREV"]/*' />
            WORDPREV = 93,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.WORDPREV_EXT"]/*' />
            WORDPREV_EXT = 94,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.WORDNEXT"]/*' />
            WORDNEXT = 96,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.WORDNEXT_EXT"]/*' />
            WORDNEXT_EXT = 97,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.COMMENTBLOCK"]/*' />
            COMMENTBLOCK = 98,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.UNCOMMENTBLOCK"]/*' />
            UNCOMMENTBLOCK = 99,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SETREPEATCOUNT"]/*' />
            SETREPEATCOUNT = 100,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.WIDGETMARGIN_LBTNDOWN"]/*' />
            WIDGETMARGIN_LBTNDOWN = 101,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SHOWCONTEXTMENU"]/*' />
            SHOWCONTEXTMENU = 102,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CANCEL"]/*' />
            CANCEL = 103,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.PARAMINFO"]/*' />
            PARAMINFO = 104,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.TOGGLEVISSPACE"]/*' />
            TOGGLEVISSPACE = 105,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.TOGGLECARETPASTEPOS"]/*' />
            TOGGLECARETPASTEPOS = 106,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.COMPLETEWORD"]/*' />
            COMPLETEWORD = 107,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SHOWMEMBERLIST"]/*' />
            SHOWMEMBERLIST = 108,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.FIRSTNONWHITEPREV"]/*' />
            FIRSTNONWHITEPREV = 109,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.FIRSTNONWHITENEXT"]/*' />
            FIRSTNONWHITENEXT = 110,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.HELPKEYWORD"]/*' />
            HELPKEYWORD = 111,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.FORMATSELECTION"]/*' />
            FORMATSELECTION = 112,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.OPENURL"]/*' />
            OPENURL = 113,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.INSERTFILE"]/*' />
            INSERTFILE = 114,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.TOGGLESHORTCUT"]/*' />
            TOGGLESHORTCUT = 115,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.QUICKINFO"]/*' />
            QUICKINFO = 116,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.LEFT_EXT_COL"]/*' />
            LEFT_EXT_COL = 117,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.RIGHT_EXT_COL"]/*' />
            RIGHT_EXT_COL = 118,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.UP_EXT_COL"]/*' />
            UP_EXT_COL = 119,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DOWN_EXT_COL"]/*' />
            DOWN_EXT_COL = 120,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.TOGGLEWORDWRAP"]/*' />
            TOGGLEWORDWRAP = 121,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ISEARCH"]/*' />
            ISEARCH = 122,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ISEARCHBACK"]/*' />
            ISEARCHBACK = 123,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.BOL_EXT_COL"]/*' />
            BOL_EXT_COL = 124,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.EOL_EXT_COL"]/*' />
            EOL_EXT_COL = 125,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.WORDPREV_EXT_COL"]/*' />
            WORDPREV_EXT_COL = 126,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.WORDNEXT_EXT_COL"]/*' />
            WORDNEXT_EXT_COL = 127,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.OUTLN_HIDE_SELECTION"]/*' />
            OUTLN_HIDE_SELECTION = 128,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.OUTLN_TOGGLE_CURRENT"]/*' />
            OUTLN_TOGGLE_CURRENT = 129,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.OUTLN_TOGGLE_ALL"]/*' />
            OUTLN_TOGGLE_ALL = 130,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.OUTLN_STOP_HIDING_ALL"]/*' />
            OUTLN_STOP_HIDING_ALL = 131,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.OUTLN_STOP_HIDING_CURRENT"]/*' />
            OUTLN_STOP_HIDING_CURRENT = 132,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.OUTLN_COLLAPSE_TO_DEF"]/*' />
            OUTLN_COLLAPSE_TO_DEF = 133,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DOUBLECLICK"]/*' />
            DOUBLECLICK = 134,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.EXTERNALLY_HANDLED_WIDGET_CLICK"]/*' />
            EXTERNALLY_HANDLED_WIDGET_CLICK = 135,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.COMMENT_BLOCK"]/*' />
            COMMENT_BLOCK = 136,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.UNCOMMENT_BLOCK"]/*' />
            UNCOMMENT_BLOCK = 137,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.OPENFILE"]/*' />
            OPENFILE = 138,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NAVIGATETOURL"]/*' />
            NAVIGATETOURL = 139,

            // For editor internal use only
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.HANDLEIMEMESSAGE"]/*' />
            HANDLEIMEMESSAGE = 140,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SELTOGOBACK"]/*' />
            SELTOGOBACK = 141,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.COMPLETION_HIDE_ADVANCED"]/*' />
            COMPLETION_HIDE_ADVANCED = 142,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.FORMATDOCUMENT"]/*' />
            FORMATDOCUMENT = 143,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.OUTLN_START_AUTOHIDING"]/*' />
            OUTLN_START_AUTOHIDING = 144,

            // Last Standard Editor Command (+1)
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.FINAL"]/*' />
            FINAL = 145,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_DECREASEFILTER"]/*' />
            ECMD_DECREASEFILTER = 146,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_COPYTIP"]/*' />
            ECMD_COPYTIP = 148,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_PASTETIP"]/*' />
            ECMD_PASTETIP = 149,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_LEFTCLICK"]/*' />
            ECMD_LEFTCLICK = 150,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_GOTONEXTBOOKMARKINDOC"]/*' />
            ECMD_GOTONEXTBOOKMARKINDOC = 151,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_GOTOPREVBOOKMARKINDOC"]/*' />
            ECMD_GOTOPREVBOOKMARKINDOC = 152,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_INVOKESNIPPETFROMSHORTCUT"]/*' />
            ECMD_INVOKESNIPPETFROMSHORTCUT = 154,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.AUTOCOMPLETE"]/*' />
            AUTOCOMPLETE = 155,

            ///  <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_INVOKESNIPPETPICKER2"]/*' />
            ECMD_INVOKESNIPPETPICKER2 = 156,
            ///  <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_DELETEALLBOOKMARKSINDOC"]/*' />
            ECMD_DELETEALLBOOKMARKSINDOC = 157,
            ///  <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_CONVERTTABSTOSPACES"]/*' />
            ECMD_CONVERTTABSTOSPACES = 158,
            ///  <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_CONVERTSPACESTOTABS"]/*' />
            ECMD_CONVERTSPACESTOTABS = 159,
            ///  <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_FINAL"]/*' />
            ECMD_FINAL = 160,

            ///////////////////////////////////////////////////////////////
            // Some new commands created during CTC file rationalisation
            ///////////////////////////////////////////////////////////////
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.STOP"]/*' />
            STOP = 220,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.REVERSECANCEL"]/*' />
            REVERSECANCEL = 221,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SLNREFRESH"]/*' />
            SLNREFRESH = 222,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SAVECOPYOFITEMAS"]/*' />
            SAVECOPYOFITEMAS = 223,
            //
            // Shareable commands originating in the HTML editor
            //
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NEWELEMENT"]/*' />
            NEWELEMENT = 224,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NEWATTRIBUTE"]/*' />
            NEWATTRIBUTE = 225,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NEWCOMPLEXTYPE"]/*' />
            NEWCOMPLEXTYPE = 226,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NEWSIMPLETYPE"]/*' />
            NEWSIMPLETYPE = 227,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NEWGROUP"]/*' />
            NEWGROUP = 228,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NEWATTRIBUTEGROUP"]/*' />
            NEWATTRIBUTEGROUP = 229,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NEWKEY"]/*' />
            NEWKEY = 230,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NEWRELATION"]/*' />
            NEWRELATION = 231,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.EDITKEY"]/*' />
            EDITKEY = 232,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.EDITRELATION"]/*' />
            EDITRELATION = 233,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.MAKETYPEGLOBAL"]/*' />
            MAKETYPEGLOBAL = 234,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.PREVIEWDATASET"]/*' />
            PREVIEWDATASET = 235,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.GENERATEDATASET"]/*' />
            GENERATEDATASET = 236,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CREATESCHEMA"]/*' />
            CREATESCHEMA = 237,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.LAYOUTINDENT"]/*' />
            LAYOUTINDENT = 238,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.LAYOUTUNINDENT"]/*' />
            LAYOUTUNINDENT = 239,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.REMOVEHANDLER"]/*' />
            REMOVEHANDLER = 240,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.EDITHANDLER"]/*' />
            EDITHANDLER = 241,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDHANDLER"]/*' />
            ADDHANDLER = 242,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.STYLE"]/*' />
            STYLE = 243,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.STYLEGETLIST"]/*' />
            STYLEGETLIST = 244,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.FONTSTYLE"]/*' />
            FONTSTYLE = 245,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.FONTSTYLEGETLIST"]/*' />
            FONTSTYLEGETLIST = 246,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.PASTEASHTML"]/*' />
            PASTEASHTML = 247,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.VIEWBORDERS"]/*' />
            VIEWBORDERS = 248,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.VIEWDETAILS"]/*' />
            VIEWDETAILS = 249,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.EXPANDCONTROLS"]/*' />
            EXPANDCONTROLS = 250,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.COLLAPSECONTROLS"]/*' />
            COLLAPSECONTROLS = 251,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SHOWSCRIPTONLY"]/*' />
            SHOWSCRIPTONLY = 252,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.INSERTTABLE"]/*' />
            INSERTTABLE = 253,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.INSERTCOLLEFT"]/*' />
            INSERTCOLLEFT = 254,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.INSERTCOLRIGHT"]/*' />
            INSERTCOLRIGHT = 255,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.INSERTROWABOVE"]/*' />
            INSERTROWABOVE = 256,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.INSERTROWBELOW"]/*' />
            INSERTROWBELOW = 257,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DELETETABLE"]/*' />
            DELETETABLE = 258,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DELETECOLS"]/*' />
            DELETECOLS = 259,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DELETEROWS"]/*' />
            DELETEROWS = 260,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SELECTTABLE"]/*' />
            SELECTTABLE = 261,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SELECTTABLECOL"]/*' />
            SELECTTABLECOL = 262,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SELECTTABLEROW"]/*' />
            SELECTTABLEROW = 263,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SELECTTABLECELL"]/*' />
            SELECTTABLECELL = 264,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.MERGECELLS"]/*' />
            MERGECELLS = 265,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SPLITCELL"]/*' />
            SPLITCELL = 266,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.INSERTCELL"]/*' />
            INSERTCELL = 267,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DELETECELLS"]/*' />
            DELETECELLS = 268,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SEAMLESSFRAME"]/*' />
            SEAMLESSFRAME = 269,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.VIEWFRAME"]/*' />
            VIEWFRAME = 270,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DELETEFRAME"]/*' />
            DELETEFRAME = 271,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SETFRAMESOURCE"]/*' />
            SETFRAMESOURCE = 272,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NEWLEFTFRAME"]/*' />
            NEWLEFTFRAME = 273,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NEWRIGHTFRAME"]/*' />
            NEWRIGHTFRAME = 274,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NEWTOPFRAME"]/*' />
            NEWTOPFRAME = 275,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NEWBOTTOMFRAME"]/*' />
            NEWBOTTOMFRAME = 276,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SHOWGRID"]/*' />
            SHOWGRID = 277,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SNAPTOGRID"]/*' />
            SNAPTOGRID = 278,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.BOOKMARK"]/*' />
            BOOKMARK = 279,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.HYPERLINK"]/*' />
            HYPERLINK = 280,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.IMAGE"]/*' />
            IMAGE = 281,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.INSERTFORM"]/*' />
            INSERTFORM = 282,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.INSERTSPAN"]/*' />
            INSERTSPAN = 283,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SMARTTASKS"]/*' />
            DIV = 284,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.HTMLCLIENTSCRIPTBLOCK"]/*' />
            HTMLCLIENTSCRIPTBLOCK = 285,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.HTMLSERVERSCRIPTBLOCK"]/*' />
            HTMLSERVERSCRIPTBLOCK = 286,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.BULLETEDLIST"]/*' />
            BULLETEDLIST = 287,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NUMBEREDLIST"]/*' />
            NUMBEREDLIST = 288,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.EDITSCRIPT"]/*' />
            EDITSCRIPT = 289,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.EDITCODEBEHIND"]/*' />
            EDITCODEBEHIND = 290,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DOCOUTLINEHTML"]/*' />
            DOCOUTLINEHTML = 291,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DOCOUTLINESCRIPT"]/*' />
            DOCOUTLINESCRIPT = 292,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.RUNATSERVER"]/*' />
            RUNATSERVER = 293,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.WEBFORMSVERBS"]/*' />
            WEBFORMSVERBS = 294,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.WEBFORMSTEMPLATES"]/*' />
            WEBFORMSTEMPLATES = 295,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ENDTEMPLATE"]/*' />
            ENDTEMPLATE = 296,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.EDITDEFAULTEVENT"]/*' />
            EDITDEFAULTEVENT = 297,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SUPERSCRIPT"]/*' />
            SUPERSCRIPT = 298,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SUBSCRIPT"]/*' />
            SUBSCRIPT = 299,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.EDITSTYLE"]/*' />
            EDITSTYLE = 300,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDIMAGEHEIGHTWIDTH"]/*' />
            ADDIMAGEHEIGHTWIDTH = 301,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.REMOVEIMAGEHEIGHTWIDTH"]/*' />
            REMOVEIMAGEHEIGHTWIDTH = 302,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.LOCKELEMENT"]/*' />
            LOCKELEMENT = 303,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.VIEWSTYLEORGANIZER"]/*' />
            VIEWSTYLEORGANIZER = 304,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_AUTOCLOSEOVERRIDE"]/*' />
            ECMD_AUTOCLOSEOVERRIDE = 305,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NEWANY"]/*' />
            NEWANY = 306,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NEWANYATTRIBUTE"]/*' />
            NEWANYATTRIBUTE = 307,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DELETEKEY"]/*' />
            DELETEKEY = 308,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.AUTOARRANGE"]/*' />
            AUTOARRANGE = 309,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.VALIDATESCHEMA"]/*' />
            VALIDATESCHEMA = 310,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NEWFACET"]/*' />
            NEWFACET = 311,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.VALIDATEXMLDATA"]/*' />
            VALIDATEXMLDATA = 312,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DOCOUTLINETOGGLE"]/*' />
            DOCOUTLINETOGGLE = 313,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.VALIDATEHTMLDATA"]/*' />
            VALIDATEHTMLDATA = 314,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.VIEWXMLSCHEMAOVERVIEW"]/*' />
            VIEWXMLSCHEMAOVERVIEW = 315,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SHOWDEFAULTVIEW"]/*' />
            SHOWDEFAULTVIEW = 316,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.EXPAND_CHILDREN"]/*' />
            EXPAND_CHILDREN = 317,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.COLLAPSE_CHILDREN"]/*' />
            COLLAPSE_CHILDREN = 318,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.TOPDOWNLAYOUT"]/*' />
            TOPDOWNLAYOUT = 319,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.LEFTRIGHTLAYOUT"]/*' />
            LEFTRIGHTLAYOUT = 320,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.INSERTCELLRIGHT"]/*' />
            INSERTCELLRIGHT = 321,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.EDITMASTER"]/*' />
            EDITMASTER = 322,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.INSERTSNIPPET"]/*' />
            INSERTSNIPPET = 323,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.FORMATANDVALIDATION"]/*' />
            FORMATANDVALIDATION = 324,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.COLLAPSETAG"]/*' />
            COLLAPSETAG = 325,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SELECT_TAG"]/*' />
            SELECT_TAG = 329,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SELECT_TAG_CONTENT"]/*' />
            SELECT_TAG_CONTENT = 330,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CHECK_ACCESSIBILITY"]/*' />
            CHECK_ACCESSIBILITY = 331,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.UNCOLLAPSETAG"]/*' />
            UNCOLLAPSETAG = 332,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.GENERATEPAGERESOURCE"]/*' />
            GENERATEPAGERESOURCE = 333,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SHOWNONVISUALCONTROLS"]/*' />
            SHOWNONVISUALCONTROLS = 334,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.RESIZECOLUMN"]/*' />
            RESIZECOLUMN = 335,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.RESIZEROW"]/*' />
            RESIZEROW = 336,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.MAKEABSOLUTE"]/*' />
            MAKEABSOLUTE = 337,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.MAKERELATIVE"]/*' />
            MAKERELATIVE = 338,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.MAKESTATIC"]/*' />
            MAKESTATIC = 339,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.INSERTLAYER"]/*' />
            INSERTLAYER = 340,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.UPDATEDESIGNVIEW"]/*' />
            UPDATEDESIGNVIEW = 341,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.UPDATESOURCEVIEW"]/*' />
            UPDATESOURCEVIEW = 342,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.INSERTCAPTION"]/*' />
            INSERTCAPTION = 343,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DELETECAPTION"]/*' />
            DELETECAPTION = 344,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.MAKEPOSITIONNOTSET"]/*' />
            MAKEPOSITIONNOTSET = 345,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.AUTOPOSITIONOPTIONS"]/*' />
            AUTOPOSITIONOPTIONS = 346,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.EDITIMAGE"]/*' />
            EDITIMAGE = 347,

            //
            // Shareable commands originating in the VC project
            //
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.COMPILE"]/*' />
            COMPILE = 350,
            //
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.PROJSETTINGS"]/*' />
            PROJSETTINGS = 352,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.LINKONLY"]/*' />
            LINKONLY = 353,
            //
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.REMOVE"]/*' />
            REMOVE = 355,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.PROJSTARTDEBUG"]/*' />
            PROJSTARTDEBUG = 356,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.PROJSTEPINTO"]/*' />
            PROJSTEPINTO = 357,
            //
            //
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.UPDATEWEBREF"]/*' />
            UPDATEWEBREF = 360,
            //
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDRESOURCE"]/*' />
            ADDRESOURCE = 362,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.WEBDEPLOY"]/*' />
            WEBDEPLOY = 363,
            //
            // Shareable commands originating in the VB and VBA projects
            // Note that there are two versions of each command. One
            // version is originally from the main (project) menu and the
            // other version from a cascading "Add" context menu. The main
            // difference between the two commands is that the main menu
            // version starts with the text "Add" whereas this is not
            // present on the context menu version.
            //
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDHTMLPAGE"]/*' />
            ADDHTMLPAGE = 400,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDHTMLPAGECTX"]/*' />
            ADDHTMLPAGECTX = 401,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDMODULE"]/*' />
            ADDMODULE = 402,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDMODULECTX"]/*' />
            ADDMODULECTX = 403,
            // unused 404
            // unused 405
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDWFCFORM"]/*' />
            ADDWFCFORM = 406,
            // unused 407
            // unused 408
            // unused 409
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDWEBFORM"]/*' />
            ADDWEBFORM = 410,
            // unused 411
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDUSERCONTROL"]/*' />
            ADDUSERCONTROL = 412,
            // unused 413 to 425
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDDHTMLPAGE"]/*' />
            ADDDHTMLPAGE = 426,
            // unused 427 to 431
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDIMAGEGENERATOR"]/*' />
            ADDIMAGEGENERATOR = 432,
            // unused 433
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDINHERWFCFORM"]/*' />
            ADDINHERWFCFORM = 434,
            // unused 435
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDINHERCONTROL"]/*' />
            ADDINHERCONTROL = 436,
            // unused 437
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDWEBUSERCONTROL"]/*' />
            ADDWEBUSERCONTROL = 438,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.BUILDANDBROWSE"]/*' />
            BUILDANDBROWSE = 439,
            // unused 440
            // unused 441
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDTBXCOMPONENT"]/*' />
            ADDTBXCOMPONENT = 442,
            // unused 443
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDWEBSERVICE"]/*' />
            ADDWEBSERVICE = 444,
            // unused 445
            //
            // Shareable commands originating in the VFP project
            //
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDVFPPAGE"]/*' />
            ADDVFPPAGE = 500,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SETBREAKPOINT"]/*' />
            SETBREAKPOINT = 501,
            //
            // Shareable commands originating in the HELP WORKSHOP project
            //
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SHOWALLFILES"]/*' />
            SHOWALLFILES = 600,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDTOPROJECT"]/*' />
            ADDTOPROJECT = 601,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDBLANKNODE"]/*' />
            ADDBLANKNODE = 602,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDNODEFROMFILE"]/*' />
            ADDNODEFROMFILE = 603,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CHANGEURLFROMFILE"]/*' />
            CHANGEURLFROMFILE = 604,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.EDITTOPIC"]/*' />
            EDITTOPIC = 605,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.EDITTITLE"]/*' />
            EDITTITLE = 606,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.MOVENODEUP"]/*' />
            MOVENODEUP = 607,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.MOVENODEDOWN"]/*' />
            MOVENODEDOWN = 608,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.MOVENODELEFT"]/*' />
            MOVENODELEFT = 609,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.MOVENODERIGHT"]/*' />
            MOVENODERIGHT = 610,
            //
            // Shareable commands originating in the Deploy project
            //
            // Note there are two groups of deploy project commands.
            // The first group of deploy commands.
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDOUTPUT"]/*' />
            ADDOUTPUT = 700,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDFILE"]/*' />
            ADDFILE = 701,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.MERGEMODULE"]/*' />
            MERGEMODULE = 702,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDCOMPONENTS"]/*' />
            ADDCOMPONENTS = 703,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.LAUNCHINSTALLER"]/*' />
            LAUNCHINSTALLER = 704,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.LAUNCHUNINSTALL"]/*' />
            LAUNCHUNINSTALL = 705,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.LAUNCHORCA"]/*' />
            LAUNCHORCA = 706,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.FILESYSTEMEDITOR"]/*' />
            FILESYSTEMEDITOR = 707,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.REGISTRYEDITOR"]/*' />
            REGISTRYEDITOR = 708,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.FILETYPESEDITOR"]/*' />
            FILETYPESEDITOR = 709,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.USERINTERFACEEDITOR"]/*' />
            USERINTERFACEEDITOR = 710,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CUSTOMACTIONSEDITOR"]/*' />
            CUSTOMACTIONSEDITOR = 711,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.LAUNCHCONDITIONSEDITOR"]/*' />
            LAUNCHCONDITIONSEDITOR = 712,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.EDITOR"]/*' />
            EDITOR = 713,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.EXCLUDE"]/*' />
            EXCLUDE = 714,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.REFRESHDEPENDENCIES"]/*' />
            REFRESHDEPENDENCIES = 715,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.VIEWOUTPUTS"]/*' />
            VIEWOUTPUTS = 716,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.VIEWDEPENDENCIES"]/*' />
            VIEWDEPENDENCIES = 717,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.VIEWFILTER"]/*' />
            VIEWFILTER = 718,

            //
            // The Second group of deploy commands.
            // Note that there is a special sub-group in which the relative 
            // positions are important (see below)
            //
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.KEY"]/*' />
            KEY = 750,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.STRING"]/*' />
            STRING = 751,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.BINARY"]/*' />
            BINARY = 752,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DWORD"]/*' />
            DWORD = 753,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.KEYSOLO"]/*' />
            KEYSOLO = 754,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.IMPORT"]/*' />
            IMPORT = 755,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.FOLDER"]/*' />
            FOLDER = 756,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.PROJECTOUTPUT"]/*' />
            PROJECTOUTPUT = 757,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.FILE"]/*' />
            FILE = 758,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDMERGEMODULES"]/*' />
            ADDMERGEMODULES = 759,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CREATESHORTCUT"]/*' />
            CREATESHORTCUT = 760,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.LARGEICONS"]/*' />
            LARGEICONS = 761,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SMALLICONS"]/*' />
            SMALLICONS = 762,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.LIST"]/*' />
            LIST = 763,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DETAILS"]/*' />
            DETAILS = 764,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDFILETYPE"]/*' />
            ADDFILETYPE = 765,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDACTION"]/*' />
            ADDACTION = 766,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SETASDEFAULT"]/*' />
            SETASDEFAULT = 767,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.MOVEUP"]/*' />
            MOVEUP = 768,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.MOVEDOWN"]/*' />
            MOVEDOWN = 769,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDDIALOG"]/*' />
            ADDDIALOG = 770,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.IMPORTDIALOG"]/*' />
            IMPORTDIALOG = 771,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDFILESEARCH"]/*' />
            ADDFILESEARCH = 772,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDREGISTRYSEARCH"]/*' />
            ADDREGISTRYSEARCH = 773,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDCOMPONENTSEARCH"]/*' />
            ADDCOMPONENTSEARCH = 774,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDLAUNCHCONDITION"]/*' />
            ADDLAUNCHCONDITION = 775,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDCUSTOMACTION"]/*' />
            ADDCUSTOMACTION = 776,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.OUTPUTS"]/*' />
            OUTPUTS = 777,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DEPENDENCIES"]/*' />
            DEPENDENCIES = 778,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.FILTER"]/*' />
            FILTER = 779,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.COMPONENTS"]/*' />
            COMPONENTS = 780,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ENVSTRING"]/*' />
            ENVSTRING = 781,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CREATEEMPTYSHORTCUT"]/*' />
            CREATEEMPTYSHORTCUT = 782,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDFILECONDITION"]/*' />
            ADDFILECONDITION = 783,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDREGISTRYCONDITION"]/*' />
            ADDREGISTRYCONDITION = 784,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDCOMPONENTCONDITION"]/*' />
            ADDCOMPONENTCONDITION = 785,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDURTCONDITION"]/*' />
            ADDURTCONDITION = 786,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDIISCONDITION"]/*' />
            ADDIISCONDITION = 787,

            //
            // The relative positions of the commands within the following deploy
            // subgroup must remain unaltered, although the group as a whole may
            // be repositioned. Note that the first and last elements are special
            // boundary elements.
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SPECIALFOLDERBASE"]/*' />
            SPECIALFOLDERBASE = 800,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.USERSAPPLICATIONDATAFOLDER"]/*' />
            USERSAPPLICATIONDATAFOLDER = 800,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.COMMONFILES64FOLDER"]/*' />
            COMMONFILES64FOLDER = 801,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.COMMONFILESFOLDER"]/*' />
            COMMONFILESFOLDER = 802,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CUSTOMFOLDER"]/*' />
            CUSTOMFOLDER = 803,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.USERSDESKTOP"]/*' />
            USERSDESKTOP = 804,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.USERSFAVORITESFOLDER"]/*' />
            USERSFAVORITESFOLDER = 805,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.FONTSFOLDER"]/*' />
            FONTSFOLDER = 806,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.GLOBALASSEMBLYCACHEFOLDER"]/*' />
            GLOBALASSEMBLYCACHEFOLDER = 807,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.MODULERETARGETABLEFOLDER"]/*' />
            MODULERETARGETABLEFOLDER = 808,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.USERSPERSONALDATAFOLDER"]/*' />
            USERSPERSONALDATAFOLDER = 809,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.PROGRAMFILES64FOLDER"]/*' />
            PROGRAMFILES64FOLDER = 810,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.PROGRAMFILESFOLDER"]/*' />
            PROGRAMFILESFOLDER = 811,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.USERSPROGRAMSMENU"]/*' />
            USERSPROGRAMSMENU = 812,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.USERSSENDTOMENU"]/*' />
            USERSSENDTOMENU = 813,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SHAREDCOMPONENTSFOLDER"]/*' />
            SHAREDCOMPONENTSFOLDER = 814,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.USERSSTARTMENU"]/*' />
            USERSSTARTMENU = 815,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.USERSSTARTUPFOLDER"]/*' />
            USERSSTARTUPFOLDER = 816,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SYSTEM64FOLDER"]/*' />
            SYSTEM64FOLDER = 817,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SYSTEMFOLDER"]/*' />
            SYSTEMFOLDER = 818,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.APPLICATIONFOLDER"]/*' />
            APPLICATIONFOLDER = 819,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.USERSTEMPLATEFOLDER"]/*' />
            USERSTEMPLATEFOLDER = 820,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.WEBCUSTOMFOLDER"]/*' />
            WEBCUSTOMFOLDER = 821,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.WINDOWSFOLDER"]/*' />
            WINDOWSFOLDER = 822,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SPECIALFOLDERLAST"]/*' />
            SPECIALFOLDERLAST = 823,
            // End of deploy sub-group
            //
            // Shareable commands originating in the Visual Studio Analyzer project
            //
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.EXPORTEVENTS"]/*' />
            EXPORTEVENTS = 900,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.IMPORTEVENTS"]/*' />
            IMPORTEVENTS = 901,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.VIEWEVENT"]/*' />
            VIEWEVENT = 902,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.VIEWEVENTLIST"]/*' />
            VIEWEVENTLIST = 903,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.VIEWCHART"]/*' />
            VIEWCHART = 904,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.VIEWMACHINEDIAGRAM"]/*' />
            VIEWMACHINEDIAGRAM = 905,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.VIEWPROCESSDIAGRAM"]/*' />
            VIEWPROCESSDIAGRAM = 906,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.VIEWSOURCEDIAGRAM"]/*' />
            VIEWSOURCEDIAGRAM = 907,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.VIEWSTRUCTUREDIAGRAM"]/*' />
            VIEWSTRUCTUREDIAGRAM = 908,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.VIEWTIMELINE"]/*' />
            VIEWTIMELINE = 909,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.VIEWSUMMARY"]/*' />
            VIEWSUMMARY = 910,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.APPLYFILTER"]/*' />
            APPLYFILTER = 911,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CLEARFILTER"]/*' />
            CLEARFILTER = 912,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.STARTRECORDING"]/*' />
            STARTRECORDING = 913,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.STOPRECORDING"]/*' />
            STOPRECORDING = 914,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.PAUSERECORDING"]/*' />
            PAUSERECORDING = 915,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ACTIVATEFILTER"]/*' />
            ACTIVATEFILTER = 916,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SHOWFIRSTEVENT"]/*' />
            SHOWFIRSTEVENT = 917,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SHOWPREVIOUSEVENT"]/*' />
            SHOWPREVIOUSEVENT = 918,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SHOWNEXTEVENT"]/*' />
            SHOWNEXTEVENT = 919,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SHOWLASTEVENT"]/*' />
            SHOWLASTEVENT = 920,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.REPLAYEVENTS"]/*' />
            REPLAYEVENTS = 921,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.STOPREPLAY"]/*' />
            STOPREPLAY = 922,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.INCREASEPLAYBACKSPEED"]/*' />
            INCREASEPLAYBACKSPEED = 923,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DECREASEPLAYBACKSPEED"]/*' />
            DECREASEPLAYBACKSPEED = 924,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDMACHINE"]/*' />
            ADDMACHINE = 925,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDREMOVECOLUMNS"]/*' />
            ADDREMOVECOLUMNS = 926,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SORTCOLUMNS"]/*' />
            SORTCOLUMNS = 927,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SAVECOLUMNSETTINGS"]/*' />
            SAVECOLUMNSETTINGS = 928,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.RESETCOLUMNSETTINGS"]/*' />
            RESETCOLUMNSETTINGS = 929,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SIZECOLUMNSTOFIT"]/*' />
            SIZECOLUMNSTOFIT = 930,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.AUTOSELECT"]/*' />
            AUTOSELECT = 931,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.AUTOFILTER"]/*' />
            AUTOFILTER = 932,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.AUTOPLAYTRACK"]/*' />
            AUTOPLAYTRACK = 933,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.GOTOEVENT"]/*' />
            GOTOEVENT = 934,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ZOOMTOFIT"]/*' />
            ZOOMTOFIT = 935,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDGRAPH"]/*' />
            ADDGRAPH = 936,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.REMOVEGRAPH"]/*' />
            REMOVEGRAPH = 937,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CONNECTMACHINE"]/*' />
            CONNECTMACHINE = 938,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DISCONNECTMACHINE"]/*' />
            DISCONNECTMACHINE = 939,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.EXPANDSELECTION"]/*' />
            EXPANDSELECTION = 940,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.COLLAPSESELECTION"]/*' />
            COLLAPSESELECTION = 941,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDFILTER"]/*' />
            ADDFILTER = 942,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDPREDEFINED0"]/*' />
            ADDPREDEFINED0 = 943,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDPREDEFINED1"]/*' />
            ADDPREDEFINED1 = 944,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDPREDEFINED2"]/*' />
            ADDPREDEFINED2 = 945,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDPREDEFINED3"]/*' />
            ADDPREDEFINED3 = 946,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDPREDEFINED4"]/*' />
            ADDPREDEFINED4 = 947,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDPREDEFINED5"]/*' />
            ADDPREDEFINED5 = 948,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDPREDEFINED6"]/*' />
            ADDPREDEFINED6 = 949,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDPREDEFINED7"]/*' />
            ADDPREDEFINED7 = 950,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDPREDEFINED8"]/*' />
            ADDPREDEFINED8 = 951,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.TIMELINESIZETOFIT"]/*' />
            TIMELINESIZETOFIT = 952,

            //
            // Shareable commands originating with Crystal Reports
            //
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.FIELDVIEW"]/*' />
            FIELDVIEW = 1000,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SELECTEXPERT"]/*' />
            SELECTEXPERT = 1001,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.TOPNEXPERT"]/*' />
            TOPNEXPERT = 1002,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SORTORDER"]/*' />
            SORTORDER = 1003,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.PROPPAGE"]/*' />
            PROPPAGE = 1004,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.HELP"]/*' />
            HELP = 1005,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SAVEREPORT"]/*' />
            SAVEREPORT = 1006,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.INSERTSUMMARY"]/*' />
            INSERTSUMMARY = 1007,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.INSERTGROUP"]/*' />
            INSERTGROUP = 1008,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.INSERTSUBREPORT"]/*' />
            INSERTSUBREPORT = 1009,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.INSERTCHART"]/*' />
            INSERTCHART = 1010,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.INSERTPICTURE"]/*' />
            INSERTPICTURE = 1011,
            //
            // Shareable commands from the common project area (DirPrj)
            //
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SETASSTARTPAGE"]/*' />
            SETASSTARTPAGE = 1100,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.RECALCULATELINKS"]/*' />
            RECALCULATELINKS = 1101,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.WEBPERMISSIONS"]/*' />
            WEBPERMISSIONS = 1102,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.COMPARETOMASTER"]/*' />
            COMPARETOMASTER = 1103,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.WORKOFFLINE"]/*' />
            WORKOFFLINE = 1104,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SYNCHRONIZEFOLDER"]/*' />
            SYNCHRONIZEFOLDER = 1105,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SYNCHRONIZEALLFOLDERS"]/*' />
            SYNCHRONIZEALLFOLDERS = 1106,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.COPYPROJECT"]/*' />
            COPYPROJECT = 1107,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.IMPORTFILEFROMWEB"]/*' />
            IMPORTFILEFROMWEB = 1108,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.INCLUDEINPROJECT"]/*' />
            INCLUDEINPROJECT = 1109,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.EXCLUDEFROMPROJECT"]/*' />
            EXCLUDEFROMPROJECT = 1110,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.BROKENLINKSREPORT"]/*' />
            BROKENLINKSREPORT = 1111,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDPROJECTOUTPUTS"]/*' />
            ADDPROJECTOUTPUTS = 1112,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDREFERENCE"]/*' />
            ADDREFERENCE = 1113,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDWEBREFERENCE"]/*' />
            ADDWEBREFERENCE = 1114,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDWEBREFERENCECTX"]/*' />
            ADDWEBREFERENCECTX = 1115,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.UPDATEWEBREFERENCE"]/*' />
            UPDATEWEBREFERENCE = 1116,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.RUNCUSTOMTOOL"]/*' />
            RUNCUSTOMTOOL = 1117,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SETRUNTIMEVERSION"]/*' />
            SETRUNTIMEVERSION = 1118,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.VIEWREFINOBJECTBROWSER"]/*' />
            VIEWREFINOBJECTBROWSER = 1119,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.PUBLISH"]/*' />
            PUBLISH = 1120,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.PUBLISHCTX"]/*' />
            PUBLISHCTX = 1121,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.STARTOPTIONS"]/*' />
            STARTOPTIONS = 1124,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADDREFERENCECTX"]/*' />
            ADDREFERENCECTX = 1125,
            // note cmdidPropertyManager is consuming 1126  and it shouldn't
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.STARTOPTIONSCTX"]/*' />
            STARTOPTIONSCTX = 1127,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DETACHLOCALDATAFILECTX"]/*' />
            DETACHLOCALDATAFILECTX = 1128,

            //
            // Shareable commands for right drag operations
            //
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DRAG_MOVE"]/*' />
            DRAG_MOVE = 1140,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DRAG_COPY"]/*' />
            DRAG_COPY = 1141,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DRAG_CANCEL"]/*' />
            DRAG_CANCEL = 1142,

            //
            // Shareable commands from the VC resource editor
            //
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.TESTDIALOG"]/*' />
            TESTDIALOG = 1200,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SPACEACROSS"]/*' />
            SPACEACROSS = 1201,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SPACEDOWN"]/*' />
            SPACEDOWN = 1202,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.TOGGLEGRID"]/*' />
            TOGGLEGRID = 1203,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.TOGGLEGUIDES"]/*' />
            TOGGLEGUIDES = 1204,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SIZETOTEXT"]/*' />
            SIZETOTEXT = 1205,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CENTERVERT"]/*' />
            CENTERVERT = 1206,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CENTERHORZ"]/*' />
            CENTERHORZ = 1207,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.FLIPDIALOG"]/*' />
            FLIPDIALOG = 1208,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SETTABORDER"]/*' />
            SETTABORDER = 1209,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.BUTTONRIGHT"]/*' />
            BUTTONRIGHT = 1210,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.BUTTONBOTTOM"]/*' />
            BUTTONBOTTOM = 1211,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.AUTOLAYOUTGROW"]/*' />
            AUTOLAYOUTGROW = 1212,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.AUTOLAYOUTNORESIZE"]/*' />
            AUTOLAYOUTNORESIZE = 1213,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.AUTOLAYOUTOPTIMIZE"]/*' />
            AUTOLAYOUTOPTIMIZE = 1214,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.GUIDESETTINGS"]/*' />
            GUIDESETTINGS = 1215,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.RESOURCEINCLUDES"]/*' />
            RESOURCEINCLUDES = 1216,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.RESOURCESYMBOLS"]/*' />
            RESOURCESYMBOLS = 1217,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.OPENBINARY"]/*' />
            OPENBINARY = 1218,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.RESOURCEOPEN"]/*' />
            RESOURCEOPEN = 1219,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.RESOURCENEW"]/*' />
            RESOURCENEW = 1220,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.RESOURCENEWCOPY"]/*' />
            RESOURCENEWCOPY = 1221,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.INSERT"]/*' />
            INSERT = 1222,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.EXPORT"]/*' />
            EXPORT = 1223,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CTLMOVELEFT"]/*' />
            CTLMOVELEFT = 1224,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CTLMOVEDOWN"]/*' />
            CTLMOVEDOWN = 1225,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CTLMOVERIGHT"]/*' />
            CTLMOVERIGHT = 1226,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CTLMOVEUP"]/*' />
            CTLMOVEUP = 1227,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CTLSIZEDOWN"]/*' />
            CTLSIZEDOWN = 1228,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CTLSIZEUP"]/*' />
            CTLSIZEUP = 1229,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CTLSIZELEFT"]/*' />
            CTLSIZELEFT = 1230,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CTLSIZERIGHT"]/*' />
            CTLSIZERIGHT = 1231,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NEWACCELERATOR"]/*' />
            NEWACCELERATOR = 1232,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CAPTUREKEYSTROKE"]/*' />
            CAPTUREKEYSTROKE = 1233,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.INSERTACTIVEXCTL"]/*' />
            INSERTACTIVEXCTL = 1234,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.INVERTCOLORS"]/*' />
            INVERTCOLORS = 1235,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.FLIPHORIZONTAL"]/*' />
            FLIPHORIZONTAL = 1236,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.FLIPVERTICAL"]/*' />
            FLIPVERTICAL = 1237,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ROTATE90"]/*' />
            ROTATE90 = 1238,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SHOWCOLORSWINDOW"]/*' />
            SHOWCOLORSWINDOW = 1239,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NEWSTRING"]/*' />
            NEWSTRING = 1240,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NEWINFOBLOCK"]/*' />
            NEWINFOBLOCK = 1241,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DELETEINFOBLOCK"]/*' />
            DELETEINFOBLOCK = 1242,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ADJUSTCOLORS"]/*' />
            ADJUSTCOLORS = 1243,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.LOADPALETTE"]/*' />
            LOADPALETTE = 1244,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SAVEPALETTE"]/*' />
            SAVEPALETTE = 1245,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CHECKMNEMONICS"]/*' />
            CHECKMNEMONICS = 1246,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DRAWOPAQUE"]/*' />
            DRAWOPAQUE = 1247,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.TOOLBAREDITOR"]/*' />
            TOOLBAREDITOR = 1248,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.GRIDSETTINGS"]/*' />
            GRIDSETTINGS = 1249,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NEWDEVICEIMAGE"]/*' />
            NEWDEVICEIMAGE = 1250,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.OPENDEVICEIMAGE"]/*' />
            OPENDEVICEIMAGE = 1251,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DELETEDEVICEIMAGE"]/*' />
            DELETEDEVICEIMAGE = 1252,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.VIEWASPOPUP"]/*' />
            VIEWASPOPUP = 1253,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CHECKMENUMNEMONICS"]/*' />
            CHECKMENUMNEMONICS = 1254,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SHOWIMAGEGRID"]/*' />
            SHOWIMAGEGRID = 1255,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SHOWTILEGRID"]/*' />
            SHOWTILEGRID = 1256,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.MAGNIFY"]/*' />
            MAGNIFY = 1257,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ResProps"]/*' />
            ResProps = 1258,
            //
            // Shareable commands from the VC resource editor (Image editor toolbar)
            //
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.PICKRECTANGLE"]/*' />
            PICKRECTANGLE = 1300,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.PICKREGION"]/*' />
            PICKREGION = 1301,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.PICKCOLOR"]/*' />
            PICKCOLOR = 1302,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ERASERTOOL"]/*' />
            ERASERTOOL = 1303,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.FILLTOOL"]/*' />
            FILLTOOL = 1304,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.PENCILTOOL"]/*' />
            PENCILTOOL = 1305,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.BRUSHTOOL"]/*' />
            BRUSHTOOL = 1306,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.AIRBRUSHTOOL"]/*' />
            AIRBRUSHTOOL = 1307,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.LINETOOL"]/*' />
            LINETOOL = 1308,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CURVETOOL"]/*' />
            CURVETOOL = 1309,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.TEXTTOOL"]/*' />
            TEXTTOOL = 1310,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.RECTTOOL"]/*' />
            RECTTOOL = 1311,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.OUTLINERECTTOOL"]/*' />
            OUTLINERECTTOOL = 1312,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.FILLEDRECTTOOL"]/*' />
            FILLEDRECTTOOL = 1313,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ROUNDRECTTOOL"]/*' />
            ROUNDRECTTOOL = 1314,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.OUTLINEROUNDRECTTOOL"]/*' />
            OUTLINEROUNDRECTTOOL = 1315,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.FILLEDROUNDRECTTOOL"]/*' />
            FILLEDROUNDRECTTOOL = 1316,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ELLIPSETOOL"]/*' />
            ELLIPSETOOL = 1317,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.OUTLINEELLIPSETOOL"]/*' />
            OUTLINEELLIPSETOOL = 1318,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.FILLEDELLIPSETOOL"]/*' />
            FILLEDELLIPSETOOL = 1319,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SETHOTSPOT"]/*' />
            SETHOTSPOT = 1320,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ZOOMTOOL"]/*' />
            ZOOMTOOL = 1321,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ZOOM1X"]/*' />
            ZOOM1X = 1322,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ZOOM2X"]/*' />
            ZOOM2X = 1323,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ZOOM6X"]/*' />
            ZOOM6X = 1324,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ZOOM8X"]/*' />
            ZOOM8X = 1325,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.TRANSPARENTBCKGRND"]/*' />
            TRANSPARENTBCKGRND = 1326,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.OPAQUEBCKGRND"]/*' />
            OPAQUEBCKGRND = 1327,
            //---------------------------------------------------
            // The commands ECMD_ERASERSMALL thru ECMD_LINELARGER
            // must be left in the same order for the use of the
            // Resource Editor - They may however be relocated as
            // a complete block
            //---------------------------------------------------
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ERASERSMALL"]/*' />
            ERASERSMALL = 1328,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ERASERMEDIUM"]/*' />
            ERASERMEDIUM = 1329,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ERASERLARGE"]/*' />
            ERASERLARGE = 1330,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ERASERLARGER"]/*' />
            ERASERLARGER = 1331,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CIRCLELARGE"]/*' />
            CIRCLELARGE = 1332,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CIRCLEMEDIUM"]/*' />
            CIRCLEMEDIUM = 1333,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CIRCLESMALL"]/*' />
            CIRCLESMALL = 1334,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SQUARELARGE"]/*' />
            SQUARELARGE = 1335,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SQUAREMEDIUM"]/*' />
            SQUAREMEDIUM = 1336,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SQUARESMALL"]/*' />
            SQUARESMALL = 1337,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.LEFTDIAGLARGE"]/*' />
            LEFTDIAGLARGE = 1338,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.LEFTDIAGMEDIUM"]/*' />
            LEFTDIAGMEDIUM = 1339,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.LEFTDIAGSMALL"]/*' />
            LEFTDIAGSMALL = 1340,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.RIGHTDIAGLARGE"]/*' />
            RIGHTDIAGLARGE = 1341,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.RIGHTDIAGMEDIUM"]/*' />
            RIGHTDIAGMEDIUM = 1342,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.RIGHTDIAGSMALL"]/*' />
            RIGHTDIAGSMALL = 1343,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SPLASHSMALL"]/*' />
            SPLASHSMALL = 1344,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SPLASHMEDIUM"]/*' />
            SPLASHMEDIUM = 1345,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SPLASHLARGE"]/*' />
            SPLASHLARGE = 1346,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.LINESMALLER"]/*' />
            LINESMALLER = 1347,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.LINESMALL"]/*' />
            LINESMALL = 1348,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.LINEMEDIUM"]/*' />
            LINEMEDIUM = 1349,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.LINELARGE"]/*' />
            LINELARGE = 1350,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.LINELARGER"]/*' />
            LINELARGER = 1351,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.LARGERBRUSH"]/*' />
            LARGERBRUSH = 1352,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.LARGEBRUSH"]/*' />
            LARGEBRUSH = 1353,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.STDBRUSH"]/*' />
            STDBRUSH = 1354,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SMALLBRUSH"]/*' />
            SMALLBRUSH = 1355,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SMALLERBRUSH"]/*' />
            SMALLERBRUSH = 1356,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ZOOMIN"]/*' />
            ZOOMIN = 1357,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ZOOMOUT"]/*' />
            ZOOMOUT = 1358,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.PREVCOLOR"]/*' />
            PREVCOLOR = 1359,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.PREVECOLOR"]/*' />
            PREVECOLOR = 1360,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NEXTCOLOR"]/*' />
            NEXTCOLOR = 1361,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NEXTECOLOR"]/*' />
            NEXTECOLOR = 1362,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.IMG_OPTIONS"]/*' />
            IMG_OPTIONS = 1363,

            //
            // Sharable Commands from Visual Web Developer (website projects)
            //
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.STARTWEBADMINTOOL"]/*' />
            STARTWEBADMINTOOL = 1400,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NESTRELATEDFILES"]/*' />
            NESTRELATEDFILES = 1401,

            //---------------------------------------------------

            //
            // Shareable commands from WINFORMS
            //
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CANCELDRAG"]/*' />
            CANCELDRAG = 1500,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DEFAULTACTION"]/*' />
            DEFAULTACTION = 1501,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CTLMOVEUPGRID"]/*' />
            CTLMOVEUPGRID = 1502,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CTLMOVEDOWNGRID"]/*' />
            CTLMOVEDOWNGRID = 1503,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CTLMOVELEFTGRID"]/*' />
            CTLMOVELEFTGRID = 1504,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CTLMOVERIGHTGRID"]/*' />
            CTLMOVERIGHTGRID = 1505,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CTLSIZERIGHTGRID"]/*' />
            CTLSIZERIGHTGRID = 1506,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CTLSIZEUPGRID"]/*' />
            CTLSIZEUPGRID = 1507,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CTLSIZELEFTGRID"]/*' />
            CTLSIZELEFTGRID = 1508,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CTLSIZEDOWNGRID"]/*' />
            CTLSIZEDOWNGRID = 1509,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NEXTCTL"]/*' />
            NEXTCTL = 1510,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.PREVCTL"]/*' />
            PREVCTL = 1511,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.RENAME"]/*' />
            RENAME = 1550,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.EXTRACTMETHOD"]/*' />
            EXTRACTMETHOD = 1551,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ENCAPSULATEFIELD"]/*' />
            ENCAPSULATEFIELD = 1552,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.EXTRACTINTERFACE"]/*' />
            EXTRACTINTERFACE = 1553,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.PROMOTELOCAL"]/*' />
            PROMOTELOCAL = 1554,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.REMOVEPARAMETERS"]/*' />
            REMOVEPARAMETERS = 1555,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.REORDERPARAMETERS"]/*' />
            REORDERPARAMETERS = 1556,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.GENERATEMETHODSTUB"]/*' />
            GENERATEMETHODSTUB = 1557,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.IMPLEMENTINTERFACEIMPLICIT"]/*' />
            IMPLEMENTINTERFACEIMPLICIT = 1558,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.IMPLEMENTINTERFACEEXPLICIT"]/*' />
            IMPLEMENTINTERFACEEXPLICIT = 1559,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.IMPLEMENTABSTRACTCLASS"]/*' />
            IMPLEMENTABSTRACTCLASS = 1560,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SURROUNDWITH"]/*' />
            SURROUNDWITH = 1561,

            // this is coming in with the VS2K guid?
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.QUICKOBJECTSEARCH"]/*' />
            QUICKOBJECTSEARCH = 1119,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ToggleWordWrapOW"]/*' />
            ToggleWordWrapOW = 1600,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.GotoNextLocationOW"]/*' />
            GotoNextLocationOW = 1601,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.GotoPrevLocationOW"]/*' />
            GotoPrevLocationOW = 1602,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.BuildOnlyProject"]/*' />
            BuildOnlyProject = 1603,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.RebuildOnlyProject"]/*' />
            RebuildOnlyProject = 1604,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CleanOnlyProject"]/*' />
            CleanOnlyProject = 1605,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SetBuildStartupsOnlyOnRun"]/*' />
            SetBuildStartupsOnlyOnRun = 1606,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.UnhideAll"]/*' />
            UnhideAll = 1607,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.HideFolder"]/*' />
            HideFolder = 1608,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.UnhideFolders"]/*' />
            UnhideFolders = 1609,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CopyFullPathName"]/*' />
            CopyFullPathName = 1610,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SaveFolderAsSolution"]/*' />
            SaveFolderAsSolution = 1611,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ManageUserSettings"]/*' />
            ManageUserSettings = 1612,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NewSolutionFolder"]/*' />
            NewSolutionFolder = 1613,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ClearPaneOW"]/*' />
            ClearPaneOW = 1615,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.GotoErrorTagOW"]/*' />
            GotoErrorTagOW = 1616,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.GotoNextErrorTagOW"]/*' />
            GotoNextErrorTagOW = 1617,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.GotoPrevErrorTagOW"]/*' />
            GotoPrevErrorTagOW = 1618,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ClearPaneFR1"]/*' />
            ClearPaneFR1 = 1619,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.GotoErrorTagFR1"]/*' />
            GotoErrorTagFR1 = 1620,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.GotoNextErrorTagFR1"]/*' />
            GotoNextErrorTagFR1 = 1621,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.GotoPrevErrorTagFR1"]/*' />
            GotoPrevErrorTagFR1 = 1622,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ClearPaneFR2"]/*' />
            ClearPaneFR2 = 1623,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.GotoErrorTagFR2"]/*' />
            GotoErrorTagFR2 = 1624,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.GotoNextErrorTagFR2"]/*' />
            GotoNextErrorTagFR2 = 1625,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.GotoPrevErrorTagFR2"]/*' />
            GotoPrevErrorTagFR2 = 1626,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.OutputPaneCombo"]/*' />
            OutputPaneCombo = 1627,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.OutputPaneComboList"]/*' />
            OutputPaneComboList = 1628,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DisableDockingChanges"]/*' />
            DisableDockingChanges = 1629,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ToggleFloat"]/*' />
            ToggleFloat = 1630,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ResetLayout"]/*' />
            ResetLayout = 1631,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NewSolutionFolderBar"]/*' />
            NewSolutionFolderBar = 1638,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DataShortcut"]/*' />
            DataShortcut = 1639,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NextToolWindow"]/*' />
            NextToolWindow = 1640,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.PrevToolWindow"]/*' />
            PrevToolWindow = 1641,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.BrowseToFileInExplorer"]/*' />
            BrowseToFileInExplorer = 1642,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ShowEzMDIFileMenu"]/*' />
            ShowEzMDIFileMenu = 1643,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.PrevToolWindowNav"]/*' />
            PrevToolWindowNav = 1645,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.StaticAnalysisOnlyProject"]/*' />
            StaticAnalysisOnlyProject = 1646,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_RUNFXCOPSEL"]/*' />
            ECMD_RUNFXCOPSEL = 1647,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CloseAllButThis"]/*' />
            CloseAllButThis = 1650,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ControlGallery"]/*' />
            ControlGallery = 1700,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.FullScreen2"]/*' />
            FullScreen2 = 1775,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NavigateBack"]/*' />
            NavigateBack = 1800,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NavigateForward"]/*' />
            NavigateForward = 1801,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_CORRECTION_1"]/*' />
            ECMD_CORRECTION_1 = 1900,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_CORRECTION_2"]/*' />
            ECMD_CORRECTION_2 = 1901,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_CORRECTION_3"]/*' />
            ECMD_CORRECTION_3 = 1902,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_CORRECTION_4"]/*' />
            ECMD_CORRECTION_4 = 1903,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_CORRECTION_5"]/*' />
            ECMD_CORRECTION_5 = 1904,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_CORRECTION_6"]/*' />
            ECMD_CORRECTION_6 = 1905,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_CORRECTION_7"]/*' />
            ECMD_CORRECTION_7 = 1906,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_CORRECTION_8"]/*' />
            ECMD_CORRECTION_8 = 1907,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_CORRECTION_9"]/*' />
            ECMD_CORRECTION_9 = 1908,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_CORRECTION_10"]/*' />
            ECMD_CORRECTION_10 = 1909,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.OBAddReference"]/*' />
            OBAddReference = 1914,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.FindReferences"]/*' />
            FindReferences = 1915,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CodeDefView"]/*' />
            CodeDefView = 1926,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CodeDefViewGoToPrev"]/*' />
            CodeDefViewGoToPrev = 1927,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CodeDefViewGoToNext"]/*' />
            CodeDefViewGoToNext = 1928,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CodeDefViewEditDefinition"]/*' />
            CodeDefViewEditDefinition = 1929,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CodeDefViewChooseEncoding"]/*' />
            CodeDefViewChooseEncoding = 1930,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ViewInClassDiagram"]/*' />
            ViewInClassDiagram = 1931,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_ADDDBTABLE"]/*' />
            ECMD_ADDDBTABLE = 1950,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_ADDDATATABLE"]/*' />
            ECMD_ADDDATATABLE = 1951,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_ADDFUNCTION"]/*' />
            ECMD_ADDFUNCTION = 1952,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_ADDRELATION"]/*' />
            ECMD_ADDRELATION = 1953,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_ADDKEY"]/*' />
            ECMD_ADDKEY = 1954,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_ADDCOLUMN"]/*' />
            ECMD_ADDCOLUMN = 1955,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_CONVERT_DBTABLE"]/*' />
            ECMD_CONVERT_DBTABLE = 1956,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_CONVERT_DATATABLE"]/*' />
            ECMD_CONVERT_DATATABLE = 1957,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_GENERATE_DATABASE"]/*' />
            ECMD_GENERATE_DATABASE = 1958,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_CONFIGURE_CONNECTIONS"]/*' />
            ECMD_CONFIGURE_CONNECTIONS = 1959,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_IMPORT_XMLSCHEMA"]/*' />
            ECMD_IMPORT_XMLSCHEMA = 1960,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_SYNC_WITH_DATABASE"]/*' />
            ECMD_SYNC_WITH_DATABASE = 1961,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_CONFIGURE"]/*' />
            ECMD_CONFIGURE = 1962,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_CREATE_DATAFORM"]/*' />
            ECMD_CREATE_DATAFORM = 1963,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_CREATE_ENUM"]/*' />
            ECMD_CREATE_ENUM = 1964,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_INSERT_FUNCTION"]/*' />
            ECMD_INSERT_FUNCTION = 1965,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_EDIT_FUNCTION"]/*' />
            ECMD_EDIT_FUNCTION = 1966,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_SET_PRIMARY_KEY"]/*' />
            ECMD_SET_PRIMARY_KEY = 1967,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_INSERT_COLUMN"]/*' />
            ECMD_INSERT_COLUMN = 1968,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_AUTO_SIZE"]/*' />
            ECMD_AUTO_SIZE = 1969,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_SHOW_RELATION_LABELS"]/*' />
            ECMD_SHOW_RELATION_LABELS = 1970,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.VSDGenerateDataSet"]/*' />
            VSDGenerateDataSet = 1971,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.VSDPreview"]/*' />
            VSDPreview = 1972,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.VSDConfigureAdapter"]/*' />
            VSDConfigureAdapter = 1973,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.VSDViewDatasetSchema"]/*' />
            VSDViewDatasetSchema = 1974,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.VSDDatasetProperties"]/*' />
            VSDDatasetProperties = 1975,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.VSDParameterizeForm"]/*' />
            VSDParameterizeForm = 1976,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.VSDAddChildForm"]/*' />
            VSDAddChildForm = 1977,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_EDITCONSTRAINT"]/*' />
            ECMD_EDITCONSTRAINT = 1978,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_DELETECONSTRAINT"]/*' />
            ECMD_DELETECONSTRAINT = 1979,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_EDITDATARELATION"]/*' />
            ECMD_EDITDATARELATION = 1980,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CloseProject"]/*' />
            CloseProject = 1982,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ReloadCommandBars"]/*' />
            ReloadCommandBars = 1983,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SolutionPlatform"]/*' />
            SolutionPlatform = 1990,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SolutionPlatformGetList"]/*' />
            SolutionPlatformGetList = 1991,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_DATAACCESSOR"]/*' />
            ECMD_DATAACCESSOR = 2000,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_ADD_DATAACCESSOR"]/*' />
            ECMD_ADD_DATAACCESSOR = 2001,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_QUERY"]/*' />
            ECMD_QUERY = 2002,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_ADD_QUERY"]/*' />
            ECMD_ADD_QUERY = 2003,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_PUBLISHSELECTION"]/*' />
            ECMD_PUBLISHSELECTION = 2005,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_PUBLISHSLNCTX"]/*' />
            ECMD_PUBLISHSLNCTX = 2006,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowserShowCallsTo"]/*' />
            CallBrowserShowCallsTo = 2010,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowserShowCallsFrom"]/*' />
            CallBrowserShowCallsFrom = 2011,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowserShowNewCallsTo"]/*' />
            CallBrowserShowNewCallsTo = 2012,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowserShowNewCallsFrom"]/*' />
            CallBrowserShowNewCallsFrom = 2013,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser1ShowCallsTo"]/*' />
            CallBrowser1ShowCallsTo = 2014,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser2ShowCallsTo"]/*' />
            CallBrowser2ShowCallsTo = 2015,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser3ShowCallsTo"]/*' />
            CallBrowser3ShowCallsTo = 2016,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser4ShowCallsTo"]/*' />
            CallBrowser4ShowCallsTo = 2017,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser5ShowCallsTo"]/*' />
            CallBrowser5ShowCallsTo = 2018,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser6ShowCallsTo"]/*' />
            CallBrowser6ShowCallsTo = 2019,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser7ShowCallsTo"]/*' />
            CallBrowser7ShowCallsTo = 2020,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser8ShowCallsTo"]/*' />
            CallBrowser8ShowCallsTo = 2021,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser9ShowCallsTo"]/*' />
            CallBrowser9ShowCallsTo = 2022,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser10ShowCallsTo"]/*' />
            CallBrowser10ShowCallsTo = 2023,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser11ShowCallsTo"]/*' />
            CallBrowser11ShowCallsTo = 2024,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser12ShowCallsTo"]/*' />
            CallBrowser12ShowCallsTo = 2025,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser13ShowCallsTo"]/*' />
            CallBrowser13ShowCallsTo = 2026,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser14ShowCallsTo"]/*' />
            CallBrowser14ShowCallsTo = 2027,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser15ShowCallsTo"]/*' />
            CallBrowser15ShowCallsTo = 2028,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser16ShowCallsTo"]/*' />
            CallBrowser16ShowCallsTo = 2029,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser1ShowCallsFrom"]/*' />
            CallBrowser1ShowCallsFrom = 2030,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser2ShowCallsFrom"]/*' />
            CallBrowser2ShowCallsFrom = 2031,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser3ShowCallsFrom"]/*' />
            CallBrowser3ShowCallsFrom = 2032,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser4ShowCallsFrom"]/*' />
            CallBrowser4ShowCallsFrom = 2033,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser5ShowCallsFrom"]/*' />
            CallBrowser5ShowCallsFrom = 2034,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser6ShowCallsFrom"]/*' />
            CallBrowser6ShowCallsFrom = 2035,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser7ShowCallsFrom"]/*' />
            CallBrowser7ShowCallsFrom = 2036,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser8ShowCallsFrom"]/*' />
            CallBrowser8ShowCallsFrom = 2037,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser9ShowCallsFrom"]/*' />
            CallBrowser9ShowCallsFrom = 2038,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser10ShowCallsFrom"]/*' />
            CallBrowser10ShowCallsFrom = 2039,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser11ShowCallsFrom"]/*' />
            CallBrowser11ShowCallsFrom = 2040,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser12ShowCallsFrom"]/*' />
            CallBrowser12ShowCallsFrom = 2041,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser13ShowCallsFrom"]/*' />
            CallBrowser13ShowCallsFrom = 2042,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser14ShowCallsFrom"]/*' />
            CallBrowser14ShowCallsFrom = 2043,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser15ShowCallsFrom"]/*' />
            CallBrowser15ShowCallsFrom = 2044,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser16ShowCallsFrom"]/*' />
            CallBrowser16ShowCallsFrom = 2045,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser1ShowFullNames"]/*' />
            CallBrowser1ShowFullNames = 2046,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser2ShowFullNames"]/*' />
            CallBrowser2ShowFullNames = 2047,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser3ShowFullNames"]/*' />
            CallBrowser3ShowFullNames = 2048,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser4ShowFullNames"]/*' />
            CallBrowser4ShowFullNames = 2049,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser5ShowFullNames"]/*' />
            CallBrowser5ShowFullNames = 2050,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser6ShowFullNames"]/*' />
            CallBrowser6ShowFullNames = 2051,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser7ShowFullNames"]/*' />
            CallBrowser7ShowFullNames = 2052,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser8ShowFullNames"]/*' />
            CallBrowser8ShowFullNames = 2053,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser9ShowFullNames"]/*' />
            CallBrowser9ShowFullNames = 2054,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser10ShowFullNames"]/*' />
            CallBrowser10ShowFullNames = 2055,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser11ShowFullNames"]/*' />
            CallBrowser11ShowFullNames = 2056,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser12ShowFullNames"]/*' />
            CallBrowser12ShowFullNames = 2057,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser13ShowFullNames"]/*' />
            CallBrowser13ShowFullNames = 2058,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser14ShowFullNames"]/*' />
            CallBrowser14ShowFullNames = 2059,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser15ShowFullNames"]/*' />
            CallBrowser15ShowFullNames = 2060,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser16ShowFullNames"]/*' />
            CallBrowser16ShowFullNames = 2061,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser1Settings"]/*' />
            CallBrowser1Settings = 2062,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser2Settings"]/*' />
            CallBrowser2Settings = 2063,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser3Settings"]/*' />
            CallBrowser3Settings = 2064,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser4Settings"]/*' />
            CallBrowser4Settings = 2065,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser5Settings"]/*' />
            CallBrowser5Settings = 2066,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser6Settings"]/*' />
            CallBrowser6Settings = 2067,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser7Settings"]/*' />
            CallBrowser7Settings = 2068,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser8Settings"]/*' />
            CallBrowser8Settings = 2069,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser9Settings"]/*' />
            CallBrowser9Settings = 2070,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser10Settings"]/*' />
            CallBrowser10Settings = 2071,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser11Settings"]/*' />
            CallBrowser11Settings = 2072,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser12Settings"]/*' />
            CallBrowser12Settings = 2073,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser13Settings"]/*' />
            CallBrowser13Settings = 2074,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser14Settings"]/*' />
            CallBrowser14Settings = 2075,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser15Settings"]/*' />
            CallBrowser15Settings = 2076,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser16Settings"]/*' />
            CallBrowser16Settings = 2077,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser1SortAlpha"]/*' />
            CallBrowser1SortAlpha = 2078,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser2SortAlpha"]/*' />
            CallBrowser2SortAlpha = 2079,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser3SortAlpha"]/*' />
            CallBrowser3SortAlpha = 2080,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser4SortAlpha"]/*' />
            CallBrowser4SortAlpha = 2081,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser5SortAlpha"]/*' />
            CallBrowser5SortAlpha = 2082,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser6SortAlpha"]/*' />
            CallBrowser6SortAlpha = 2083,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser7SortAlpha"]/*' />
            CallBrowser7SortAlpha = 2084,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser8SortAlpha"]/*' />
            CallBrowser8SortAlpha = 2085,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser9SortAlpha"]/*' />
            CallBrowser9SortAlpha = 2086,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser10SortAlpha"]/*' />
            CallBrowser10SortAlpha = 2087,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser11SortAlpha"]/*' />
            CallBrowser11SortAlpha = 2088,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser12SortAlpha"]/*' />
            CallBrowser12SortAlpha = 2089,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser13SortAlpha"]/*' />
            CallBrowser13SortAlpha = 2090,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser14SortAlpha"]/*' />
            CallBrowser14SortAlpha = 2091,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser15SortAlpha"]/*' />
            CallBrowser15SortAlpha = 2092,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser16SortAlpha"]/*' />
            CallBrowser16SortAlpha = 2093,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser1SortAccess"]/*' />
            CallBrowser1SortAccess = 2094,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser2SortAccess"]/*' />
            CallBrowser2SortAccess = 2095,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser3SortAccess"]/*' />
            CallBrowser3SortAccess = 2096,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser4SortAccess"]/*' />
            CallBrowser4SortAccess = 2097,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser5SortAccess"]/*' />
            CallBrowser5SortAccess = 2098,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser6SortAccess"]/*' />
            CallBrowser6SortAccess = 2099,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser7SortAccess"]/*' />
            CallBrowser7SortAccess = 2100,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser8SortAccess"]/*' />
            CallBrowser8SortAccess = 2101,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser9SortAccess"]/*' />
            CallBrowser9SortAccess = 2102,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser10SortAccess"]/*' />
            CallBrowser10SortAccess = 2103,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser11SortAccess"]/*' />
            CallBrowser11SortAccess = 2104,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser12SortAccess"]/*' />
            CallBrowser12SortAccess = 2105,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser13SortAccess"]/*' />
            CallBrowser13SortAccess = 2106,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser14SortAccess"]/*' />
            CallBrowser14SortAccess = 2107,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser15SortAccess"]/*' />
            CallBrowser15SortAccess = 2108,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser16SortAccess"]/*' />
            CallBrowser16SortAccess = 2109,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ShowCallBrowser"]/*' />
            ShowCallBrowser = 2120,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser1"]/*' />
            CallBrowser1 = 2121,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser2"]/*' />
            CallBrowser2 = 2122,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser3"]/*' />
            CallBrowser3 = 2123,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser4"]/*' />
            CallBrowser4 = 2124,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser5"]/*' />
            CallBrowser5 = 2125,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser6"]/*' />
            CallBrowser6 = 2126,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser7"]/*' />
            CallBrowser7 = 2127,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser8"]/*' />
            CallBrowser8 = 2128,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser9"]/*' />
            CallBrowser9 = 2129,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser10"]/*' />
            CallBrowser10 = 2130,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser11"]/*' />
            CallBrowser11 = 2131,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser12"]/*' />
            CallBrowser12 = 2132,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser13"]/*' />
            CallBrowser13 = 2133,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser14"]/*' />
            CallBrowser14 = 2134,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser15"]/*' />
            CallBrowser15 = 2135,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser16"]/*' />
            CallBrowser16 = 2136,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser17"]/*' />
            CallBrowser17 = 2137,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.GlobalUndo"]/*' />
            GlobalUndo = 2138,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.GlobalRedo"]/*' />
            GlobalRedo = 2139,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowserShowCallsToCmd"]/*' />
            CallBrowserShowCallsToCmd = 2140,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowserShowCallsFromCmd"]/*' />
            CallBrowserShowCallsFromCmd = 2141,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowserShowNewCallsToCmd"]/*' />
            CallBrowserShowNewCallsToCmd = 2142,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowserShowNewCallsFromCmd"]/*' />
            CallBrowserShowNewCallsFromCmd = 2143,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser1Search"]/*' />
            CallBrowser1Search = 2145,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser2Search"]/*' />
            CallBrowser2Search = 2146,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser3Search"]/*' />
            CallBrowser3Search = 2147,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser4Search"]/*' />
            CallBrowser4Search = 2148,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser5Search"]/*' />
            CallBrowser5Search = 2149,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser6Search"]/*' />
            CallBrowser6Search = 2150,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser7Search"]/*' />
            CallBrowser7Search = 2151,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser8Search"]/*' />
            CallBrowser8Search = 2152,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser9Search"]/*' />
            CallBrowser9Search = 2153,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser10Search"]/*' />
            CallBrowser10Search = 2154,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser11Search"]/*' />
            CallBrowser11Search = 2155,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser12Search"]/*' />
            CallBrowser12Search = 2156,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser13Search"]/*' />
            CallBrowser13Search = 2157,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser14Search"]/*' />
            CallBrowser14Search = 2158,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser15Search"]/*' />
            CallBrowser15Search = 2159,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser16Search"]/*' />
            CallBrowser16Search = 2160,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser1Refresh"]/*' />
            CallBrowser1Refresh = 2161,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser2Refresh"]/*' />
            CallBrowser2Refresh = 2162,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser3Refresh"]/*' />
            CallBrowser3Refresh = 2163,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser4Refresh"]/*' />
            CallBrowser4Refresh = 2164,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser5Refresh"]/*' />
            CallBrowser5Refresh = 2165,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser6Refresh"]/*' />
            CallBrowser6Refresh = 2166,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser7Refresh"]/*' />
            CallBrowser7Refresh = 2167,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser8Refresh"]/*' />
            CallBrowser8Refresh = 2168,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser9Refresh"]/*' />
            CallBrowser9Refresh = 2169,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser10Refresh"]/*' />
            CallBrowser10Refresh = 2170,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser11Refresh"]/*' />
            CallBrowser11Refresh = 2171,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser12Refresh"]/*' />
            CallBrowser12Refresh = 2172,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser13Refresh"]/*' />
            CallBrowser13Refresh = 2173,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser14Refresh"]/*' />
            CallBrowser14Refresh = 2174,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser15Refresh"]/*' />
            CallBrowser15Refresh = 2175,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser16Refresh"]/*' />
            CallBrowser16Refresh = 2176,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser1SearchCombo"]/*' />
            CallBrowser1SearchCombo = 2180,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser2SearchCombo"]/*' />
            CallBrowser2SearchCombo = 2181,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser3SearchCombo"]/*' />
            CallBrowser3SearchCombo = 2182,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser4SearchCombo"]/*' />
            CallBrowser4SearchCombo = 2183,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser5SearchCombo"]/*' />
            CallBrowser5SearchCombo = 2184,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser6SearchCombo"]/*' />
            CallBrowser6SearchCombo = 2185,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser7SearchCombo"]/*' />
            CallBrowser7SearchCombo = 2186,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser8SearchCombo"]/*' />
            CallBrowser8SearchCombo = 2187,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser9SearchCombo"]/*' />
            CallBrowser9SearchCombo = 2188,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser10SearchCombo"]/*' />
            CallBrowser10SearchCombo = 2189,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser11SearchCombo"]/*' />
            CallBrowser11SearchCombo = 2190,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser12SearchCombo"]/*' />
            CallBrowser12SearchCombo = 2191,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser13SearchCombo"]/*' />
            CallBrowser13SearchCombo = 2192,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser14SearchCombo"]/*' />
            CallBrowser14SearchCombo = 2193,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser15SearchCombo"]/*' />
            CallBrowser15SearchCombo = 2194,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CallBrowser16SearchCombo"]/*' />
            CallBrowser16SearchCombo = 2195,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.TaskListProviderCombo"]/*' />
            TaskListProviderCombo = 2200,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.TaskListProviderComboList"]/*' />
            TaskListProviderComboList = 2201,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CreateUserTask"]/*' />
            CreateUserTask = 2202,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ErrorListShowErrors"]/*' />
            ErrorListShowErrors = 2210,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ErrorListShowWarnings"]/*' />
            ErrorListShowWarnings = 2211,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ErrorListShowMessages"]/*' />
            ErrorListShowMessages = 2212,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.Registration"]/*' />
            Registration = 2214,



            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SnippetProp"]/*' />
            SnippetProp = 2240,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SnippetRef"]/*' />
            SnippetRef = 2241,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SnippetRepl"]/*' />
            SnippetRepl = 2242,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.StartPage"]/*' />
            StartPage = 2245,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.EditorLineFirstColumn"]/*' />
            EditorLineFirstColumn = 2250,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.EditorLineFirstColumnExtend"]/*' />
            EditorLineFirstColumnExtend = 2251,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SEServerExplorer"]/*' />
            SEServerExplorer = 2260,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SEDataExplorer"]/*' />
            SEDataExplorer = 2261,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_UPDATEMGDRES"]/*' />
            ECMD_UPDATEMGDRES = 358,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_PROJTOOLORDER"]/*' />
            ECMD_PROJTOOLORDER = 367,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_PROJECTTOOLFILES"]/*' />
            ECMD_PROJECTTOOLFILES = 368,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_OTB_PGO_INSTRUMENT"]/*' />
            ECMD_OTB_PGO_INSTRUMENT = 369,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_OTB_PGO_OPT"]/*' />
            ECMD_OTB_PGO_OPT = 370,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_OTB_PGO_UPDATE"]/*' />
            ECMD_OTB_PGO_UPDATE = 371,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_OTB_PGO_RUNSCENARIO"]/*' />
            ECMD_OTB_PGO_RUNSCENARIO = 372,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_TAB"]/*' />
            ECMD_TAB = 4,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_ADDMASTERPAGE"]/*' />
            ECMD_ADDMASTERPAGE = 411,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_ADDCONTENTPAGE"]/*' />
            ECMD_ADDCONTENTPAGE = 413,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_ADDSTYLESHEET"]/*' />
            ECMD_ADDSTYLESHEET = 445,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_SETBROWSELOCATION"]/*' />
            ECMD_SETBROWSELOCATION = 446,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_REFRESHFOLDER"]/*' />
            ECMD_REFRESHFOLDER = 447,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_SETBROWSELOCATIONCTX"]/*' />
            ECMD_SETBROWSELOCATIONCTX = 448,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_VIEWMARKUP"]/*' />
            ECMD_VIEWMARKUP = 449,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_NEXTMETHOD"]/*' />
            ECMD_NEXTMETHOD = 450,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_PREVMETHOD"]/*' />
            ECMD_PREVMETHOD = 451,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_RENAMESYMBOL"]/*' />
            ECMD_RENAMESYMBOL = 452,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_SHOWREFERENCES"]/*' />
            ECMD_SHOWREFERENCES = 453,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_CREATESNIPPET"]/*' />
            ECMD_CREATESNIPPET = 454,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_CREATEREPLACEMENT"]/*' />
            ECMD_CREATEREPLACEMENT = 455,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_INSERTCOMMENT"]/*' />
            ECMD_INSERTCOMMENT = 456,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_VALIDATION_TARGET"]/*' />
            ECMD_VALIDATION_TARGET = 11281,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_VALIDATION_TARGET_GET_LIST"]/*' />
            ECMD_VALIDATION_TARGET_GET_LIST = 11282,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_CSS_TARGET"]/*' />
            ECMD_CSS_TARGET = 11283,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ECMD_CSS_TARGET_GET_LIST"]/*' />
            ECMD_CSS_TARGET_GET_LIST = 11284,

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NewDiagram"]/*' />
            NewDiagram = 0x3004,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NewTable"]/*' />
            NewTable = 0x3006,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NewDBItem"]/*' />
            NewDBItem = 0x300E,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NewTrigger"]/*' />
            NewTrigger = 0x3010,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.Debug"]/*' />
            Debug = 0x3012,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NewProcedure"]/*' />
            NewProcedure = 0x3013,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NewQuery"]/*' />
            NewQuery = 0x3014,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.RefreshLocal"]/*' />
            RefreshLocal = 0x3015,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DbAddDataConnection"]/*' />
            DbAddDataConnection = 0x3017,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DBDefDBRef"]/*' />
            DBDefDBRef = 0x3018,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.RunCmd"]/*' />
            RunCmd = 0x3019,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.RunOn"]/*' />
            RunOn = 0x301A,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NewDBRef"]/*' />
            NewDBRef = 0x301B,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SetAsDef"]/*' />
            SetAsDef = 0x301C,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CreateCmdFile"]/*' />
            CreateCmdFile = 0x301D,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.Cancel"]/*' />
            Cancel = 0x301E,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NewDatabase"]/*' />
            NewDatabase = 0x3020,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NewUser"]/*' />
            NewUser = 0x3021,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NewRole"]/*' />
            NewRole = 0x3022,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ChangeLogin"]/*' />
            ChangeLogin = 0x3023,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NewView"]/*' />
            NewView = 0x3024,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ModifyConnection"]/*' />
            ModifyConnection = 0x3025,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.Disconnect"]/*' />
            Disconnect = 0x3026,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CopyScript"]/*' />
            CopyScript = 0x3027,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.AddSCC"]/*' />
            AddSCC = 0x3028,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.RemoveSCC"]/*' />
            RemoveSCC = 0x3029,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.GetLatest"]/*' />
            GetLatest = 0x3030,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CheckOut"]/*' />
            CheckOut = 0x3031,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CheckIn"]/*' />
            CheckIn = 0x3032,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.UndoCheckOut"]/*' />
            UndoCheckOut = 0x3033,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.AddItemSCC"]/*' />
            AddItemSCC = 0x3034,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NewPackageSpec"]/*' />
            NewPackageSpec = 0x3035,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NewPackageBody"]/*' />
            NewPackageBody = 0x3036,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.InsertSQL"]/*' />
            InsertSQL = 0x3037,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.RunSelection"]/*' />
            RunSelection = 0x3038,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.UpdateScript"]/*' />
            UpdateScript = 0x3039,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NewScript"]/*' />
            NewScript = 0x303C,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NewFunction"]/*' />
            NewFunction = 0x303D,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NewTableFunction"]/*' />
            NewTableFunction = 0x303E,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.NewInlineFunction"]/*' />
            NewInlineFunction = 0x303F,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.AddDiagram"]/*' />
            AddDiagram = 0x3040,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.AddTable"]/*' />
            AddTable = 0x3041,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.AddSynonym"]/*' />
            AddSynonym = 0x3042,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.AddView"]/*' />
            AddView = 0x3043,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.AddProcedure"]/*' />
            AddProcedure = 0x3044,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.AddFunction"]/*' />
            AddFunction = 0x3045,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.AddTableFunction"]/*' />
            AddTableFunction = 0x3046,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.AddInlineFunction"]/*' />
            AddInlineFunction = 0x3047,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.AddPkgSpec"]/*' />
            AddPkgSpec = 0x3048,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.AddPkgBody"]/*' />
            AddPkgBody = 0x3049,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.AddTrigger"]/*' />
            AddTrigger = 0x304A,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.ExportData"]/*' />
            ExportData = 0x304B,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DbnsVcsAdd"]/*' />
            DbnsVcsAdd = 0x304C,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DbnsVcsRemove"]/*' />
            DbnsVcsRemove = 0x304D,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DbnsVcsCheckout"]/*' />
            DbnsVcsCheckout = 0x304E,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DbnsVcsUndoCheckout"]/*' />
            DbnsVcsUndoCheckout = 0x304F,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DbnsVcsCheckin"]/*' />
            DbnsVcsCheckin = 0x3050,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SERetrieveData"]/*' />
            SERetrieveData = 0x3060,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.SEEditTextObject"]/*' />
            SEEditTextObject = 0x3061,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.DesignSQLBlock"]/*' />
            DesignSQLBlock = 0x3064,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.RegisterSQLInstance"]/*' />
            RegisterSQLInstance = 0x3065,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.UnregisterSQLInstance"]/*' />
            UnregisterSQLInstance = 0x3066,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CommandWindowSaveScript"]/*' />
            CommandWindowSaveScript = 0x3106,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CommandWindowRunScript"]/*' />
            CommandWindowRunScript = 0x3107,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CommandWindowCursorUp"]/*' />
            CommandWindowCursorUp = 0x3108,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CommandWindowCursorDown"]/*' />
            CommandWindowCursorDown = 0x3109,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CommandWindowCursorLeft"]/*' />
            CommandWindowCursorLeft = 0x310A,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CommandWindowCursorRight"]/*' />
            CommandWindowCursorRight = 0x310B,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CommandWindowHistoryUp"]/*' />
            CommandWindowHistoryUp = 0x310C,
            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="VSStd2KCmdID.CommandWindowHistoryDown"]/*' />
            CommandWindowHistoryDown = 0x310D,
        }


        /// <summary>
        /// Set of the standard, shared commands in CMDSETID.StandardCommandSet12_guid
        /// </summary>
        [Guid("2A8866DC-7BDE-4dc8-A360-A60679534384")]
        public enum VSStd12CmdID {
            ShowUserNotificationsToolWindow = 1,
            OpenProjectFromScc = 2,
            ShareProject = 3,
            PeekDefinition = 4,
            AccountSettings = 5,
            PeekNavigateForward = 6,
            PeekNavigateBackward = 7,
            RetargetProject = 8,
            RetargetProjectInstallComponent = 9,
            AddReferenceProjectOnly = 10,
            AddWebReferenceProjectOnly = 11,
            AddServiceReferenceProjectOnly = 12,
            AddReferenceNonProjectOnly = 13,
            AddWebReferenceNonProjectOnly = 14,
            AddServiceReferenceNonProjectOnly = 15,
        }

        [Guid("4C7763BF-5FAF-4264-A366-B7E1F27BA958")]
        public enum VSStd14CmdID {
            //
            ShowQuickFixes = 1,
            //
            ShowRefactorings = 2,
            //
            SmartBreakLine = 3,
            //
            ManageWindowLayouts = 4,
            //
            SaveWindowLayout = 5,
            //
            ShowQuickFixesForPosition = 6,
            //
            DeleteFR1 = 10,
            //
            DeleteFR2 = 20,
            //
            ErrorContextComboList = 30,
            //
            ErrorContextComboGetList = 31,
            //
            ErrorBuildContextComboList = 40,
            //
            ErrorBuildContextComboGetList = 41,
            ErrorListClearFilters = 50,
            //
            WindowLayoutList0 = 4096,
            //
            WindowLayoutListFirst = 4096,
            //
            WindowLayoutList1 = 4097,
            //
            WindowLayoutList2 = 4098,
            //
            WindowLayoutList3 = 4099,
            //
            WindowLayoutList4 = 4100,
            //
            WindowLayoutList5 = 4101,
            //
            WindowLayoutList6 = 4102,
            //
            WindowLayoutList7 = 4103,
            //
            WindowLayoutList8 = 4104,
            //
            WindowLayoutList9 = 4105,
            //
            WindowLayoutListDynamicFirst = 4112,
            //
            WindowLayoutListLast = 8191
        }

        internal enum DebugCommands {
            None = 0,
            InsertTracepoint = 0x00000041,
        }

        public sealed class ErrorHandler {
            private ErrorHandler() { }

            // Helper Methods

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="ErrorHandler.Succeeded"]/*' />
            /// <summary>
            /// Checks if a HRESULT is a success return code.
            /// </summary>
            /// <param name="hr">The HRESULT to test.</param>
            /// <returns>true if hr represents a success, false otherwise.</returns>
            public static bool Succeeded(int hr) => hr >= 0;

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="ErrorHandler.Failed"]/*' />
            /// <summary>
            /// Checks if a HRESULT is an error return code.
            /// </summary>
            /// <param name="hr">The HRESULT to test.</param>
            /// <returns>true if hr represents an error, false otherwise.</returns>
            public static bool Failed(int hr)=> hr < 0;

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="ErrorHandler.ThrowOnFailure"]/*' />
            /// <summary>
            /// Checks if the parameter is a success or failure HRESULT and throws an exception in case
            /// of failure.
            /// </summary>
            /// <param name="hr">The HRESULT to test.</param>
            public static int ThrowOnFailure(int hr) => ThrowOnFailure(hr, null);

            /// <include file='doc\VSConstants.uex' path='docs/doc[@for="ErrorHandler.ThrowOnFailure"]/*' />
            /// <summary>
            /// Checks if the parameter is a success or failure HRESULT and throws an exception if it is a
            /// failure that is not included in the array of well-known failures.
            /// </summary>
            /// <param name="hr">The HRESULT to test.</param>
            /// <param name="expectedHRFailure">Array of well-known and expected failures.</param>
            public static int ThrowOnFailure(int hr, params int[] expectedHRFailure) {
                if (Failed(hr)) {
                    if ((null == expectedHRFailure) || (Array.IndexOf(expectedHRFailure, hr) < 0)) {
                        Marshal.ThrowExceptionForHR(hr);
                    }
                }
                return hr;
            }
        }

        public const int S_OK = 0x00000000;
        public const int S_FALSE = 0x00000001;
        public const int E_FAIL = unchecked((int)0x80004005);
        public const int E_NOTIMPL = unchecked((int)0x80004001);

        public static bool Succeeded(int hr) => hr >= 0;
        public static bool Failed(int hr) => hr < 0;
    }
}

