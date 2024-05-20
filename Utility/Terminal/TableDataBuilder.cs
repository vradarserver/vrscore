// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using Terminal.Gui;

namespace VirtualRadar.Utility.Terminal
{
    public class TableDataBuilder<T>
    {
        public enum CellAlignment
        {
            Left,
            Centre,
            Right,
            Justified,
        }

        public class ColumnDefinition
        {
            public Expression<Func<T, object>> PropertyExpression { get; }

            public PropertyInfo PropertyInfo { get; }

            public string DisplayName { get; }

            public bool? OverrideNullable { get; }

            public Func<T, object> GetValueFunc { get; }

            public Type ColumnType { get; }

            public TextAlignment CellAlignment { get; set; } = TextAlignment.Left;

            public string Format { get; set; }

            /// <summary>
            /// Won't work until Terminal.Gui v2 is released, use NullSymbol at the table level instead.
            /// </summary>
            public string NullFormat { get; set; } = "-";

            public ColumnDefinition(
                Expression<Func<T, object>> propertyExpression,
                string displayName = null
            )
            {
                PropertyExpression = propertyExpression;
                PropertyInfo = ExpressionHelper.ExtractPropertyInfo(propertyExpression);
                DisplayName = displayName ?? PropertyInfo.Name;
                ColumnType = PropertyInfo.PropertyType;
                GetValueFunc = propertyExpression.Compile();

                var underlyingNullableType = Nullable.GetUnderlyingType(ColumnType);
                if(underlyingNullableType != null) {
                    ColumnType = underlyingNullableType;
                    OverrideNullable = true;
                }
            }
        }

        public List<ColumnDefinition> ColumnDefinitions { get; } = new();

        public string Name { get; }

        public TableDataBuilder(string name = null)
        {
            Name = name ?? typeof(T).Name;
        }

        public DataTable BuildDataTable(
            IEnumerable<T> enumeration,
            string name = null
        )
        {
            var result = new DataTable(name ?? Name);

            foreach(var coldef in ColumnDefinitions) {
                var column = new DataColumn(
                    coldef.DisplayName,
                    coldef.ColumnType
                );
                if(coldef.OverrideNullable != null) {
                    column.AllowDBNull = coldef.OverrideNullable.Value;
                }
                result.Columns.Add(column);
            }

            foreach(var row in enumeration) {
                var rowColumns = new object[result.Columns.Count];
                for(var colIdx = 0;colIdx < result.Columns.Count;++colIdx) {
                    var coldef = ColumnDefinitions[colIdx];
                    rowColumns[colIdx] = coldef.GetValueFunc(row);
                }
                result.Rows.Add(rowColumns);
            }

            return result;
        }

        public void ApplyStyling(TableView tableView)
        {
            for(var colIdx = 0;colIdx < ColumnDefinitions.Count;++colIdx) {
                var coldef = ColumnDefinitions[colIdx];
                var cellStyle = tableView
                    .Style
                    .GetOrCreateColumnStyle(tableView.Table.Columns[colIdx]);
                cellStyle.Alignment = coldef.CellAlignment;

                if(coldef.ColumnType == typeof(bool) || coldef.ColumnType == typeof(bool?)) {
                    cellStyle.RepresentationGetter = v => v == null ? coldef.NullFormat : Object.Equals(v, true) ? "Yes" : "No";
                } else if(coldef.Format != null) {
                    cellStyle.RepresentationGetter = v => v == null ? coldef.NullFormat : String.Format(coldef.Format, v);
                }
            }
        }
    }
}
