mergeInto(LibraryManager.library, {
    GetReplay: function () {
        if (window.replay === undefined) {
            return "";
        }
        return window.replay;
    },
});