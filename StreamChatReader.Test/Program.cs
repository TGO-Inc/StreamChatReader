using StreamingServices;
using StreamingServices.Chat;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;

double money = 0;

ChatReader ChatReader = new();
ChatReader.ChatEvent += ChatReader_ChatEvent;
ChatReader.ChatLoaded += ChatReader_ChatLoaded;

void ChatReader_ChatLoaded(ChatLoadedArgs e)
{
    Debug.WriteLine("Loaded/Linked");
}

void ChatReader_ChatEvent(ChatEventArgs e)
{
    Console.WriteLine(e.Message);
}

ChatReader.AddYoutubeStream("etyCb38ag-s");

while(true)
    Console.ReadKey();