const signalR = require('@microsoft/signalr')

export function createSignalRClient(address) {
    var connection =  new signalR.HubConnectionBuilder().withUrl(address).build()
    connection.start().then(result => console.log('Connected!'))
        .catch(e => console.log('Connection to chat server failed: ', e));
    return connection
}
