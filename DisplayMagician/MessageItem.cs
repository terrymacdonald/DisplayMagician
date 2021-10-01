using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayMagician
{
    class MessageItem
    {
        public int Id
        { get; set; }

        public string MessageMode
        { get; set; } = "txt";
        
        public string MinVersion
        { get; set; }

        public string MaxVersion
        { get; set; }

        public string StartDate
        { get; set; } = null;

        public string EndDate
        { get; set; } = null;

        public string HeadingText
        { get; set; } = "DisplayMagician Message";

        public string ButtonText
        { get; set; } = "&Close";

        public string Url
        { get; set; } = null;
    }
}
