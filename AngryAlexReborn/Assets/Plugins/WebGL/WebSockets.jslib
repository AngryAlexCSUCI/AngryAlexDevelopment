
//Copyright (c) 2015 Gumpanat Keardkeawfa
//Licensed under the MIT license

//Websocket Jslib for UnityWebgl
//We will save this file as *.jslib for using in UNITY
var WebSockets = {
    Hello: function () {
        return "Hello,world!";
    },
    InitWebSocket: function (url) {
        console.log("Initializing web socket.");
        var init_url = Pointer_stringify(url);
        console.log("Url generated: " + init_url);
        window.wsclient = new WebSocket(init_url);
        console.log("Websocket created from url");
        window.wsclient.onopen = function (evt) {
            console.log("[open]" + init_url);
            window.wsclient.send("hello");
        };
        window.wsclient.onclose = function (evt) {
            var msg = evt.code && evt.reason ? evt.code + " " + evt.reason : JSON.stringify(evt);
            console.log("[close] " + msg);
        };
        window.wsclient.onmessage = function (evt) {
            var received_msg = evt.data;
            console.log("Message received:");
            console.log(received_msg);
            if (received_msg == "hello") {
                window.wsclient.send("hello");
            } else {
                console.log("[recv] " + received_msg);
                SendMessage('WebSocket', 'RecvString', received_msg);
            }
        };
        window.wsclient.onerror = function (evt) {
            var error_msg = evt.data ? evt.data : JSON.stringify(evt);
            console.log("[error] " + error_msg);
            SendMessage('WebSocket', 'ErrorString', "close");
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
