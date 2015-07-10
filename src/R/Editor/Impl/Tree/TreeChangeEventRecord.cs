using Microsoft.R.Core.AST.Definitions;

namespace Microsoft.R.Editor.Tree
{
    class TreeChangeEventRecord
    {
        public TreeChangeType ChangeType { get; private set; }

        public IAstNode Node { get; private set; }

        public TreeChangeEventRecord(TreeChangeType changeType)
        {
            ChangeType = changeType;
        }

        public TreeChangeEventRecord(TreeChangeType changeType, IAstNode node) :
            this(changeType)
        {
            Node = node;
        }
    }
}
