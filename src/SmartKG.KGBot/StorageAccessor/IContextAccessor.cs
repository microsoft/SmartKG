using SmartKG.KGBot.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartKG.KGBot.StorageAccessor
{
    public interface IContextAccessor
    {
        DialogContext GetContext(string userId, string sessionId);
        void UpdateContext(string userId, string sessionId, DialogContext context);

        void CleanContext();
    }
}
