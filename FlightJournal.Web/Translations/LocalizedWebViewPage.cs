﻿using System;
using System.Web.Mvc;

namespace FlightJournal.Web.Translations
{
    /// <summary>
    /// Insert into Views\Web.config section system.web.webPages.razor override pages pageBaseType with FlightJournal.Web.Translations.LocalizedWebViewPage
    /// </summary>
    public abstract class LocalizedWebViewPage<TModel> : WebViewPage<TModel>  
    {
        /// <summary>
        /// Localized Translation (i18n)
        /// </summary>
        /// <param name="en"></param>
        /// <returns></returns>
        /// <example>@_("Country")</example>
        public string _(string en)
        {
            return Internationalization.GetText(en, Internationalization.LanguageCode);
        }
        public string __(string en)
        {
            return "\"" + Internationalization.GetText(en, Internationalization.LanguageCode) + "\"";
        }

        /// <summary>
        /// Localized Translation (i18n) String format
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <example>@_("Benyttes på {0} registreringer."), usedCount)</example>
        public string _(string format, params Object[] args)
        {
            if (format == null || args == null)
                throw new ArgumentNullException((format == null) ? "format" : "args");

            return string.Format(Internationalization.GetText(format, Internationalization.LanguageCode), args);
        }
    }
}
