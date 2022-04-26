mergeInto(LibraryManager.library, {
    GetReplay: function () {
        const replay = getReplay();
        const bufferSize = lengthBytesUTF8(replay) + 1;
        const buffer = _malloc(bufferSize);
        stringToUTF8(replay, buffer, bufferSize);
        return buffer;
    },
    GetToken: function () {
        const token = getToken();
        const bufferSize = lengthBytesUTF8(token) + 1;
        const buffer = _malloc(bufferSize);
        stringToUTF8(token, buffer, bufferSize);
        return buffer;
    },
    ConnectSaiblo: function (tokenDecoded, tokenEncoded) {
        const websocket = new WebSocket("wss://" + UTF8ToString(tokenDecoded));
        websocket.onopen = function (event) {
            console.log("judger connected");
            websocket.send(JSON.stringify({
                token: UTF8ToString(tokenEncoded),
                request: "connect",
            }))
        };
        bindWebsocket(websocket, UTF8ToString(tokenEncoded));
    },
    SendWsMessage: function (message) {
        sendWebsocketMessage(UTF8ToString(message));
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