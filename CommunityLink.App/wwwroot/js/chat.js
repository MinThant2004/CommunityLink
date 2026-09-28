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
    }
};
