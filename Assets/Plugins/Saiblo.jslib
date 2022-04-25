mergeInto(LibraryManager.library, {
    GetReplay: function () {
        const replay = getReplay();
        const bufferSize = lengthBytesUTF8(replay) + 1;
        const buffer = _malloc(bufferSize);
        stringToUTF8(replay, buffer, bufferSize);
        return buffer;
    },
    JsAlert: function (message) {
        window.alert(UTF8ToString(message));
    },
});