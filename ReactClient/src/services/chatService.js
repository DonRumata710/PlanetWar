import { createSignalRClient } from "../utils/signalRUtils"

const launchServer = 'https://localhost:44351'
const ApiUrl = launchServer;

let chatClient = createSignalRClient(ApiUrl + '/chat/ChatHub')

export function writeChatMessage(chat, message) {
    chatClient.invoke('send', chat, message).then((result) => {
        console.log(result)
    }).catch((error) => {
        console.log("chat invoking failed ", error)
    });
}

export function waitChatMessage(messageHandler) {
    chatClient.on("send", messageHandler)
}
