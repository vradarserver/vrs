using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// A static class that creates and caches fonts for the application.
    /// </summary>
    public static class FontFactory
    {
        #region FontDescription
        /// <summary>
        /// Describes a font's important qualities.
        /// </summary>
        class FontDescription
        {
            public string FontFamily { get; private set; }

            public float Size { get; private set; }

            public FontStyle Style { get; private set; }

            public FontDescription(Font font)
            {
                FontFamily = font.FontFamily.Name;
                Size = font.Size;
                Style = font.Style;
            }

            public FontDescription(Font font, FontDescription overrideWith, float newSize) : this(font)
            {
                FontFamily = overrideWith.FontFamily;
                Size = newSize;
            }

            public override bool Equals(object obj)
            {
                var result = Object.ReferenceEquals(obj, this);
                if(!result) {
                    var other = obj as FontDescription;
                    result = other != null &&
                             other.FontFamily == FontFamily &&
                             other.Size == Size &&
                             other.Style == Style;
                }

                return result;
            }

            public override int GetHashCode()
            {
                return FontFamily.GetHashCode();
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// A cache of fonts.
        /// </summary>
        private static Dictionary<FontDescription, Font> _FontCache = new Dictionary<FontDescription,Font>();

        /// <summary>
        /// True if <see cref="FindAlternateUIFonts"/> has run at least once.
        /// </summary>
        private static bool _SearchedForAlternateUIFonts;

        /// <summary>
        /// The descriptor for the UI font that we would like to use instead of the design font.
        /// </summary>
        private static FontDescription _PreferredUIFont;

        /// <summary>
        /// The value to scale design fonts by (assuming a design size of 8.25) to get the correct size of preferred font.
        /// </summary>
        private static float _PreferredUIFontScale;

        /// <summary>
        /// True if the preferred UI font doesn't get on well with WinForms auto-sizing.
        /// </summary>
        private static bool _PreferredUIFontClashesWithAutoSize;
        #endregion

        #region Properties
        /// <summary>
        /// Gets a value indicating that there is a suitable preferred UI font available.
        /// </summary>
        public static bool HasPreferredUIFont
        {
            get
            {
                FindAlternateUIFonts();
                return _PreferredUIFont != null;
            }
        }

        /// <summary>
        /// Gets a value indicating that the preferred UI font clashes with WinForms autosizing.
        /// </summary>
        public static bool PreferredUIFontClashesWithAutoSizing
        {
            get { return HasPreferredUIFont ? _PreferredUIFontClashesWithAutoSize : false; }
        }

        /// <summary>
        /// Gets or sets a value indicating that font replacement should not be performed.
        /// </summary>
        public static bool DisableFontReplacement;
        #endregion

        #region GetFont
        /// <summary>
        /// Fetches a font from the cache or creates an entry for it if one does not already exist.
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        private static Font GetFont(FontDescription description)
        {
            Font result;
            if(!_FontCache.TryGetValue(description, out result)) {
                result = new Font(description.FontFamily, description.Size, description.Style);
                _FontCache.Add(description, result);
            }

            return result;
        }
        #endregion

        #region FindAlternateUIFonts
        /// <summary>
        /// Examines the fonts installed on the system to find ones that we would like
        /// to use in preference to the design font.
        /// </summary>
        private static void FindAlternateUIFonts()
        {
            if(!_SearchedForAlternateUIFonts) {
                _SearchedForAlternateUIFonts = true;

                if(!DisableFontReplacement) {
                    var preferredFontFamilies =     new List<string> {}; // { "Segoe UI", "Ubuntu", };
                    var preferredSizes =            new List<float>  {}; // { 9F,         9F, };
                    var preferredDisableAutoSize =  new List<bool>   {}; // { true,       true, };
                
                    // If this looks like it might be a non-Western code-page then prefer Arial Unicode MS
                    try {
                        switch(Thread.CurrentThread.CurrentUICulture.TextInfo.ANSICodePage) {
                            case 1250:
                            case 1251:
                            case 1252:
                                break;
                            default:
                                preferredFontFamilies.Insert(0, "Arial Unicode MS");
                                preferredSizes.Insert(0, 9F);
                                preferredDisableAutoSize.Insert(0, true);
                                break;
                        }
                    } catch {
                        ;
                    }

                    for(var i = 0;i < preferredFontFamilies.Count;++i) {
                        var fontFamilyName = preferredFontFamilies[i];
                        var fontSize = preferredSizes[i];
                        var disableAutoSize = preferredDisableAutoSize[i];

                        using(var font = new Font(fontFamilyName, fontSize)) {
                            if(font.FontFamily.Name == fontFamilyName) {
                                _PreferredUIFont = new FontDescription(font);
                                _PreferredUIFontScale = fontSize / 8.25F;
                                _PreferredUIFontClashesWithAutoSize = disableAutoSize;
                                break;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region GetPreferredUIFont
        /// <summary>
        /// Returns the font to use for user interface elements or null if the design font should be used.
        /// </summary>
        /// <param name="designFont"></param>
        /// <returns></returns>
        public static Font GetPreferredUIFont(Font designFont)
        {
            Font result = null;

            if(designFont != null) {
                FindAlternateUIFonts();
                if(_PreferredUIFont != null && designFont.FontFamily.Name != _PreferredUIFont.FontFamily) {
                    var newSize = ((int)((designFont.Size * _PreferredUIFontScale) * 100F)) / 100F;
                    var description = new FontDescription(designFont, _PreferredUIFont, newSize);
                    result = GetFont(description);
                }
            }

            return result;
        }
        #endregion
    }
}
