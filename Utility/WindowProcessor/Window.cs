// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Text;

namespace WindowProcessor
{
    public class Window
    {
        protected FBColors _InitialColors;

        protected Point _InitialPosition;

        public int AvailableWidth => Console.WindowWidth - Console.CursorLeft;

        public int LastUsableLine => Console.WindowHeight - 1;

        public Point Position
        {
            get => Point.Current;
            set => value.Apply();
        }

        public int Left
        {
            get => Console.CursorLeft;
            set => Console.CursorLeft = value;
        }

        public int Top
        {
            get => Console.CursorTop;
            set => Console.CursorTop = value;
        }

        public Window()
        {
            _InitialColors = FBColors.Current;
            _InitialPosition = Point.Current;
            Initialise();
        }

        protected virtual void Initialise()
        {
        }

        protected bool _Redrawing;
        protected void Redraw()
        {
            if(!_Redrawing) {
                _Redrawing = true;
                try {
                    DoRedraw();
                } finally {
                    _Redrawing = false;
                }
            }
        }

        protected virtual void DoRedraw()
        {
        }

        public void ClearScreen(ConsoleColor background)
        {
            Console.BackgroundColor = background;
            Console.Clear();
        }

        public void ClearToEndOfLine(char padChar = ' ')
        {
            var padding = AvailableWidth;
            if(padding > 0) {
                var buffer = new StringBuilder();
                for(var idx = 0;idx < padding;++idx) {
                    buffer.Append(padChar);
                }
                Console.Write(buffer.ToString());
            }
        }

        public void WriteField(string text, int availableWidth, Alignment alignment)
        {
            text ??= "";
            if(text.Length > availableWidth) {
                text = alignment == Alignment.Right
                    ? text[^availableWidth..]
                    : text[..availableWidth];
            }

            var padding = availableWidth - text.Length;

            if(alignment != Alignment.Left) {
                var pad = alignment == Alignment.Centre ? padding / 2 : padding;
                Write(' ', pad);
                padding -= pad;
            }
            Write(text);
            if(alignment != Alignment.Right) {
                Write(' ', padding);
            }
        }

        public void Write(string text)
        {
            if(!String.IsNullOrEmpty(text)) {
                var actualLength = Math.Min(text.Length, AvailableWidth);
                Console.Write(text[..actualLength]);
            }
        }

        public void Write(char ch)
        {
            if(AvailableWidth > 0) {
                Console.Write(ch);
            }
        }

        public void Write(string text, int count)
        {
            if(!String.IsNullOrEmpty(text)) {
                for(var idx = 0;idx < count;++idx) {
                    Write(text);
                }
            }
        }

        public void Write(char ch, int count)
        {
            count = Math.Min(count, AvailableWidth);
            for(var idx = 0;idx < count;++idx) {
                Console.Write(ch);
            }
        }

        private CancellationTokenSource _CancellationTokenSource;
        protected CancellationToken _CancellationToken;

        protected virtual void HandleKeyPress(ConsoleKeyInfo keyInfo)
        {
        }

        public async Task EventLoop(CancellationTokenSource cancellationTokenSource)
        {
            _CancellationTokenSource = cancellationTokenSource;
            _CancellationToken = cancellationTokenSource.Token;

            await Task.Run(() => {
                while(!cancellationTokenSource.IsCancellationRequested) {
                    if(!Console.KeyAvailable) {
                        Thread.Sleep(1);
                    } else {
                        var keyInfo = Console.ReadKey(intercept: true);
                        HandleKeyPress(keyInfo);
                    }
                }
            }, cancellationTokenSource.Token);
        }

        public void Cancel()
        {
            _CancellationTokenSource.Cancel();
        }
    }
}
