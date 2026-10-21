using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using Microsoft.Extensions.Localization;

namespace CommunityLink.App.Services;

/// <summary>
/// Dynamic implementation of IStringLocalizer&lt;AppResources&gt; that evaluates localized strings
/// against the circuit's active CultureService instead of strictly relying on thread-static ambient cultures.
/// This enables zero-reload, in-place language switching across all injected Razor components.
/// </summary>
public class DynamicAppStringLocalizer : IStringLocalizer<AppResources>, IStringLocalizer
{
    private readonly CultureService _cultureService;
    private readonly ResourceManager _resourceManager;

    public DynamicAppStringLocalizer(CultureService cultureService)
    {
        _cultureService = cultureService ?? throw new ArgumentNullException(nameof(cultureService));
        _resourceManager = Resources.AppResources.ResourceManager;
    }

    public LocalizedString this[string name]
    {
        get
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            var culture = _cultureService.CurrentUICulture;
            var value = _resourceManager.GetString(name, culture);

            return new LocalizedString(name, value ?? name, resourceNotFound: value == null, searchedLocation: typeof(Resources.AppResources).FullName);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            var culture = _cultureService.CurrentUICulture;
            var format = _resourceManager.GetString(name, culture);

            if (format == null)
            {
                return new LocalizedString(name, name, resourceNotFound: true, searchedLocation: typeof(Resources.AppResources).FullName);
            }

            var formatted = string.Format(culture, format, arguments);
            return new LocalizedString(name, formatted, resourceNotFound: false, searchedLocation: typeof(Resources.AppResources).FullName);
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        var culture = _cultureService.CurrentUICulture;
        var resourceSet = _resourceManager.GetResourceSet(culture, createIfNotExists: true, tryParents: includeParentCultures);
        if (resourceSet != null)
        {
            foreach (DictionaryEntry entry in resourceSet)
            {
                var key = entry.Key?.ToString() ?? string.Empty;
                var val = entry.Value?.ToString() ?? key;
                yield return new LocalizedString(key, val, resourceNotFound: false, searchedLocation: typeof(Resources.AppResources).FullName);
            }
        }
    }
}
