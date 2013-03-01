using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Misterhex
{
    public class TweetInformation : IEqualityComparer<TweetInformation>
    {
        public string Content { get; set; }
        public string Name { get; set; }
        public string TimeZone { get; set; }
        public string CreatedAt { get; set; }
        public bool IsContentContainingURL
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(this.Content) && this.Content.Contains(@"http://"))
                {
                    return true;
                }
                return false;
            }
        }

        public string FormattedContent
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.Content)) return this.Content;

                StringBuilder sb = new StringBuilder();

                foreach (var section in this.Content.Split(' '))
                {
                    if (section.StartsWith("http://"))
                    {
                        sb.Append(@"<a href='" + section + "'></a> ");
                    }
                    else
                    {
                        sb.Append(section + ' ');
                    }
                }
                var result = sb.ToString().TrimEnd();
                return result;

            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var property in this.GetType().GetProperties().ToList())
            {
                sb.AppendLine(string.Format("{0} : {1}", property.Name, property.GetValue(this, null)));
            }

            return sb.ToString();
        }

        public override int GetHashCode()
        {
            string content = this.Content != null ? this.Content : string.Empty;
            string name = this.Name != null ? this.Name : string.Empty;
            string timezone = this.TimeZone != null ? this.TimeZone : string.Empty;
            string createdAt = this.CreatedAt != null ? this.CreatedAt : string.Empty;

            int hash = 17;
            hash = hash * 31 + content.GetHashCode();
            hash = hash * 31 + name.GetHashCode();
            hash = hash * 31 + timezone.GetHashCode();
            hash = hash * 31 + createdAt.GetHashCode();
            return hash;
        }

        public bool Equals(TweetInformation x, TweetInformation y)
        {
            return x.GetHashCode() == y.GetHashCode();
        }

        public int GetHashCode(TweetInformation obj)
        {
            return obj.GetHashCode();
        }

        public string ToJSON()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}
