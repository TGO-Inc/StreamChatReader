var injectScript = async function () {
    // setup message queue
    window.MessageQueue = []
    // get message queue
    window.GetMessageQueue = function () {
        var q = JSON.stringify(window.MessageQueue);
        window.MessageQueue = [];
        return q;
    }
    // setup message queue
    window.DEBUGQueue = []
    // get message queue
    window.GetDEBUGQueue = function () {
        var q = JSON.stringify(window.DEBUGQueue);
        window.DEBUGQueue = [];
        return q;
    }
    // backup the original XMLHttpRequest open function
    var originalRequestOpen = XMLHttpRequest.prototype.open;
    // backup the original fetch function
    window.originalFetch = window.fetch;
    // helper functions used to intercept and modify youtube api responses
    var responseProxy = function (callback) {
        XMLHttpRequest.prototype.open = function () {
            this.addEventListener("readystatechange", function (event) {
                if (this.readyState === 4) {
                    var response = callback(this.responseURL, event.target.responseText);
                    // re-define response content properties and remove "read-only" flags
                    Object.defineProperty(this, "response", { writable: true });
                    Object.defineProperty(this, "responseText", { writable: true });
                    this.response = response;
                    this.responseText = response;
                }
            });
            return originalRequestOpen.apply(this, arguments);
        };
        // since july 2020 YouTube uses the Fetch-API to retrieve context menu items
        window.fetch = (...args) => (async (args) => {
            var result = await originalFetch(...args);
            var json = await result.json();
            // returns the original result if the request fails
            if (json === null) return result;
            var responseText = JSON.stringify(json);
            var responseTextModified = callback(result.url, responseText);
            result.json = function () {
                return new Promise(function (resolve, reject) {
                    resolve(JSON.parse(responseTextModified));
                })
            };
            result.text = function () {
                return new Promise(function (resolve, reject) {
                    resolve(responseTextModified);
                })
            };
            return result;
        })(args);
    };
    var extractAuthorExternalChannelIds = function (chatData) {
        // lets deal with this stupid json object...
        var availableCommentActions = chatData.continuationContents ? chatData.continuationContents.liveChatContinuation.actions : chatData.contents.liveChatRenderer.actions;
        if (!availableCommentActions || !Array.isArray(availableCommentActions)) return;
        // format message json
        availableCommentActions.forEach((message_elem, index) => {
            window.DEBUGQueue.push(message_elem);
            if (typeof message_elem.addChatItemAction !== "undefined") {
                var item = message_elem.addChatItemAction.item;
                if ("liveChatViewerEngagementMessageRenderer" in item) return;
                var returnMessage = {};
                var messageString = "";
                if ("liveChatTextMessageRenderer" in item) message = item.liveChatTextMessageRenderer;
                if ("liveChatPaidMessageRenderer" in item) message = item.liveChatPaidMessageRenderer;
                if ("purchaseAmountText" in message) returnMessage.purchaseAmount = message.purchaseAmountText.simpleText;
                if ("authorName" in message) returnMessage.authorName = message.authorName.simpleText;
                if ("message" in message)
                    message.message.runs.forEach((obj, index) =>
                    {
                        var name = Object.keys(obj)[0]
                        switch (name.toLowerCase()) {
                            case "text":
                                messageString += obj.text;
                                break;
                            case "emoji":
                                messageString += obj.emoji.shortcuts[0];
                                break;
                        }
                    });
                returnMessage.message = messageString;
                if ("authorPhoto" in message) returnMessage.authorPhoto = message.authorPhoto.thumbnails[1].url;
                if ("timestampUsec" in message) returnMessage.timestamp = message.timestampUsec;
                if ("id" in message) returnMessage.id = message.id.split("%")[0];
                if ("authorExternalChannelId" in message) returnMessage.channelId = message.authorExternalChannelId;
                if ("authorBadges" in message) {
                    returnMessage.memberInfo = [];
                    message.authorBadges.forEach((_item, index) => {
                        returnMessage.memberInfo.push(_item.liveChatAuthorBadgeRenderer.accessibility.accessibilityData.label);
                    })
                }
                window.MessageQueue.push(returnMessage);
                // clear chat shit for performace
                document.querySelector("#item-scroller > div > div").innerHTML = null;
                document.querySelector("#ticker > yt-live-chat-ticker-renderer > div > div:nth-child(1)").innerHTML = null;
            }
        });
    }
    // proxy function for processing and editing the api responses
    responseProxy(function (reqUrl, responseText) {
        try {
            // we will extract the channel-ids from the "get_live_chat" response | old api endpoint:
            if (reqUrl.startsWith("https://www.youtube.com/live_chat/get_live_chat?"))
                extractAuthorExternalChannelIds(JSON.parse(responseText).response);
            if (reqUrl.startsWith("https://www.youtube.com/live_chat/get_live_chat_replay?"))
                extractAuthorExternalChannelIds(JSON.parse(responseText).response);
            // new api endpoint (since july 2020):
            if (reqUrl.startsWith("https://www.youtube.com/youtubei/v1/live_chat/get_live_chat?"))
                extractAuthorExternalChannelIds(JSON.parse(responseText));
            if (reqUrl.startsWith("https://www.youtube.com/youtubei/v1/live_chat/get_live_chat_replay?"))
                extractAuthorExternalChannelIds(JSON.parse(responseText));
        } catch (ex) {
            console.error("YouTube Livechat - Exception: ", ex);
        }
        // return the original response by default
        return responseText;
    });
    // process chat comments from inital data
    /*
    // hijack youtube inital variable data from page source and rename it before it gets overwritten by youtube
    // idk how to do it better...
    var scripts = document.getElementsByTagName("script");
    for (var script of scripts) {
        if (script.text.indexOf("window[\"ytInitialData\"]") >= 0) {
            window.eval(script.text.replace("ytInitialData", "ytInitialData_original"));
        }
    }
    // if (window.ytInitialData_original) extractAuthorExternalChannelIds(window.ytInitialData_original);
    */
}

const delay = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

/*
var tryBrowserIndependentExecution = async function () {
    // window found & ready?
    if (!window || !window.document || window.document.readyState != "complete") {
        await delay(1000);
        await tryBrowserIndependentExecution();
        return;
    }
    // check live chat versus top chat
    while (!document.querySelector("#menu > a:nth-child(2)").classList.contains("iron-selected")) {
        document.querySelector("#menu > a:nth-child(2)").click();
        await delay(250);
    }
    // script already injected?
    if (window.channelResolverInitialized) return;
    // Inject main script
    await delay(1000);
    await injectScript();
    // Flag window as initalizied to prevent mutiple executions
    window.channelResolverInitialized = true;
}

tryBrowserIndependentExecution();
*/
injectScript();