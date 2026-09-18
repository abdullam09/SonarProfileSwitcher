using System.Net;
using System.Text;
using System.Text.Json;
using SonarProfileSwitcher.Interfaces;

namespace SonarProfileSwitcher.Services
{
    /// <summary>
    /// Publishes the current app state to the bundled iCUE widget over loopback only.
    /// The widget polls http://127.0.0.1:9783/status once per second.
    /// </summary>
    public sealed class WidgetStateService : IWidgetStateService
    {
        private const string Prefix = "http://127.0.0.1:9783/";
        private readonly HttpListener _listener = new();
        private readonly object _stateLock = new();
        private WidgetState _state = new("Flat", "", DateTimeOffset.UtcNow);

        public WidgetStateService()
        {
            _listener.Prefixes.Add(Prefix);
            _listener.Start();
            _ = ListenAsync();
        }

        public void Update(string profileName, string keyboardLayout)
        {
            lock (_stateLock)
            {
                _state = new WidgetState(profileName, keyboardLayout, DateTimeOffset.UtcNow);
            }
        }

        public void Dispose()
        {
            _listener.Close();
        }

        private async Task ListenAsync()
        {
            try
            {
                while (_listener.IsListening)
                {
                    var context = await _listener.GetContextAsync();
                    await RespondAsync(context);
                }
            }
            catch (HttpListenerException) when (!_listener.IsListening)
            {
                // The host is shutting down.
            }
            catch (ObjectDisposedException)
            {
                // The host is shutting down.
            }
        }

        private async Task RespondAsync(HttpListenerContext context)
        {
            var response = context.Response;
            response.Headers["Access-Control-Allow-Origin"] = "*";
            response.Headers["Cache-Control"] = "no-store";

            if (context.Request.HttpMethod == "OPTIONS")
            {
                response.Headers["Access-Control-Allow-Methods"] = "GET, OPTIONS";
                response.StatusCode = (int)HttpStatusCode.NoContent;
                response.Close();
                return;
            }

            if (context.Request.HttpMethod != "GET" || context.Request.Url?.AbsolutePath.TrimEnd('/') != "/status")
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Close();
                return;
            }

            WidgetState state;
            lock (_stateLock)
            {
                state = _state;
            }

            var payload = JsonSerializer.Serialize(state);
            var bytes = Encoding.UTF8.GetBytes(payload);
            response.ContentType = "application/json; charset=utf-8";
            response.ContentLength64 = bytes.Length;
            await response.OutputStream.WriteAsync(bytes);
            response.Close();
        }

        private sealed record WidgetState(string ProfileName, string KeyboardLayout, DateTimeOffset UpdatedAt);
    }
}
