// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
"use strict";

import * as io from "socket.io";
import * as http from "http";
import { createDeferred, Deferred } from "./deferred";
import { EventEmitter } from "events";
import * as express from "express";
import { Express, Request, Response } from "express";
import * as path from "path";
import * as cors from "cors";
import * as vscode from "vscode";

const uniqid = require("uniqid");

export class ResultsServer extends EventEmitter {
    private server: SocketIO.Server;
    private app: Express;
    private httpServer: http.Server;
    private clients: SocketIO.Socket[] = [];

    constructor() {
        super();
        this.responsePromises = new Map<string, Deferred<boolean>>();
    }

    dispose() {
        this.app = null;
        this.port = null;
        if (this.httpServer) {
            this.httpServer.close();
            this.httpServer = null;
        }
        if (this.server) {
            this.server.close();
            this.server = null;
        }
    }

    private port: number;
    start(): Promise<number> {
        if (this.port) {
            return Promise.resolve(this.port);
        }

        let def = createDeferred<number>();

        this.app = express();
        this.httpServer = http.createServer(this.app);
        this.server = io(this.httpServer);

        const rootDirectory = path.join(__dirname, "..");
        this.app.use(express.static(rootDirectory));
        // Required by transformime
        // It will look in the path http://localhost:port/resources/MathJax/MathJax.js
        this.app.use(express.static(path.join(__dirname, "..", "node_modules", "mathjax-electron")));
        this.app.use(cors());
        // this.app.get('/', function (req, res, next) {
        //     res.sendFile(path.join(rootDirectory, 'index.html'));
        // });
        this.app.get("/", (req, res, next) => {
            this.rootRequestHandler(req, res);
        });

        this.httpServer.listen(0, () => {
            this.port = this.httpServer.address().port;
            def.resolve(this.port);
            def = null;
        });
        this.httpServer.on("error", error => {
            if (def) {
                def.reject(error);
            }
        });

        this.server.on("connection", this.onSocketConnection.bind(this));
        return def.promise;
    }

    rootRequestHandler(req: Request, res: Response) {
        const theme: string = req.query.theme;
        const backgroundColor: string = req.query.backgroundcolor;
        const color: string = req.query.color;
        const editorConfig = vscode.workspace.getConfiguration("editor");
        const fontFamily = editorConfig.get<string>("fontFamily").split("'").join("").split('"').join("");
        const fontSize = editorConfig.get<number>("fontSize") + "px";
        const fontWeight = editorConfig.get<string>("fontWeight");
        res.render(path.join(__dirname, "render", "index.ejs"),
            {
                theme: theme,
                backgroundColor: backgroundColor,
                color: color,
                fontFamily: fontFamily,
                fontSize: fontSize,
                fontWeight: fontWeight
            }
        );
    }

    private buffer: any[] = [];

    clearBuffer() {
        this.buffer = [];
    }

    sendResults(code: string, data: string) {
        let results: string;

        if (code.length > 64) {
            code = code.substring(0, 64).concat("...");
        }
        code = this.formatCode(code, null);
        this.buffer = this.buffer.concat(code);

        if (data.startsWith("$$IMAGE ")) {
            const base64 = data.substring(8, data.length - 8);
            results = `"<img src='data:image/gif;base64, ${base64}' style='display:block; margin: 0 auto; text-align: center' />"`;
        } else if (data.startsWith("$$ERROR ")) {
            const error = data.substring(8, data.length - 8);
            results = this.formatError(error);
        } else {
            results = this.formatCode(data, null);
        }

        this.buffer = this.buffer.concat(results);
        this.broadcast("results", results);
    }

    sendSetting(name: string, value: any) {
        this.broadcast(name, value);
    }

    private broadcast(eventName: string, data: any) {
        this.server.emit(eventName, data);
    }

    private onSocketConnection(socket: SocketIO.Socket) {
        this.clients.push(socket);

        socket.on("disconnect", () => {
            const index = this.clients.findIndex(sock => sock.id === socket.id);
            if (index >= 0) {
                this.clients.splice(index, 1);
            }
        });

        socket.on("clientExists", (data: { id: string }) => {
            console.log("clientExists, on server");
            console.log(data);
            if (!this.responsePromises.has(data.id)) {
                console.log("Not found");
                return;
            }
            const def = this.responsePromises.get(data.id);
            this.responsePromises.delete(data.id);
            def.resolve(true);
        });

        socket.on("settings.appendResults", (data: any) => {
            this.emit("settings.appendResults", data);
        });

        socket.on("clearResults", () => {
            this.buffer = [];
        });

        socket.on("results.ack", () => {
            this.buffer = [];
        });

        this.emit("connected");

        // Someone is connected, send them the data we have
        socket.emit("results", this.buffer);
    }

    private responsePromises: Map<string, Deferred<boolean>>;
    clientsConnected(timeoutMilliSeconds: number): Promise<any> {
        const id = new Date().getTime().toString();
        const def = createDeferred<boolean>();
        this.broadcast("clientExists", { id: id });
        this.responsePromises.set(id, def);

        setTimeout(() => {
            if (this.responsePromises.has(id)) {
                this.responsePromises.delete(id);
                def.resolve(false);
                console.log("Timeout");
            }
        }, timeoutMilliSeconds);

        return def.promise;
    }

    private formatError(text: string): string {
        return this.formatCode(text, "color: red;");
    }

    private formatCode(code: string, style: string): string {
        if (style === undefined || style === null) {
            style = "";
        }
        return `"<code style='white-space: pre-wrap; display: block; ${style}' > ${code} </code>"`;
    }
}