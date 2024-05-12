using System.Text;

namespace VirtualRadar.Extensions
{
    public static class StringBuilderExtensions
    {
        /// <summary>
        /// Adds padding to the buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="count"></param>
        /// <param name="character"></param>
        public static void AppendMany(this StringBuilder buffer, int count, char character)
        {
            for(var idx = 0;idx < count;++idx) {
                buffer.Append(character);
            }
        }

        /// <summary>
        /// Adds padding to the buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="count"></param>
        /// <param name="text></param>
        public static void AppendMany(this StringBuilder buffer, int count, string text)
        {
            for(var idx = 0;idx < count;++idx) {
                buffer.Append(text);
            }
        }

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, string separator, object value)
        {
            AppendSeparator(builder, separator);
            return builder.Append(value);
        }

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, bool condition, string separator, object value)
            => !condition ? builder : builder.AppendWithSeparator(separator, value);

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, string separator, char[] value)
        {
            AppendSeparator(builder, separator);
            return builder.Append(value);
        }

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, bool condition, string separator, char[] value)
            => !condition ? builder : builder.AppendWithSeparator(separator, value);

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, string separator, ulong value)
        {
            AppendSeparator(builder, separator);
            return builder.Append(value);
        }

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, bool condition, string separator, ulong value)
            => !condition ? builder : builder.AppendWithSeparator(separator, value);

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, string separator, uint value)
        {
            AppendSeparator(builder, separator);
            return builder.Append(value);
        }

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, bool condition, string separator, uint value)
            => !condition ? builder : builder.AppendWithSeparator(separator, value);

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, string separator, ushort value)
        {
            AppendSeparator(builder, separator);
            return builder.Append(value);
        }

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, bool condition, string separator, ushort value)
            => !condition ? builder : builder.AppendWithSeparator(separator, value);

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, string separator, decimal value)
        {
            AppendSeparator(builder, separator);
            return builder.Append(value);
        }

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, bool condition, string separator, decimal value)
            => !condition ? builder : builder.AppendWithSeparator(separator, value);

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, string separator, double value)
        {
            AppendSeparator(builder, separator);
            return builder.Append(value);
        }

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, bool condition, string separator, double value)
            => !condition ? builder : builder.AppendWithSeparator(separator, value);

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, string separator, float value)
        {
            AppendSeparator(builder, separator);
            return builder.Append(value);
        }

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, bool condition, string separator, float value)
            => !condition ? builder : builder.AppendWithSeparator(separator, value);

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, string separator, int value)
        {
            AppendSeparator(builder, separator);
            return builder.Append(value);
        }

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, bool condition, string separator, int value)
            => !condition ? builder : builder.AppendWithSeparator(separator, value);

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, string separator, short value)
        {
            AppendSeparator(builder, separator);
            return builder.Append(value);
        }

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, bool condition, string separator, short value)
            => !condition ? builder : builder.AppendWithSeparator(separator, value);

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, string separator, char value)
        {
            AppendSeparator(builder, separator);
            return builder.Append(value);
        }

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, bool condition, string separator, char value)
            => !condition ? builder : builder.AppendWithSeparator(separator, value);

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, string separator, long value)
        {
            AppendSeparator(builder, separator);
            return builder.Append(value);
        }

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, bool condition, string separator, long value)
            => !condition ? builder : builder.AppendWithSeparator(separator, value);

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, string separator, sbyte value)
        {
            AppendSeparator(builder, separator);
            return builder.Append(value);
        }

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, bool condition, string separator, sbyte value)
            => !condition ? builder : builder.AppendWithSeparator(separator, value);

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, string separator, bool value)
        {
            AppendSeparator(builder, separator);
            return builder.Append(value);
        }

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, bool condition, string separator, bool value)
            => !condition ? builder : builder.AppendWithSeparator(separator, value);

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, string separator, string value, int startIndex, int count)
        {
            AppendSeparator(builder, separator);
            return builder.Append(value, startIndex, count);
        }

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, bool condition, string separator, string value, int startIndex, int count)
            => !condition ? builder : builder.AppendWithSeparator(separator, value, startIndex, count);

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, string separator, char[] value, int startIndex, int count)
        {
            AppendSeparator(builder, separator);
            return builder.Append(value, startIndex, count);
        }

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, bool condition, string separator, char[] value, int startIndex, int count)
            => !condition ? builder : builder.AppendWithSeparator(separator, value, startIndex, count);

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, string separator, char value, int repeatCount)
        {
            AppendSeparator(builder, separator);
            return builder.Append(value, repeatCount);
        }

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, bool condition, string separator, char value, int repeatCount)
            => !condition ? builder : builder.AppendWithSeparator(separator, value, repeatCount);

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, string separator, byte value)
        {
            AppendSeparator(builder, separator);
            return builder.Append(value);
        }

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, bool condition, string separator, byte value)
            => !condition ? builder : builder.AppendWithSeparator(separator, value);

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, string separator, string value)
        {
            AppendSeparator(builder, separator);
            return builder.Append(value);
        }

        public static StringBuilder AppendWithSeparator(this StringBuilder builder, bool condition, string separator, string value)
            => !condition ? builder : builder.AppendWithSeparator(separator, value);

        public static StringBuilder AppendFormatWithSeparator(this StringBuilder builder, string separator, string format, object arg0)
        {
            AppendSeparator(builder, separator);
            return builder.AppendFormat(format, arg0);
        }

        public static StringBuilder AppendFormatWithSeparator(this StringBuilder builder, bool condition, string separator, string format, object arg0)
            => !condition ? builder : builder.AppendFormatWithSeparator(separator, format, arg0);

        public static StringBuilder AppendFormatWithSeparator(this StringBuilder builder, string separator, string format, object arg0, object arg1)
        {
            AppendSeparator(builder, separator);
            return builder.AppendFormat(format, arg0, arg1);
        }

        public static StringBuilder AppendFormatWithSeparator(this StringBuilder builder, bool condition, string separator, string format, object arg0, object arg1)
            => !condition ? builder : builder.AppendFormatWithSeparator(separator, format, arg0, arg1);

        public static StringBuilder AppendFormatWithSeparator(this StringBuilder builder, string separator, string format, object arg0, object arg1, object arg2)
        {
            AppendSeparator(builder, separator);
            return builder.AppendFormat(format, arg0, arg1, arg2);
        }

        public static StringBuilder AppendFormatWithSeparator(this StringBuilder builder, bool condition, string separator, string format, object arg0, object arg1, object arg2)
            => !condition ? builder : builder.AppendFormatWithSeparator(separator, format, arg0, arg1, arg2);

        public static StringBuilder AppendFormatWithSeparator(this StringBuilder builder, string separator, IFormatProvider provider, string format, params object[] args)
        {
            AppendSeparator(builder, separator);
            return builder.AppendFormat(provider, format, args);
        }

        public static StringBuilder AppendFormatWithSeparator(this StringBuilder builder, bool condition, string separator, IFormatProvider provider, string format, params object[] args)
            => !condition ? builder : builder.AppendFormatWithSeparator(separator, provider, format, args);

        public static StringBuilder AppendFormatWithSeparator(this StringBuilder builder, string separator, string format, params object[] args)
        {
            AppendSeparator(builder, separator);
            return builder.AppendFormat(format, args);
        }

        public static StringBuilder AppendFormatWithSeparator(this StringBuilder builder, bool condition, string separator, string format, params object[] args)
            => !condition ? builder : builder.AppendFormatWithSeparator(separator, format, args);

        public static StringBuilder AppendLineWithSeparator(this StringBuilder builder, string separator, string value)
        {
            AppendSeparator(builder, separator);
            return builder.AppendLine(value);
        }

        public static StringBuilder AppendLineWithSeparator(this StringBuilder builder, bool condition, string separator, string value)
            => !condition ? builder : builder.AppendLineWithSeparator(separator, value);

        public static StringBuilder AppendLineWithSeparator(this StringBuilder builder, string separator)
        {
            AppendSeparator(builder, separator);
            return builder.AppendLine();
        }

        public static StringBuilder AppendIfMissing(this StringBuilder builder, char appendCharacter)
        {
            if(builder.Length > 0 && builder[^1] != appendCharacter) {
                builder.Append(appendCharacter);
            }
            return builder;
        }

        public static StringBuilder AppendLineWithSeparator(this StringBuilder builder, bool condition, string separator)
            => !condition ? builder : builder.AppendLineWithSeparator(separator);

        public static int IndexOf(this StringBuilder builder, string value, int startIndex, int count, bool caseInsensitive)
        {
            if(value == null) {
                throw new ArgumentNullException(nameof(value));
            }
            if(startIndex < 0) {
                throw new ArgumentOutOfRangeException($"{nameof(startIndex)} cannot be negative");
            }
            if(count < 0) {
                throw new ArgumentOutOfRangeException($"{nameof(count)} cannot be negative");
            }
            if(count > (builder.Length - startIndex)) {
                throw new ArgumentOutOfRangeException($"{nameof(count)} cannot be larger than StringBuilder.Length - {nameof(startIndex)}");
            }

            var result = -1;

            if(value.Length > 0) {
                var searchFor = caseInsensitive ? value.ToLower() : value;
                var searchForIdx = 0;

                for(var i = startIndex;i < count && searchForIdx < value.Length;++i) {
                    var builderCh = caseInsensitive ? Char.ToLower(builder[i]) : builder[i];
                    var searchForCh = searchFor[searchForIdx];

                    if(builderCh != searchForCh) {
                        if(result != -1) {
                            i = result + 1;
                            result = -1;
                            searchForIdx = 0;
                        }
                    } else {
                        if(result == -1) {
                            result = i;
                        }
                        ++searchForIdx;
                    }
                }

                if(searchForIdx != value.Length) {
                    result = -1;
                }
            }

            return result;
        }

        public static int IndexOf(this StringBuilder builder, String value, int startIndex, bool caseInsensitive) => IndexOf(builder, value, startIndex, builder.Length - startIndex, caseInsensitive);

        public static int IndexOf(this StringBuilder builder, String value, int startIndex, int count) => IndexOf(builder, value, startIndex, count, caseInsensitive: false);

        public static int IndexOf(this StringBuilder builder, String value) => IndexOf(builder, value, 0, builder.Length, caseInsensitive: false);

        public static int IndexOf(this StringBuilder builder, String value, int startIndex) => IndexOf(builder, value, startIndex, builder.Length - startIndex, caseInsensitive: false);

        public static int IndexOf(this StringBuilder builder, String value, bool caseInsensitive) => IndexOf(builder, value, 0, builder.Length, caseInsensitive);

        public static int LastIndexOf(this StringBuilder builder, char ch) => builder.Length == 0 ? -1 : LastIndexOf(builder, ch, builder.Length - 1);

        public static int LastIndexOf(this StringBuilder builder, char ch, int startIdx)
        {
            if(startIdx < 0) {
                throw new IndexOutOfRangeException($"Start index {startIdx} cannot be negative");
            }

            var result = -1;

            for(var idx = startIdx;idx >= 0;--idx) {
                if(builder[idx] == ch) {
                    result = idx;
                    break;
                }
            }

            return result;
        }

        public static string LastLine(this StringBuilder builder)
        {
            var lastNewLine = builder.Length == 0
                ? -1
                : builder.LastIndexOf('\n', builder[builder.Length - 1] == '\n' && builder.Length > 1
                      ? builder.Length - 2
                      : builder.Length - 1
                  );

            var result = lastNewLine == -1
                ? builder.ToString()
                : builder.Substring(lastNewLine + 1);

            return result.Trim('\r', '\n');
        }

        public static string Substring(this StringBuilder builder, int start, int length = -1)
        {
            var result = new StringBuilder();

            if(start > builder.Length) {
                throw new IndexOutOfRangeException($"Cannot start at {start}, the text is only {builder.Length}");
            }
            if(length == -1) {
                length = builder.Length - start;
            }
            if(length < 0) {
                throw new IndexOutOfRangeException($"Length ({length}) cannot be negative");
            }
            if(start + length > builder.Length) {
                throw new IndexOutOfRangeException($"A start of {start} running for {length} characters exceeds the length ({builder.Length}) of the string");
            }
            for(var idx = start;result.Length < length;++idx) {
                result.Append(builder[idx]);
            }

            return result.ToString();
        }

        /// <summary>
        /// Replaces characters that some monospaced fonts will not render at the same width as all of
        /// the other characters.
        /// </summary>
        /// <param name="buffer"></param>
        public static void NormaliseMonospaced(this StringBuilder buffer)
        {
            for(var idx = 0;idx < buffer.Length;++idx) {
                switch(buffer[idx]) {
                    case ' ':
                        // Narrow Non-Breaking Space -> Non-Breaking Space
                        buffer[idx] = Format.Characters.NonBreakingSpace;
                        break;
                }
            }
        }

        /// <summary>
        /// Appends the separator to the string builder if it already has content, does nothing
        /// if it does not.
        /// </summary>
        /// <param name="stringBuilder"></param>
        /// <param name="separator"></param>
        private static void AppendSeparator(StringBuilder stringBuilder, string separator)
        {
            if((stringBuilder?.Length ?? 0) > 0) {
                stringBuilder.Append(separator);
            }
        }
    }
}
