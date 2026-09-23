// CommunityLink Executive Design System - Theme Manager
(function () {
    const THEME_KEY = 'communitylink_theme';

    function getPreferredTheme() {
        const storedTheme = localStorage.getItem(THEME_KEY);
        if (storedTheme === 'dark' || storedTheme === 'light') {
            return storedTheme;
        }
        return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    }

    function applyTheme(theme) {
        const root = document.documentElement;
        if (theme === 'dark') {
            root.classList.add('dark');
        } else {
            root.classList.remove('dark');
        }
        localStorage.setItem(THEME_KEY, theme);
        window.dispatchEvent(new CustomEvent('themeChanged', { detail: { theme } }));
    }

    window.themeManager = {
        getTheme: function () {
            return document.documentElement.classList.contains('dark') ? 'dark' : 'light';
        },
        setTheme: function (theme) {
            applyTheme(theme);
            return theme;
        },
        toggleTheme: function () {
            const current = this.getTheme();
            const next = current === 'dark' ? 'light' : 'dark';
            applyTheme(next);
            return next;
        },
        init: function () {
            const theme = getPreferredTheme();
            applyTheme(theme);
            return theme;
        }
    };
})();
