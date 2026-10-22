using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using Microsoft.Extensions.Localization;

namespace CommunityLink.App.Services;

public class DynamicAppStringLocalizer : IStringLocalizer<CommunityLink.App.AppResources>
{
    private readonly CultureService _cultureService;
    private readonly ResourceManager _resourceManager;

    public DynamicAppStringLocalizer(CultureService cultureService)
    {
        _cultureService = cultureService;
        _resourceManager = CommunityLink.App.Resources.AppResources.ResourceManager;
    }

    public LocalizedString this[string name]
    {
        get
        {
            if (string.IsNullOrEmpty(name))
            {
                return new LocalizedString(name ?? string.Empty, string.Empty, resourceNotFound: true);
            }

            var culture = _cultureService.CurrentCulture;
            var val = _resourceManager.GetString(name, culture);
            return new LocalizedString(name, val ?? name, resourceNotFound: val == null);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            if (string.IsNullOrEmpty(name))
            {
                return new LocalizedString(name ?? string.Empty, string.Empty, resourceNotFound: true);
            }

            var culture = _cultureService.CurrentCulture;
            var format = _resourceManager.GetString(name, culture);
            if (format == null)
            {
                return new LocalizedString(name, name, resourceNotFound: true);
            }

            try
            {
                var val = string.Format(culture, format, arguments);
                return new LocalizedString(name, val, resourceNotFound: false);
            }
            catch (FormatException)
            {
                return new LocalizedString(name, format, resourceNotFound: false);
            }
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        var culture = _cultureService.CurrentCulture;
        var resourceSet = _resourceManager.GetResourceSet(culture, createIfNotExists: true, tryParents: true);
        if (resourceSet != null)
        {
            foreach (DictionaryEntry entry in resourceSet)
            {
                var key = entry.Key?.ToString();
                if (!string.IsNullOrEmpty(key))
                {
                    yield return new LocalizedString(key, entry.Value?.ToString() ?? key);
                }
            }
        }
    }
}
