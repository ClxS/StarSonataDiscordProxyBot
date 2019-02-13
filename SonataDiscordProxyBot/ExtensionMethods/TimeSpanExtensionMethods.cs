namespace SonataDiscordProxyBot.ExtensionMethods
{
    using System;
    using System.Text;

    public static class TimeSpanExtensionMethods
    {
        public static string ToPrettyFormat(this TimeSpan span)
        {
            if (span == TimeSpan.Zero)
            {
                return "0 minutes";
            }

            var sb = new StringBuilder();
            if (span.Days > 0)
            {
                sb.AppendFormat("{0} day{1} ", span.Days, span.Days > 1 ? "s" : string.Empty);
            }

            if (span.Hours > 0)
            {
                sb.AppendFormat("{0} hour{1} ", span.Hours, span.Hours > 1 ? "s" : string.Empty);
            }

            if (span.Minutes > 0)
            {
                sb.AppendFormat("{0} minute{1} ", span.Minutes, span.Minutes > 1 ? "s" : string.Empty);
            }

            if (span.Seconds > 0)
            {
                sb.AppendFormat("{0} second{1} ", span.Seconds, span.Seconds > 1 ? "s" : string.Empty);
            }

            if (sb.Length == 0)
            {
                sb.Append("< 1 second");
            }

            return sb.ToString();
        }
    }
}
