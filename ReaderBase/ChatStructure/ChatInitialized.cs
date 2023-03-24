using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamingServices.Chat
{
    public delegate void ChatLoadedHandler(ChatLoadedArgs e);
    public class ChatLoadedArgs : EventArgs
    {
    }
}
