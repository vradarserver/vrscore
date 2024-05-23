// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace WindowProcessor
{
    public class Table<T>
    {
        public Window Window { get; private set; }

        public Point HeadingTopLeft { get; private set; }

        public Point BodyTopLeft { get; private set; }

        public BorderOptions OuterBorder { get; }

        public bool HasOuterHorizontal => OuterBorder.HasHorizontalBorder;

        public bool HasOuterVertical => OuterBorder.HasVerticalBorder;

        public BorderOptions InnerBorder { get; }

        public bool HasHeadingRuleoff => InnerBorder.HasHorizontalBorder;

        public bool HasColumnDividers => InnerBorder.HasVerticalBorder;

        public BorderOptions TopBottomJunction { get; }

        public BorderOptions LeftRightJunction { get; }

        public Column[] Columns { get; }

        public Func<T, string[]> ExtractCells { get; }

        public FBColors? OuterBorderColors { get; }

        public FBColors? InnerBorderColors { get; }

        public FBColors? HeadingColors { get; }

        public FBColors? BodyColors { get; }

        public Table(
            Column[] columns,
            Func<T, string[]> extractCells,
            BorderOptions outer,
            BorderOptions inner,
            FBColors? outerColors = null,
            FBColors? innerColors = null,
            FBColors? headingColors = null,
            FBColors? bodyColors = null
        )
        {
            Columns = columns;
            ExtractCells = extractCells;
            OuterBorder = outer;
            InnerBorder = inner;
            TopBottomJunction = new(outer.HorizontalStyle, inner.VerticalStyle);
            LeftRightJunction = new(inner.HorizontalStyle, outer.VerticalStyle);

            OuterBorderColors = outerColors;
            InnerBorderColors = innerColors;
            HeadingColors = headingColors;
            BodyColors = bodyColors;
        }

        public void DrawHeadingInto(Window window)
        {
            Window = window;
            HeadingTopLeft = Point.Current;

            var linesOutput = 0;
            var resetColors = FBColors.Current;
            if(HasOuterHorizontal) {
                (OuterBorderColors ?? resetColors).Apply();
                Window.Write(OuterBorder.TopLeft);
                for(var idx = 0;idx < Columns.Length;++idx) {
                    Window.Write(OuterBorder.Horizontal, Columns[idx].Width);
                    if(idx + 1 < Columns.Length) {
                        Window.Write(TopBottomJunction.TopJunction);
                    }
                }
                Window.Write(OuterBorder.TopRight);
                ++linesOutput;
            }

            Window.Position = HeadingTopLeft.Down(linesOutput);
            if(HasOuterVertical) {
                (OuterBorderColors ?? resetColors).Apply();
                Window.Write(OuterBorder.Vertical);
            }
            for(var idx = 0;idx < Columns.Length;++idx) {
                var column = Columns[idx];
                (HeadingColors ?? resetColors).Apply();
                Window.WriteField(column.Heading, column.Width, column.Alignment);
                if(idx + 1 < Columns.Length) {
                    (InnerBorderColors ?? resetColors).Apply();
                    Window.Write(HasColumnDividers ? InnerBorder.Vertical : ' ');
                }
            }
            if(HasOuterVertical) {
                (OuterBorderColors ?? resetColors).Apply();
                Window.Write(OuterBorder.Vertical);
            }
            ++linesOutput;

            Window.Position = HeadingTopLeft.Down(linesOutput);
            if(HasHeadingRuleoff) {
                if(HasOuterVertical) {
                    (OuterBorderColors ?? resetColors).Apply();
                    Window.Write(LeftRightJunction.LeftJunction);
                }
                (InnerBorderColors ?? resetColors).Apply();
                for(var idx = 0;idx < Columns.Length;++idx) {
                    var column = Columns[idx];
                    Window.Write(InnerBorder.Horizontal, column.Width);
                    if(idx + 1 < Columns.Length) {
                        Window.Write(HasColumnDividers ? InnerBorder.Crossroads : ' ');
                    }
                }
                if(HasOuterVertical) {
                    (OuterBorderColors ?? resetColors).Apply();
                    Window.Write(LeftRightJunction.RightJunction);
                }
            }
            ++linesOutput;
            resetColors.Apply();

            BodyTopLeft = HeadingTopLeft.Down(linesOutput);
        }

        public void DrawBody(IEnumerable<T> rows, int countDisplayRows)
        {
            var resetColors = FBColors.Current;

            using(var enumerator = rows.GetEnumerator()) {
                var hasRow = true;

                var actualDisplayRows = HasOuterHorizontal
                    ? countDisplayRows - 1
                    : countDisplayRows;

                for(var displayRow = 0;displayRow < actualDisplayRows;++displayRow) {
                    hasRow = hasRow && enumerator.MoveNext();
                    var row = hasRow ? enumerator.Current : default;
                    var cells = hasRow ? ExtractCells(row) : null;

                    Window.Position = BodyTopLeft.Down(displayRow);
                    if(HasOuterVertical) {
                        (OuterBorderColors ?? resetColors).Apply();
                        Window.Write(OuterBorder.Vertical);
                    }
                    for(var idx = 0;idx < Columns.Length;++idx) {
                        var column = Columns[idx];
                        (BodyColors ?? resetColors).Apply();
                        Window.WriteField(cells == null ? "" : cells[idx], column.Width, column.Alignment);
                        if(idx + 1 < Columns.Length) {
                            (InnerBorderColors ?? resetColors).Apply();
                            Window.Write(HasColumnDividers ? InnerBorder.Vertical : ' ');
                        }
                    }
                    if(HasOuterVertical) {
                        (OuterBorderColors ?? resetColors).Apply();
                        Window.Write(OuterBorder.Vertical);
                    }
                }

                if(HasOuterHorizontal) {
                    (OuterBorderColors ?? resetColors).Apply();
                    Window.Position = BodyTopLeft.Down(countDisplayRows - 1);
                    Window.Write(OuterBorder.BottomLeft);
                    for(var idx = 0;idx < Columns.Length;++idx) {
                        Window.Write(OuterBorder.Horizontal, Columns[idx].Width);
                        if(idx + 1 < Columns.Length) {
                            Window.Write(TopBottomJunction.BottomJunction);
                        }
                    }
                    Window.Write(OuterBorder.BottomRight);
                }
            }

            resetColors.Apply();
        }
    }
}
