// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Extensions;
using WindowProcessor;

namespace VirtualRadar.Utility.CLIConsole
{
    class ConsoleTable<T>
    {
        public Column[] Columns { get; }

        public Func<T, string>[] CellExtractors { get; }

        public ConsoleTable(IEnumerable<(Column, Func<T, string>)> columns)
        {
            Columns = columns.Select(r => r.Item1).ToArray();
            CellExtractors = columns.Select(r => r.Item2).ToArray();
        }

        public async Task Dump(IEnumerable<T> rows)
        {
            await DumpHeader();
            await DumpBody(rows);
        }

        public async Task DumpHeader()
        {
            for(var idx = 0;idx < Columns.Length;++idx) {
                var column = Columns[idx];
                if(idx > 0) {
                    await Console.Out.WriteAsync(' ');
                }
                await DumpCell(column, column.Heading, idx + 1 == Columns.Length);
            }
            await Console.Out.WriteLineAsync();

            for (var idx = 0;idx < Columns.Length;++idx) {
                var column = Columns[idx];
                if(idx > 0) {
                    await Console.Out.WriteAsync(' ');
                }
                for(var underlineIdx = 0;underlineIdx < column.Width;++underlineIdx) {
                    await Console.Out.WriteAsync('-');
                }
            }
            await Console.Out.WriteLineAsync();
        }

        public async Task DumpBody(IEnumerable<T> rows)
        {
            foreach(var row in rows.Where(row => row != null)) {
                for(var idx = 0;idx < Columns.Length;++idx) {
                    if(idx > 0) {
                        await Console.Out.WriteAsync(' ');
                    }
                    await DumpCell(Columns[idx], CellExtractors[idx](row), idx + 1 == Columns.Length);
                }
                await Console.Out.WriteLineAsync();
            }
        }

        private async Task DumpCell(Column column, string content, bool lastCell)
        {
            content = (content ?? "").TruncateAt(column.Width);
            var shortfall = column.Width - content.Length;

            async Task<int> pad(int length)
            {
                for(var idx = 0;idx < length;++idx) {
                    await Console.Out.WriteAsync(' ');
                }
                return length;
            }

            switch(column.Alignment) {
                case Alignment.Centre:  shortfall -= await pad(shortfall / 2); break;
                case Alignment.Right:   await pad(shortfall); break;
            }
            await Console.Out.WriteAsync(content);
            if(!lastCell) {
                switch(column.Alignment) {
                    case Alignment.Centre:
                    case Alignment.Left:
                        await pad(shortfall);
                        break;
                }
            }
        }
    }
}
