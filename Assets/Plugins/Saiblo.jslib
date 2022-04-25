mergeInto(LibraryManager.library, {
    GetReplay: function () {
        const replay = getReplay();
        const bufferSize = lengthBytesUTF8(replay) + 1;
        const buffer = _malloc(bufferSize);
        stringToUTF8(replay, buffer, bufferSize);
        return buffer;
    },
    GetPlayers: function () {
        const players = getPlayers();
        const playersText = players === undefined ? "" : players[1] + " v.s. " + players[0];
        const bufferSize = lengthBytesUTF8(playersText) + 1;
        const buffer = _malloc(bufferSize);
        stringToUTF8(playersText, buffer, bufferSize);
        return buffer;
    },
    JsAlert: function (message) {
        window.alert(UTF8ToString(message));
    },
});