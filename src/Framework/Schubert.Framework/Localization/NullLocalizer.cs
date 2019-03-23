using System;

namespace Schubert.Framework.Localization
{
    public static class NullLocalizer
    {

        private static readonly Localizer _instance;

        static NullLocalizer()
        {
            _instance = (key, defaultString, args) => (args == null || args.Length == 0) ? defaultString : String.Format(defaultString, args);
        }


        public static Localizer Instance { get { return _instance; } }
    }
}