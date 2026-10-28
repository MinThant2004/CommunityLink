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

    window.setCulturePreference = function (culture) {
        var canonical = (culture && culture.toLowerCase().startsWith('my')) ? 'my' : 'en';
        var cookieVal = encodeURIComponent('c=' + canonical + '|uic=' + canonical);
        document.cookie = '.AspNetCore.Culture=' + cookieVal + ';path=/;max-age=31536000;SameSite=Lax';
        document.documentElement.lang = canonical;
    };
})();

window.scrollToElement = function (id) {
    var el = document.getElementById(id);
    if (!el) return;

    var parent = el.parentElement;
    var scrollContainer = null;
    while (parent && parent !== document.body && parent !== document.documentElement) {
        var style = window.getComputedStyle(parent);
        var overflowY = style.overflowY || style.overflow;
        if (overflowY === 'auto' || overflowY === 'scroll') {
            scrollContainer = parent;
            break;
        }
        parent = parent.parentElement;
    }

    if (!scrollContainer) {
        scrollContainer = document.querySelector('main') || window;
    }

    if (scrollContainer && scrollContainer !== window && scrollContainer.scrollTo) {
        var rect = el.getBoundingClientRect();
        var containerRect = scrollContainer.getBoundingClientRect();
        var targetTop = scrollContainer.scrollTop + (rect.top - containerRect.top) - 16;
        scrollContainer.scrollTo({ top: targetTop, behavior: 'smooth' });
    } else {
        el.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
};