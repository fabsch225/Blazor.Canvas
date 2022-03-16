﻿using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Excubo.Blazor.Canvas
{
    internal class BlobCallback : IDisposable
    {
        private readonly IJSRuntime js;
        private Action<Blob> callback { get; set; }

        public BlobCallback(IJSRuntime js, Action<Blob> callback)
        {
            this.js = js;
            this.callback = callback;
            objRef = DotNetObjectReference.Create(this);
        }

        public DotNetObjectReference<BlobCallback> objRef { get; init; }

        [JsonIgnore]
        public IJSObjectReference blobWrapper { get; set; }

        public void Dispose()
        {
            objRef.Dispose();
        }

        [JSInvokable("Callback")]
        public async Task InvokeCallback()
        {
            await js.InvokeVoidAsync("eval", "window.blobWrapper = {}");
            var windowBlobWrapper = await js.InvokeAsync<IJSObjectReference>("eval", "window.blobWrapper");
            await js.InvokeVoidAsync("Object.assign", windowBlobWrapper, blobWrapper);
            var jSBlob = await js.InvokeAsync<IJSObjectReference>("eval", "window.blobWrapper.blob");
            var blob = new Blob(jSBlob);
            callback.Invoke(blob);
            this.Dispose();
        }
    }
}
