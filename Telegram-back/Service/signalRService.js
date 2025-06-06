import { HubConnectionBuilder } from "@microsoft/signalr";
import * as signalR from "@microsoft/signalr";

namespace Telegram_back.Service {
    //class SignalRService {
    //    connection = null;

    //    async startConnection(token) {
    //        this.connection = new signalR.HubConnectionBuilder()
    //            .withUrl("http://localhost:5277/chathub", {
    //                accessTokenFactory: () => token
    //            })
    //            .withAutomaticReconnect()
    //            .build();

    //        this.connection.onclose(async () => {
    //            await this.startConnection(token);
    //        });

    //        try {
    //            await this.connection.start();
    //            console.log("SignalR Connected.");
    //        } catch (err) {
    //            console.error("SignalR Connection Error: ", err);
    //            setTimeout(() => this.startConnection(token), 5000);
    //        }
    //    }

    //    async sendMessage(messageDto) {
    //        if (!this.connection) return;
    //        try {
    //            await this.connection.invoke("SendMessage", messageDto);
    //        } catch (err) {
    //            console.error(err);
    //        }
    //    }

    //    onReceiveMessage(callback) {
    //        if (!this.connection) return;
    //        this.connection.on("ReceiveMessage", callback);
    //    }
    //}

    //const signalRService = new SignalRService();
    //export default signalRService;

    class SignalRService {
        constructor() {
            this.connection = null;
        }

        async startConnection(token) {
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("http://localhost:5277/chathub", { accessTokenFactory: () => token })
                .withAutomaticReconnect()
                .build();

            await this.connection.start();
            console.log("SignalR connected");

            if (chatId) {
                try {
                    await this.connection.invoke("JoinChat", chatId);
                    console.log(`Joined chat group ${chatId}`);
                } catch (err) {
                    console.error("Error joining chat group:", err);
                }
            }
        }

        onReceiveMessage(callback) {
            if (!this.connection) {
                console.error("Connection not established");
                return;
            }
            this.connection.on("ReceiveMessage", callback);
        }

        offReceiveMessage() {
            if (!this.connection) return;
            this.connection.off("ReceiveMessage");
        }

        stopConnection() {
            if (!this.connection) return;
            this.connection.stop();
        }
    }

    const signalRService = new SignalRService();
    export default signalRService;
}