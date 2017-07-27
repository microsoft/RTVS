// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using Microsoft.Common.Core;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal sealed class MatrixViewAutomationPeer : FrameworkElementAutomationPeer, ITableProvider {
        private readonly IDictionary<int, WeakReference<MatrixViewHeaderAutomationPeer>> _headers;
        private readonly IDictionary<(int row, int column), WeakReference<MatrixViewCellAutomationPeer>> _cells;
        private readonly MatrixView _owner;
        private GridRange _visibleRange;

        public MatrixViewAutomationPeerScrollProvider ScrollProvider { get; }
        public int RowCount => _owner.DataProvider.RowCount.ReduceToInt();
        public int ColumnCount => _owner.DataProvider.ColumnCount.ReduceToInt();

        public RowOrColumnMajor RowOrColumnMajor { get; } = RowOrColumnMajor.Indeterminate;

        public MatrixViewAutomationPeer(MatrixView owner) : base(owner) {
            _owner = owner;
            _headers = new Dictionary<int, WeakReference<MatrixViewHeaderAutomationPeer>>();
            _cells = new Dictionary<(int row, int column), WeakReference<MatrixViewCellAutomationPeer>>();
            ScrollProvider = new MatrixViewAutomationPeerScrollProvider(owner);
        }

        protected override Rect GetBoundingRectangleCore() {
            var bounds = new Rect(new Size(_owner.Points.ViewportWidth, _owner.Points.ViewportHeight));
            return new Rect(Owner.PointToScreen(bounds.TopLeft), Owner.PointToScreen(bounds.BottomRight));
        }

        public override object GetPattern(PatternInterface patternInterface) {
            switch (patternInterface) {
                case PatternInterface.Grid:
                case PatternInterface.Table:
                    return this;
                case PatternInterface.Scroll:
                    return ScrollProvider;
                default:
                    return base.GetPattern(patternInterface);
            }
        }

        public IRawElementProviderSimple GetItem(int row, int column)
            => ProviderFromPeer(GetOrCreateCell(row, column));

        public void Update() {
            var scroller = _owner.Scroller;
            var oldVisibleRange = _visibleRange;
            _visibleRange = scroller?.DataViewport ?? new GridRange(new Range(0, 0), new Range(0, 0));

            if (!oldVisibleRange.Contains(_visibleRange)) {
                ResetChildrenCache();
            }
            
            foreach (var automationPeer in GetChildren()) {
                switch (automationPeer) {
                    case MatrixViewCellAutomationPeer cellAutomationPeer when _visibleRange.Contains(cellAutomationPeer.Row, cellAutomationPeer.Column):
                        cellAutomationPeer.SetValue(scroller?.CellsData[cellAutomationPeer.Row, cellAutomationPeer.Column]);
                        break;
                    case MatrixViewHeaderAutomationPeer headerAutomationPeer when _visibleRange.Contains(_visibleRange.Rows.Start, headerAutomationPeer.Column):
                        headerAutomationPeer.SetValue(scroller?.ColumnsData[0, headerAutomationPeer.Column]);
                        break;
                    case MatrixViewItemAutomationPeer itemAutomationPeer:
                        itemAutomationPeer.ClearValue();
                        break;
                }
            }
        }

        public bool IsInVisibleRange(int row, int column) => _visibleRange.Contains(row, column);

        protected override List<AutomationPeer> GetChildrenCore() {
            var children = new List<AutomationPeer>();

            for (var row = _visibleRange.Rows.Start.ReduceToInt(); row < _visibleRange.Rows.End.ReduceToInt(); row++) {
                for (var column = _visibleRange.Columns.Start.ReduceToInt(); column < _visibleRange.Columns.End.ReduceToInt(); column++) {
                    children.Add(GetOrCreateCell(row, column));
                }
            }

            for (var column = _visibleRange.Columns.Start.ReduceToInt(); column < _visibleRange.Columns.End.ReduceToInt(); column++) {
                children.Add(GetOrCreateHeader(column));
            }

            PurgePeersCollections();

            return children;
        }
        
        public IRawElementProviderSimple[] GetRowHeaders() => null;

        public IRawElementProviderSimple[] GetColumnHeaders() {
            var start = _visibleRange.Columns.Start.ReduceToInt();
            var count = _visibleRange.Columns.End.ReduceToInt() - start;
            var headers = new IRawElementProviderSimple[count];
            for (var i = 0; i < headers.Length; i++) {
                headers[i] = ProviderFromPeer(GetOrCreateHeader(start + i));
            }
            return headers;
        }

        public MatrixViewHeaderAutomationPeer GetOrCreateHeader(int column) {
            MatrixViewHeaderAutomationPeer peer;
            if (!_headers.TryGetValue(column, out WeakReference<MatrixViewHeaderAutomationPeer> peerReference)) {
                peer = new MatrixViewHeaderAutomationPeer(_owner, column);
                peerReference = new WeakReference<MatrixViewHeaderAutomationPeer>(peer);
                _headers.Add(column, peerReference);
            } else if (!peerReference.TryGetTarget(out peer)) {
                peer = new MatrixViewHeaderAutomationPeer(_owner, column);
                peerReference.SetTarget(peer);
            }

            return peer;
        }

        public MatrixViewCellAutomationPeer GetOrCreateCell(int row, int column) {
            MatrixViewCellAutomationPeer peer;
            if (!_cells.TryGetValue((row, column), out WeakReference<MatrixViewCellAutomationPeer> peerReference)) {
                peer = new MatrixViewCellAutomationPeer(_owner, row, column);
                peerReference = new WeakReference<MatrixViewCellAutomationPeer>(peer);
                _cells.Add((row, column), peerReference);
            } else if (!peerReference.TryGetTarget(out peer)) {
                peer = new MatrixViewCellAutomationPeer(_owner, row, column);
                peerReference.SetTarget(peer);
            }

            return peer;
        }

        private void PurgePeersCollections() {
            PurgePeersCollections(_cells);
            PurgePeersCollections(_headers);
        }

        private void PurgePeersCollections<TKey, TValue>(IDictionary<TKey, WeakReference<TValue>> peers) where TValue : class 
            => peers.RemoveWhere(kvp => !kvp.Value.TryGetTarget(out _));
    }
}