import React, { Component } from "react";
import { writeChatMessage, waitChatMessage } from "../services/chatService";
import { getUserName } from "../services/launchService";
import Launcher from "./react-chat-window/components/Launcher";

export class Chat extends Component {
    constructor() {
        super()
        this.state = {
            messageList: [],
            isOpen: false
        }
        waitChatMessage((chat, userId, message) => {
            this._sendMessage(getUserName(userId), message);
        })

        this._onMessageWasSent.bind(this)
        this._handleClick.bind(this)
    }

    _onMessageWasSent(message) {
        writeChatMessage("", message);
        this.setState({
            messageList: [...this.state.messageList, message]
        });
    }

    _sendMessage(author, text) {
        if (text.length > 0) {
            this.setState({
                messageList: [...this.state.messageList, {
                    author: 'them',
                    type: 'text',
                    data: { text }
                }]
            })
        }
    }

    _handleClick() {
        this.setState({
            isOpen: !this.state.isOpen,
            newMessagesCount: 0
        });
    }

    render() {
        return (<div>
            <Launcher
                agentProfile={{
                    teamName: 'react-chat-window',
                    imageUrl: 'https://a.slack-edge.com/66f9/img/avatars-teams/ava_0001-34.png'
                }}
            onMessageWasSent={this._onMessageWasSent}
            isOpen={this.state.isOpen}
            handleClick={this._handleClick}
            messageList={this.state.messageList}
            showEmoji
            />
        </div>)
    }
}
