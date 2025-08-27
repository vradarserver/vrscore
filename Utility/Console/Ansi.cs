namespace VirtualRadar.Utility.CLIConsole
{
    public static class Ansi
    {
        public static readonly Chunk Blue = new(ConsoleColor.DarkBlue, ConsoleColor.Black);
        public static readonly Chunk BlueBold = new(ConsoleColor.Blue, ConsoleColor.Black);
        public static readonly Chunk Cyan = new(ConsoleColor.DarkCyan, ConsoleColor.Black);
        public static readonly Chunk CyanBold = new(ConsoleColor.Cyan, ConsoleColor.Black);
        public static readonly Chunk Red = new(ConsoleColor.DarkRed, ConsoleColor.Black);
        public static readonly Chunk RedBold = new(ConsoleColor.Red, ConsoleColor.Black);
        public static readonly Chunk Magenta = new(ConsoleColor.DarkMagenta, ConsoleColor.Black);
        public static readonly Chunk MagentaBold = new(ConsoleColor.Magenta, ConsoleColor.Black);
        public static readonly Chunk Yellow = new(ConsoleColor.DarkYellow, ConsoleColor.Black);
        public static readonly Chunk YellowBold = new(ConsoleColor.Yellow, ConsoleColor.Black);
        public static readonly Chunk White = new(ConsoleColor.Gray, ConsoleColor.Black);
        public static readonly Chunk WhiteBold = new(ConsoleColor.White, ConsoleColor.Black);
        public static readonly Chunk Regular = White;

        public class Chunk
        {
            public string Text { get; }

            public ConsoleColor? Foreground { get; }

            public ConsoleColor? Background { get; }

            public Chunk(string text)
            {
                Text = text;
            }

            public Chunk(ConsoleColor? foreground, ConsoleColor? background)
            {
                Foreground = foreground;
                Background = background;
            }

            public Chunk() : this(null, null)
            {
            }

            public Chunk(ConsoleColor foreground) : this(foreground, null)
            {
            }

            public static implicit operator Chunk(string text) => new(text);
        }

        public static void WriteLine(params Chunk[] chunks)
        {
            if(!Console.IsOutputRedirected) {
                Emit(chunks, text => Console.Write(text), () => Console.WriteLine());
            } else {
                EmitText(chunks, text => Console.Write(text), () => Console.WriteLine());
            }
        }

        public static void Emit(Chunk[] chunks, Action<string> emitTextAction, Action endOfSequenceAction)
        {
            var initialForeground = Console.ForegroundColor;
            var initialBackground = Console.BackgroundColor;
            var currentForeground = initialForeground;
            var currentBackground = initialBackground;

            void setForeground(ConsoleColor? colour)
            {
                if(colour != null && colour.Value != currentForeground) {
                    currentForeground = colour.Value;
                    Console.ForegroundColor = colour.Value;
                }
            }
            void setBackground(ConsoleColor? colour)
            {
                if(colour != null && colour.Value != currentBackground) {
                    currentBackground = colour.Value;
                    Console.BackgroundColor = colour.Value;
                }
            }

            try {
                foreach(var chunk in chunks) {
                    if(chunk.Text != null) {
                        emitTextAction(chunk.Text);
                    } else {
                        if(chunk.Foreground == null && chunk.Background == null) {
                            setForeground(initialForeground);
                            setBackground(initialBackground);
                        } else {
                            setForeground(chunk.Foreground);
                            setBackground(chunk.Background);
                        }
                    }
                }
            } finally {
                setForeground(initialForeground);
                setBackground(initialBackground);
            }

            endOfSequenceAction();
        }

        public static void EmitText(Chunk[] chunks, Action<string> emitTextAction, Action endOfSequenceAction)
        {
            foreach(var chunk in chunks.Where(c => c.Text != null)) {
                emitTextAction(chunk.Text);
            }
            endOfSequenceAction();
        }
    }
}
