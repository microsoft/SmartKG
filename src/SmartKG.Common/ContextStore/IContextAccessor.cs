// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using SmartKG.Common.Data;

namespace SmartKG.Common.ContextStore
{
    public interface IContextAccessor
    {
        (bool, DialogContext) GetContext(string userId, string sessionId);
        bool UpdateContext(string userId, string sessionId, DialogContext context);

        void CleanContext();
    }
}
