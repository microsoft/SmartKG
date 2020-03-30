// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using SmartKG.KGBot.Data;

namespace SmartKG.KGBot.StorageAccessor
{
    public interface IContextAccessor
    {
        DialogContext GetContext(string userId, string sessionId);
        void UpdateContext(string userId, string sessionId, DialogContext context);

        void CleanContext();
    }
}
