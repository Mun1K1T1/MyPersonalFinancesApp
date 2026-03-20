using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace FinanceManager.Helpers
{
    public static class LocalizationHelper
    {
        public static Dictionary<string, string> GenerateLocalizedStringsDictionary(Type keysContainerType, IStringLocalizer localizer)
        {
            var localizedStrings = new Dictionary<string, string>();

            PropertyInfo[] properties = keysContainerType.GetProperties(BindingFlags.Public | BindingFlags.Static);

            foreach (PropertyInfo property in properties)
            {
                string keyName = property.Name;

                LocalizedString localizedString = localizer[keyName];

                localizedStrings.Add(keyName, localizedString.Value);
            }

            return localizedStrings;
        }
    }
}