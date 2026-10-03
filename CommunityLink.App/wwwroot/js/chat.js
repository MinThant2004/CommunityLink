// Interop helpers for the unified Chat surface.
window.communityLinkChat = {
    // Keeps the message stream pinned to the newest message. Blazor's JS interop only
    // forwards the arguments of a named function, so this must be a real function
    // rather than an inline eval expression.
    scrollToBottom: function (element) {
        if (!element) {
            return;
        }

        element.scrollTop = element.scrollHeight;
    },

    // Moves focus onto the dialog shell, but only when focus is not already inside the
    // dialog. Blazor re-renders dialog content on every keystroke, so an unconditional
    // focus() would blur the input the user is typing into after the first character.
    focusDialog: function (panel) {
        if (!panel) {
            return;
        }

        var active = document.activeElement;
        if (active && panel.contains(active)) {
            return;
        }

        panel.focus();
    },

    // Wraps Tab / Shift+Tab focus to the first and last focusable control inside the
    // dialog, so keyboard users cannot move focus behind the overlay.
    trapTab: function (panel, shiftKey) {
        if (!panel) {
            return;
        }

        var focusable = panel.querySelectorAll(
            'a[href], button:not([disabled]), textarea:not([disabled]), input:not([disabled]), select:not([disabled]), [tabindex]:not([tabindex="-1"])'
        );

        if (focusable.length === 0) {
            return;
        }

        var first = focusable[0];
        var last = focusable[focusable.length - 1];
        var active = document.activeElement;

        if (shiftKey && (active === first || active === panel)) {
            last.focus();
        } else if (!shiftKey && active === last) {
            first.focus();
        }
    },

    // Wraps a byte array in a Blob and returns an object URL for local image previews.
    // Object URLs pin the blob in memory, so every caller must revoke the returned URL
    // once the preview is replaced or the component goes away.
    createObjectUrl: function (bytes, contentType) {
        var type = contentType || 'application/octet-stream';
        return URL.createObjectURL(new Blob([new Uint8Array(bytes)], { type: type }));
    },

    // Releases a URL handed out by createObjectUrl. Unknown URLs are ignored so a
    // double dispose is harmless.
    revokeObjectUrl: function (url) {
        if (url) {
            URL.revokeObjectURL(url);
        }
    },

    // Scrolls smoothly to a target message by element ID ('msg-' + id) and applies a Telegram-style highlight animation
    scrollToAndHighlightMessage: function (messageId) {
        if (!messageId) {
            return;
        }

        var el = document.getElementById('msg-' + messageId);
        if (el) {
            el.scrollIntoView({ behavior: 'smooth', block: 'center' });
            el.classList.remove('telegram-highlight');
            // Trigger reflow to restart animation if clicked repeatedly
            void el.offsetWidth;
            el.classList.add('telegram-highlight');
            setTimeout(function () {
                el.classList.remove('telegram-highlight');
            }, 2200);
        }
    },

    // Measures available vertical space around an element to determine whether
    // a dropdown popup box should open above or below the element.
    getPopupPlacement: function (element) {
        if (!element) {
            return 'below';
        }

        var rect = element.getBoundingClientRect();
        var windowHeight = window.innerHeight || document.documentElement.clientHeight;

        // Estimated height of context menu (approx 280px)
        var menuHeight = 280;

        var spaceBelow = windowHeight - rect.bottom;
        var spaceAbove = rect.top;

        // If space below is less than menu height AND space above has more room, flip to above
        if (spaceBelow < menuHeight && spaceAbove > spaceBelow) {
            return 'above';
        }

        return 'below';
    }
};
