/**
 * Universal Click Outside & Escape key detector for Blazor dropdowns/modals
 */
window.clickOutsideHandler = {
    listeners: new Map(),

    register: function (elementId, dotNetRef, methodName) {
        this.unregister(elementId);

        const onPointerDown = (event) => {
            const el = document.getElementById(elementId);
            if (!el) {
                this.unregister(elementId);
                return;
            }
            if (!el.contains(event.target)) {
                dotNetRef.invokeMethodAsync(methodName).catch(() => {});
            }
        };

        const onKeyDown = (event) => {
            if (event.key === "Escape") {
                dotNetRef.invokeMethodAsync(methodName).catch(() => {});
            }
        };

        setTimeout(() => {
            document.addEventListener("pointerdown", onPointerDown);
            document.addEventListener("keydown", onKeyDown);
            this.listeners.set(elementId, { onPointerDown, onKeyDown });
        }, 0);
    },

    unregister: function (elementId) {
        const entry = this.listeners.get(elementId);
        if (entry) {
            document.removeEventListener("pointerdown", entry.onPointerDown);
            document.removeEventListener("keydown", entry.onKeyDown);
            this.listeners.delete(elementId);
        }
    }
};
