using System;

namespace Immutable.Passport.Core
{
    [Serializable]
    [Preserve]
    public class BrowserRequest
    {
        public string fxName;
        public string requestId;
        public string data;
    }
}

