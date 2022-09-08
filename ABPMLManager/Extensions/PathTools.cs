namespace ABPMLManager.Extensions
{
    public static class PathTools
    {
        const char slashWin = '\\';
        const char slashLin = '/';

        static public ReadOnlySpan<char> DirectoryGoUp(string path)
        {
            var result = path.AsSpan();
            if (!string.Equals(path, Path.GetPathRoot(path), StringComparison.OrdinalIgnoreCase))
            {
                var span = StripEndSlash(path);
                int i = span.Length - 1;
                while (i > 0 && span[i] != slashWin && span[i] != slashLin)
                    i--;

                result = span[0..(i+1)];
            }
            return result;
        }

        static public ReadOnlySpan<char> StripEndSlash(string path) => StripEndSlash(path.AsSpan());

        static public ReadOnlySpan<char> StripEndSlash(ReadOnlySpan<char> path)
        {
            int s = path.Length - 1;
            while (s > 0 && path[s] == slashLin || path[s] == slashWin)
                s--;

            return s == 0 ? string.Empty : path[0..(s+1)];
        }

        static public ReadOnlySpan<char> StripForwardSlash(string path) => StripForwardSlash(path.AsSpan());

        static public ReadOnlySpan<char> StripForwardSlash(ReadOnlySpan<char> path)
        {
            int s = 0;
            while (s < path.Length && path[s] == slashLin || path[s] == slashWin)
                s++;

            return s == path.Length ? string.Empty : path[s..];
        }
    }
}
