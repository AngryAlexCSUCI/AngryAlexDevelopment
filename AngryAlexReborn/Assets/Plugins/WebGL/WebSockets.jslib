
//Copyright (c) 2015 Gumpanat Keardkeawfa
//Licensed under the MIT license

//Websocket Jslib for UnityWebgl
//We will save this file as *.jslib for using in UNITY
var WebSockets = {

    InitWebSocket: function (url) {
        var init_url = Pointer_stringify(url);
        window.wsclient = new WebSocket(init_url);

        window.wsclient.onopen = function (evt) {
            console.log("[open]" + init_url);
            window.wsclient.send("connected");
            window.wsclient.send("play "); // todo how to send player name and player spawnpoints in this message?
        };

        window.wsclient.onclose = function (evt) {
            var msg = evt.code && evt.reason ? evt.code + " " + evt.reason : JSON.stringify(evt);
            console.log("[close] " + msg);
            window.wsclient.send("disconnect");
        };

        // todo send message when player connects to spawn player

        window.wsclient.onmessage = function (evt) {
            var received_msg = evt.data;
            console.log("[received] " + received_msg);
            // todo parse received message with player information in it
            if (received_msg.startsWIth("play ")) {
                SendMessage('WebSocketManager', 'OnPlay', received_msg); // call websocket manager? 
            }
//            SendMessage('WebSocketManager', 'RecvString', received_msg); // echos message back to server
        };

        window.wsclient.onerror = function (evt) {
            var error_msg = evt.data ? evt.data : JSON.stringify(evt);
            console.log("[error] " + error_msg);
            SendMessage('WebSocketManager', 'ErrorString', "close");
        };
    },

    State: function () {
        var status = 0;
        if ((typeof window.wsclient !== "undefined") && (window.wsclient !== null))
            status = window.wsclient.readystate;
        return status;
    },

    Send: function (msg) {
        var message = Pointer_stringify(msg);
        if (typeof window.wsclient !== "undefined") {
            console.log("[send] " + message);
            window.wsclient.send(message);
        } else {
            console.log("[send-failed] " + message);
        }
    },

    Close: function () {
        if ((typeof window.wsclient !== "undefined") && (window.wsclient !== null))
            window.wsclient.close();
    }
}
mergeInto(LibraryManager.library, WebSockets);
