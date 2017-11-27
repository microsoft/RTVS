// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
"use strict";

export interface IDeferred<T> {
    readonly promise: Promise<T>;
    readonly resolved: boolean;
    readonly rejected: boolean;
    readonly completed: boolean;
    resolve(value?: T | PromiseLike<T>);
    reject(reason?: any);
}

class DeferredImpl<T> implements IDeferred<T> {
    private _resolve: (value?: T | PromiseLike<T>) => void;
    private _reject: (reason?: any) => void;
    private _resolved: boolean = false;
    private _rejected: boolean = false;
    private _promise: Promise<T>;
    constructor(private scope: any = null) {
        this._promise = new Promise<T>((res, rej) => {
            this._resolve = res;
            this._reject = rej;
        });
    }
    resolve(value?: T | PromiseLike<T>) {
        this._resolve.apply(this.scope ? this.scope : this, arguments);
        this._resolved = true;
    }
    reject(reason?: any) {
        this._reject.apply(this.scope ? this.scope : this, arguments);
        this._rejected = true;
    }
    get promise(): Promise<T> {
        return this._promise;
    }
    get resolved(): boolean {
        return this._resolved;
    }
    get rejected(): boolean {
        return this._rejected;
    }
    get completed(): boolean {
        return this._rejected || this._resolved;
    }
}
export function createDeferred<T>(scope: any = null): IDeferred<T> {
    return new DeferredImpl<T>(scope);
}
