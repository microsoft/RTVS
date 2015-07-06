using System;
using System.Collections.Generic;

namespace Microsoft.Languages.Editor.Controller.Command
{
    public class Command : ICommand
    {
        bool _needCheckout = false;
        CommandId[] _commandIds = null;

        public Command(Guid group, int id, bool needCheckout)
            : this(new CommandId[1] { new CommandId(group, id) }, needCheckout)
        {
        }

        public Command(CommandId id, bool needCheckout)
            : this(new CommandId[1] { id }, needCheckout)
        {
        }

        public Command(CommandId id)
            : this(id, false)
        {
        }

        public Command(int id, bool needCheckout)
            : this(new CommandId(id), needCheckout)
        {
        }

        public Command(CommandId[] ids, bool needCheckout)
        {
            _commandIds = ids;
            _needCheckout = needCheckout;
        }

        public Command(Guid group, int[] ids)
        {
            _commandIds = new CommandId[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                _commandIds[i] = new CommandId(group, ids[i]);
            }

            _needCheckout = false;
        }

        #region ICommand
        public virtual bool NeedCheckout(Guid group, int id)
        {
            return _needCheckout;
        }

        public virtual IList<CommandId> CommandIds
        {
            get { return _commandIds; }
        }

        public virtual CommandStatus Status(Guid group, int id)
        {
            return CommandStatus.NotSupported;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "3#")]
        public virtual CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg)
        {
            return CommandResult.NotSupported;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "4#")]
        public virtual void PostProcessInvoke(CommandResult result, Guid group, int id, object inputArg, ref object outputArg)
        {
        }
        #endregion
    }
}
