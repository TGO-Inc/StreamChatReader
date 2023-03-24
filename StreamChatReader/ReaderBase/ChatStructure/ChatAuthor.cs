using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamingServices.Chat
{
    public class ChatAuthor
    {
        public string Name { get; init; }
        public string ChannelId { get; init; }
        public bool IsMember { get; init; }
        public bool IsModerator { get; init; }
        public int MemberLevel { get; init; }
        public TimeSpan MemberDuration { get; init; }
        /// <summary>
        /// New Chat Author
        /// </summary>
        /// <param name="n">Name</param>
        /// <param name="cid">Channel Id</param>
        /// <param name="s">Is Member</param>
        /// <param name="l">Member Level</param>
        /// <param name="m">Is Moderator</param>
        /// <param name="md">Member Duration</param>
        public ChatAuthor(string n, string cid, bool s, int l, bool m, TimeSpan md)
        {
            this.Name = n;
            this.ChannelId = cid;
            this.IsMember = s;
            this.MemberLevel = l;
            this.IsModerator = m;
            this.MemberDuration = md;
        }
        public ChatAuthor(string n, string cid, IEnumerable<string> memberInfo)
        {
            this.Name = n;
            this.ChannelId = cid;
            foreach (string mInfo in memberInfo)
            {
                string info = mInfo.ToLower();
                if (info.Length > 2)
                {
                    if (info.Contains("member"))
                    {
                        this.IsMember = true;
                        this.MemberLevel = 0;
                        if (info.IndexOf("new") == -1)
                        {
                            double multiplyer = 1;
                            List<string> _mInfo = info.Replace("(", "").Replace(")", "").Split(' ').ToList();
                            _mInfo.ForEach(_ => _ = _.ToLower());
                            if (_mInfo[2].Contains("month")) multiplyer = 30.0;
                            if (_mInfo[2].Contains("year")) multiplyer = 365.0;
                            this.MemberDuration = TimeSpan.FromDays(double.Parse(_mInfo[1]) * multiplyer);
                        }
                    }
                    if (info.IndexOf("moderator") >= 0) this.IsModerator = true;
                }
            }
        }
    }
}
